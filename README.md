# API Health Intelligence Platform

> An AI-powered distributed system that monitors APIs, detects anomalies,
> and generates intelligent health summaries — built on Azure, .NET, Python and Claude AI.

![Build Status](https://github.com/AnshathAli/api-health-platform/actions/workflows/ci.yml/badge.svg)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Azure](https://img.shields.io/badge/Azure-Functions%20%7C%20Service%20Bus%20%7C%20SQL-blue)
![Python](https://img.shields.io/badge/Python-3.11-yellow)

---

## What this does

This platform ingests API diagnostic events, processes them through a .NET worker
service, caches metrics in Redis, and uses the Claude AI API (via a Python Azure
Function) to generate human-readable health summaries — all surfaced through a
REST API and a simple HTML dashboard.

---

## Architecture
```
APIM / APIs → Azure Function (ingest) → Service Bus
                                              ↓
                                     .NET Core Worker
                                      ↙            ↘
                                 Azure SQL        Redis
                                              ↓
                              Python Function + Claude API
                                              ↓
                               ASP.NET Web API + Dashboard
```

---

## Tech stack

| Layer | Technology |
|---|---|
| Event ingestion | Azure Functions (C#) |
| Message queue | Azure Service Bus |
| Processing | .NET 8 Worker Service |
| Persistence | Azure SQL (serverless) |
| Caching | Azure Cache for Redis |
| AI integration | Python Azure Function + Claude API |
| REST API | ASP.NET Core Web API |
| Frontend | HTML, CSS, vanilla JS |
| CI/CD | GitHub Actions |
| Infrastructure | Azure (manual + ARM templates) |

---

## Project structure
```
api-health-platform/
├── src/
│   ├── ApiHealth.Ingestor/       # Azure Function — HTTP event collector
│   ├── ApiHealth.Worker/         # .NET Worker Service — processes events
│   ├── ApiHealth.Api/            # ASP.NET Core Web API — REST endpoints
│   ├── ApiHealth.AiProcessor/    # Python Azure Function — Claude AI integration
│   └── ApiHealth.Dashboard/      # HTML/CSS/JS frontend
├── infrastructure/               # ARM templates, Azure setup scripts
├── docs/                         # Architecture decisions, notes
└── .github/workflows/            # CI/CD pipelines
```

---

## Getting started

> Prerequisites: .NET 8 SDK, Python 3.11, Azure CLI, Azure Functions Core Tools
```bash
git clone https://github.com/YOUR_USERNAME/api-health-platform.git
cd api-health-platform
git checkout develop
```

Each component has its own setup instructions in its folder's README.

---

## Build phases

- [x] Phase 1 — GitHub repo setup, project structure, README
- [ ] Phase 2 — Azure Function + event ingestion
- [ ] Phase 3 — .NET Worker Service + Azure SQL
- [ ] Phase 4 — Redis caching layer
- [ ] Phase 5 — Python Function + Claude AI integration
- [ ] Phase 6 — ASP.NET Core Web API
- [ ] Phase 7 — HTML Dashboard
- [ ] Phase 8 — GitHub Actions CI/CD

---

## Author

**Anshath Ali Melethil** — [LinkedIn](https://www.linkedin.com/in/anshath-ali-m-50185a19b/) · [GitHub](https://github.com/AnshathAli)

> Built as a learning project to explore distributed systems, Azure cloud services,
> and AI integration — with a focus on clean architecture and real-world patterns.