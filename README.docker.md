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

Secrets
-
The repository should not contain production secrets. `JWT_KEY` is required by the API to sign JWTs.

- Local development: create a `.env` file (this repo includes `.env` in `.gitignore`) or export `JWT_KEY` in your shell. Use `.env.example` as a template.
- CI / Production: store `JWT_KEY` in your CI secrets or a secret manager (GitHub Secrets, Azure Key Vault, etc.) and inject it into the environment when building or running containers.

Example GitHub Actions snippet (set `JWT_KEY` in the repository/organization Secrets):

```yaml
env:
	POSTGRES_DB: recommendationdb
	POSTGRES_USER: recommendation_user
	POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
	JWT_KEY: ${{ secrets.JWT_KEY }}

steps:
	- name: Build and run tests
		run: |
			docker compose up --build --detach
			# run your integration tests here
```
