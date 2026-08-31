# Recommendation Platform — Architecture

## Overview
This repository implements a small recommendation-platform microservice system:

- A backend web API written in ASP.NET Core (`src/Recommendation.Api`) responsible for application logic, persistence, and exposing REST endpoints (Products, Users, UserInteractions, Recommendations proxy).
- A Python ML service using FastAPI (`src/recommendation-ai`) that generates product embeddings and returns top‑N recommendations.
- PostgreSQL as the canonical product and user interaction store.

## Components

- `Recommendation.Api` (C#/.NET):
	- `AppDbContext` — EF Core DbContext with `Products`, `Users`, `Orders`, `UserInteractions`.
	- Controllers: `ProductsController`, `UsersController`, `UserInteractionsController`, `OrdersController`, `RecommendationsController` (forwards requests to the ML service).
	- Swagger/OpenAPI is configured for local development.

- `recommendation-ai` (Python/FastAPI):
	- Loads product catalog from PostgreSQL, computes embeddings with `sentence-transformers` and returns cosine-similarity ranked top‑N results.
	- Exposes `POST /api/recommendations` and a health endpoint.

## Data Flow

1. Client calls `Recommendation.Api` (e.g., `POST /api/recommendations`).
2. `Recommendation.Api` reads products from PostgreSQL (via EF Core) and forwards the request to the Python ML service.
3. Python service encodes product text, scores by cosine similarity, and returns top‑N recommendations.
4. `Recommendation.Api` returns the response to the client.

## Local Run (dev)

1. Ensure PostgreSQL is running and seeded (see `seed-products.sql`).
2. Start Python service (from repo root):

```bash
cd src/recommendation-ai
.venv/bin/python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
```

3. Start .NET API:

```bash
dotnet run --project src/Recommendation.Api --urls http://127.0.0.1:5000
```

4. Use Swagger UI (when running) or curl to call endpoints.

## Phase Status (short)

- Phase 1 — Scaffold .NET + Python: Completed
- Phase 2 — PostgreSQL and Products: Completed (Products/Users/UserInteractions/Orders implemented with migrations and controllers)
- Phase 3 — DB-backed recommendation engine: Completed
- Phase 4 — Containerization: Completed (docker-compose brings up postgres, recommendation-ai, api, seed, api-init end-to-end; see README.docker.md)
- Phase 5 — Cloud (Azure): Not started (no IaC/deployment manifests in repo)
- Phase 6 — CI/CD: Mostly complete (.github/workflows/ci.yml runs Python + .NET tests and builds images; integration.yml runs smoke tests via GitHub Actions runner)
- Phase 7 — Production hardening: Not started (DB credentials are hardcoded in docker-compose.yml/appsettings.json instead of a secrets store; no HTTPS/prod configuration)

## Next Steps

- Seed sample users and interactions and add basic integration tests for Products/Users/Interactions endpoints.
- Containerize services and validate `docker-compose` locally.
- Move secrets out of `appsettings.json` into environment variables or a secrets store before pushing to remote.

For implementation details, see the projects under `src/Recommendation.Api` and `src/recommendation-ai`.

