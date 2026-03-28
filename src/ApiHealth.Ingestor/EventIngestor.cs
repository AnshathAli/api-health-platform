using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiHealth.Ingestor;

public class EventIngestor
{
    private readonly ILogger<EventIngestor> _logger;
    private readonly QueueClient _queueClient;

    public EventIngestor(ILogger<EventIngestor> logger, QueueClient queueClient)
    {
        _logger = logger;
        _queueClient = queueClient;
    }

    [Function("EventIngestor")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Event ingestor received a request.");

        // Deserialize the incoming payload
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            return new BadRequestObjectResult("Request body cannot be empty.");

        ApiHealthEvent? healthEvent;

        try
        {
            healthEvent = JsonSerializer.Deserialize<ApiHealthEvent>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Invalid JSON payload.");
        }

        if (healthEvent is null)
            return new BadRequestObjectResult("Could not parse event.");

        // Enqueue as base64 (Storage Queue requires base64 encoding)
        var messageJson = JsonSerializer.Serialize(healthEvent);
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(messageJson);
        var base64Message = Convert.ToBase64String(messageBytes);

        await _queueClient.CreateIfNotExistsAsync();
        await _queueClient.SendMessageAsync(base64Message);

        _logger.LogInformation("Event queued for API: {ApiName}", healthEvent.ApiName);

        return new OkObjectResult(new
        {
            message = "Event accepted and queued.",
            apiName = healthEvent.ApiName,
            timestamp = healthEvent.Timestamp
        });
    }
}

// The shape of the incoming event payload
public record ApiHealthEvent(
    string ApiName,
    int StatusCode,
    double ResponseTimeMs,
    string Region,
    DateTime Timestamp
);