using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ApiHealth.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly QueueClient _queueClient;
    private readonly string _sqlConnectionString;

    public Worker(
        ILogger<Worker> logger,
        QueueClient queueClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _queueClient = queueClient;
        _sqlConnectionString = configuration.GetConnectionString("SqlConnection")
            ?? throw new InvalidOperationException("SqlConnection not configured.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started, listening to queue...");

        await _queueClient.CreateIfNotExistsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(
                    maxMessages: 10,
                    cancellationToken: stoppingToken);

                foreach (var message in messages)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }

                // If no messages, wait 5 seconds before polling again
                if (messages.Length == 0)
                    await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown — expected when stoppingToken fires
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in worker loop.");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Worker stopped.");
    }

    private async Task ProcessMessageAsync(QueueMessage message, CancellationToken ct)
    {
        try
        {
            // Decode base64 message
            var json = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(message.MessageText));

            var healthEvent = JsonSerializer.Deserialize<ApiHealthEvent>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (healthEvent is null)
            {
                _logger.LogWarning("Could not deserialize message, skipping.");
                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                return;
            }

            // Persist to SQL
            await PersistEventAsync(healthEvent, ct);

            // Delete from queue only after successful processing
            await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);

            _logger.LogInformation(
                "Processed event for API: {ApiName}, Status: {StatusCode}, ResponseTime: {ResponseTimeMs}ms",
                healthEvent.ApiName, healthEvent.StatusCode, healthEvent.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message {MessageId}.", message.MessageId);
            // Don't delete — message will reappear after visibility timeout for retry
        }
    }

    private async Task PersistEventAsync(ApiHealthEvent healthEvent, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO ApiHealthEvents 
                (ApiName, StatusCode, ResponseTimeMs, Region, Timestamp)
            VALUES 
                (@ApiName, @StatusCode, @ResponseTimeMs, @Region, @Timestamp)
            """;

        await using var connection = new SqlConnection(_sqlConnectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ApiName", healthEvent.ApiName);
        command.Parameters.AddWithValue("@StatusCode", healthEvent.StatusCode);
        command.Parameters.AddWithValue("@ResponseTimeMs", healthEvent.ResponseTimeMs);
        command.Parameters.AddWithValue("@Region", healthEvent.Region);
        command.Parameters.AddWithValue("@Timestamp", healthEvent.Timestamp);

        await command.ExecuteNonQueryAsync(ct);
    }
}

public record ApiHealthEvent(
    string ApiName,
    int StatusCode,
    double ResponseTimeMs,
    string Region,
    DateTime Timestamp
);