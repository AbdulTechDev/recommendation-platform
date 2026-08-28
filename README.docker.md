# Docker Compose — local run

Prerequisites:
- Docker & Docker Compose installed

Start services:

```bash
docker compose up --build
```

This will start:
- `postgres` — Postgres DB (seeded automatically by `recommendation-ai` container if empty).
- `recommendation-ai` — Python ML service (waits for Postgres and runs `seed-products.sql`).
- `api-init` — runs EF migrations against Postgres (idempotent).
- `api` — .NET API (built from `src/Recommendation.Api`).

Notes:
- The `api` service depends on `api-init` implicitly via `depends_on` ordering; you can remove or modify if you prefer manual migration control.
