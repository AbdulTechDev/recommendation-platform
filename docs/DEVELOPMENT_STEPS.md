# Development Steps and Change Log

This document tracks the major steps taken during the development of the recommendation platform. Update this file whenever you complete a feature, change CI, or modify startup/run steps.

## Overview
- Python FastAPI service: `src/recommendation-ai`
- .NET API: `src/Recommendation.Api`
- Database: PostgreSQL (seeded via `seed-products.sql`)
- Orchestration: `docker-compose.yml`
- CI: `.github/workflows/ci.yml` (tests + image builds)
- Integration: `.github/workflows/integration.yml` (manual compose smoke tests)

## Change Log

### 2026-08-28 — Phase4: Docker wait+seed & CI
- Added `wait_for_postgres.py` and `seed_db.py` to `src/recommendation-ai` to wait for Postgres and seed `Products` table.
- Migrated product data from `products.json` to `seed-products.sql` at repository root.
- Updated `src/recommendation-ai/Dockerfile` to copy files with repo-root context.
- Added `src/Recommendation.Api/Dockerfile` (multi-stage) and `api-init` job in `docker-compose.yml` to run EF migrations.
- Created `scripts/smoke_test.sh` to validate local stack endpoints.
- Added GitHub Actions CI workflow `.github/workflows/ci.yml` to run Python and .NET tests and build images.
- Added integration workflow `.github/workflows/integration.yml` to run `docker compose up` on a runner and execute smoke tests.
- Multiple CI fixes: adjusted `PYTHONPATH`, explicit `dotnet restore` path, build contexts.

## How to run locally

1. Install Docker Desktop
2. Start services: `docker compose up --build`
3. Run smoke tests: `bash scripts/smoke_test.sh`

## Notes and Troubleshooting
- If Docker build fails with containerd/overlayfs errors: restart Docker Desktop, prune builders and images, increase disk image size.
- CI runs on GitHub runners and may use `buildx`; ensure `integration.yml` is run manually via Actions → Workflows → Integration Smoke Tests → Run workflow.

## Next Steps
- Add integration tests that run full end-to-end flows (product retrieval → recommendation) in CI.
- Add deployment/workflow for staging environment.
