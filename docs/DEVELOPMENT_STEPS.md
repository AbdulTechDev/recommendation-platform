## Development Steps and Complete Change Log (detailed)

This file documents every notable step taken during development from project bootstrap through the current state (including small/minor steps). Update this file as you progress.

Repository layout (important paths)
- Python service: src/recommendation-ai/
- .NET API: src/Recommendation.Api/
- Docker compose: docker-compose.yml
- Product seed SQL: src/recommendation-ai/seed-products.sql
- Seed runner: src/recommendation-ai/seed_db.py
- Postgres wait: src/recommendation-ai/wait_for_postgres.py
- CI: .github/workflows/ci.yml
- Integration workflow: .github/workflows/integration.yml
- Smoke test script: scripts/smoke_test.sh

High-level timeline (chronological, with minor steps)

1) Initial project scaffold
- Created repository and basic README.
- Added `src/Recommendation.Api` ASP.NET project with initial controllers and models (`Product`, `RecommendationRequest`, `RecommendationResponse`).
- Added `src/recommendation-ai` as a minimal FastAPI app (`app/main.py`) placeholder.

2) Product data migration and DB model
- Added `Products` model to EF Core `AppDbContext` in `src/Recommendation.Api/Data/AppDbContext.cs`.
- Created EF Core migrations and committed migration files.
- Migrated product data from `src/recommendation-ai/data/products.json` into SQL insert statements in `src/recommendation-ai/seed-products.sql`.

3) Recommendation engine prototype (local, file-backed)
- Implemented `RecommendationEngine` in `src/recommendation-ai/app/recommendation.py` using sentence-transformers embeddings for vector similarity.
- Wrote `src/recommendation-ai/app/models.py` with request/response Pydantic models and aliases for `userId`/`user_id`.
- Added unit tests `src/recommendation-ai/tests/test_recommendation.py` using a `FakeModel` to ensure deterministic behavior.
- Added lazy imports for heavy dependencies (`sentence-transformers`, `numpy`, `psycopg`) so tests run fast.

4) Postgres backing and seeding
- Switched product storage to PostgreSQL and added `seed-products.sql` for seeding.
- Implemented `wait_for_postgres.py` to block service start until Postgres is available.
- Implemented `seed_db.py` to execute `seed-products.sql` against the running database.
- Updated recommendation code to load products from Postgres rather than local JSON.

5) Dockerization and compose orchestration
- Added Dockerfiles for `src/recommendation-ai` and `src/Recommendation.Api` (multi-stage build for .NET).
- Standardized build context to repository root and updated `Dockerfile` COPY paths to reference `src/...` explicitly.
- Created `docker-compose.yml` to orchestrate `postgres`, `recommendation-ai`, `api`, and an `api-init` migration job that runs migrations before the API starts.
- Added `scripts/smoke_test.sh` to validate endpoints after containers come up.

6) CI and integration automation
- Added `.github/workflows/ci.yml` to run Python `pytest`, .NET tests, and build images using `docker/build-push-action` (buildx) on push/PR.
- Fixed CI issues: added `PYTHONPATH` so pytest can import `app` package; changed `dotnet restore` and `dotnet test` to point to the specific csproj paths.
- Created `.github/workflows/integration.yml` (manual dispatch) to run `docker compose up --build` on a GitHub runner and run `scripts/smoke_test.sh` to validate the stack when local Docker fails.

7) Branching, PRs, and releases
- Created branch `phase4-docker-seed` for Docker + seeding changes.
- Iterated with commits and PRs to fix CI and Dockerfile context issues; merged `phase4-docker-seed` into `main` after passing CI checks.

8) Troubleshooting and environment fixes (minor steps)
- Resolved pytest import errors in CI by exporting `PYTHONPATH` before running pytest.
- Fixed dotnet build/restore errors by restoring/building the specific project files rather than the repo root.
- When local Docker builds failed with containerd read-only filesystem errors during image export, documented suggested host-side fixes (see Troubleshooting below).

9) Tests and determinism
- Python unit tests use a `FakeModel` to avoid heavy model downloads in CI and to provide deterministic outputs for assertion.
- .NET tests use EF InMemory provider to run DB-related unit/integration tests without external dependencies.

10) Developer convenience and scripts
- Added `scripts/smoke_test.sh` to wait for services and perform simple GET/POST checks.
- Added `README.docker.md` with Docker run instructions and common troubleshooting steps.

Current state (as of 2026-08-28)
- Core features implemented: product data in Postgres, recommendation engine, API endpoints, unit tests for Python and .NET, Dockerfiles, compose orchestration, CI workflows.
- Outstanding: local `docker compose up` blocked by containerd export read-only FS error for `recommendation-ai` image on the developer's machine; integration workflow is available to validate on GitHub runners.

How to reproduce locally (commands)
1. Build and run the stack (from repo root):

```bash
docker compose up --build
```

2. Seed DB manually (if you want to run seeding script directly):

```bash
docker compose up -d postgres
python3 src/recommendation-ai/seed_db.py --dsn "postgresql://postgres:postgres@localhost:5432/postgres"
```

3. Run smoke tests once services are up:

```bash
bash scripts/smoke_test.sh
```

CI / GitHub runner integration
- To validate end-to-end on GitHub (useful when local Docker fails):
	1. Push branch with workflow (or merge to `main`).
	2. In GitHub, open Actions → choose `Integration Smoke Tests` → Run workflow (select branch/ref).
	3. Review logs; smoke test script runs simple HTTP checks against `api` and `recommendation-ai`.

Troubleshooting checklist (local Docker export/read-only failures)
- Restart Docker Desktop.
- Prune builders and images:

```bash
docker buildx rm mybuilder 2>/dev/null || true
docker buildx create --use --name mybuilder
docker builder prune -af
docker system prune -af
```

- Increase Docker disk image size or free host disk space.
- Disable GPU sharing / WSL GPU passthrough in Docker Desktop if CUDA libs are leaking into images.
- If failure persists only on your machine, run the integration workflow on GitHub as a reliable runner.

Notes for future development
- Add a small lightweight integration test that uses `docker compose` on CI to spin up services and run API-to-Python flow.
- Add automated seeding as part of `api-init` job with idempotence checks.
- Add more robust health checks and readiness probes for the API and recommendation service.

Acknowledgements / Contact
- If anything here is out of date, update this file and commit. For quick checks, run `bash scripts/smoke_test.sh`.

Detailed step-by-step developer guide (create repo → services → controllers)

These are the explicit terminal commands and file templates used to create the project from scratch. Run them from a development machine with `git`, `dotnet`, `python3`, and `docker` installed.

1) Create repository and clone

```bash
mkdir recommendation-platform && cd recommendation-platform
git init
gh repo create AbdulTechDev/recommendation-platform --public --confirm
git remote add origin git@github.com:AbdulTechDev/recommendation-platform.git
git checkout -b main
```

2) Create .NET API skeleton

```bash
dotnet new webapi -o src/Recommendation.Api
cd src/Recommendation.Api
# optionally remove WeatherForecast sample controller
rm Controllers/WeatherForecastController.cs || true
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
cd ../..
```

3) Add EF models and DbContext (files to create)

- `src/Recommendation.Api/Models/Product.cs` (POCO for Product)
- `src/Recommendation.Api/Data/AppDbContext.cs` (register DbSet<Product>)

Command to add migration and update DB locally (requires Postgres running):

```bash
cd src/Recommendation.Api
dotnet ef migrations add InitialCreate -p . -s .
dotnet ef database update -p . -s .
cd ../..
```

4) Create Python FastAPI service

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install fastapi uvicorn numpy pydantic sentence-transformers psycopg
mkdir -p src/recommendation-ai/app
```

Create `src/recommendation-ai/app/main.py` with a minimal FastAPI app and `recommend` endpoint; `recommendation.py` implements `RecommendationEngine`.

5) Add seeding and DB-ready scripts

Create `src/recommendation-ai/wait_for_postgres.py` and `src/recommendation-ai/seed_db.py` to wait for Postgres and run `seed-products.sql`.

6) Dockerize both services

Create `src/recommendation-ai/Dockerfile` (Python) and `src/Recommendation.Api/Dockerfile` (multi-stage .NET). Use repository-root build context and copy `src/...` into image.

7) Compose orchestration

Create `docker-compose.yml` at repo root to define `postgres`, `api-init` (migrations), `api`, and `recommendation-ai` services. Example excerpt:

```yaml
services:
	postgres:
		image: postgres:16
		environment:
			POSTGRES_PASSWORD: postgres
		volumes:
			- pgdata:/var/lib/postgresql/data
	api-init:
		build: { context: ., dockerfile: src/Recommendation.Api/Dockerfile }
		command: ["dotnet", "ef", "database", "update"]
		depends_on:
			- postgres
	api:
		build: { context: ., dockerfile: src/Recommendation.Api/Dockerfile }
		depends_on: [api-init, postgres]
	recommendation-ai:
		build: { context: ., dockerfile: src/recommendation-ai/Dockerfile }
		depends_on: [postgres]
volumes:
	pgdata:
```

8) Add unit tests

- Python: `src/recommendation-ai/tests/test_recommendation.py` using `FakeModel` to return deterministic embeddings.
- .NET: `tests/Recommendation.Api.Tests` using WebApplicationFactory and EF InMemory provider.

9) CI and integration

- Add `.github/workflows/ci.yml` to run Python tests and dotnet tests; add buildx image builds.
- Add `.github/workflows/integration.yml` for manual Compose runs on GitHub runners.

10) Common commands summary

Build and test locally:

```bash
# Python tests
cd src/recommendation-ai
pytest

# .NET tests
cd ../../src/Recommendation.Api
dotnet test
```

Bring up containers:

```bash
docker compose up --build
```

Push branch and open PR (example):

```bash
git checkout -b phase4-docker-seed
git add .
git commit -m "phase4: add docker compose, seed scripts, CI integration"
git push -u origin phase4-docker-seed
gh pr create --title "Phase4: Docker & seed" --body "Adds compose, seeding, CI" --base main
```

If you want, I can now commit and push this expanded document for you.

Deep technical walkthrough: how the .NET API, database, and Python recommendation service were created and interact

1) `.NET API` — project structure and controller creation

- Project creation: `dotnet new webapi -o src/Recommendation.Api` creates a minimal API template with `Program.cs`, `Controllers/WeatherForecastController.cs`, and a launch profile.
- Models: create a `Models` folder and add `Product.cs`:

```csharp
namespace Recommendation.Api.Models {
	public class Product {
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
	}
}
```

- DbContext: `Data/AppDbContext.cs` registers `DbSet<Product>` and configures Npgsql provider in `Program.cs`.

Program.cs snippet to register DB and services:

```csharp
builder.Services.AddDbContext<AppDbContext>(opts =>
		opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IRecommendationService, RecommendationService>(c => {
		c.BaseAddress = new Uri(builder.Configuration["Recommendation:ServiceUrl"]);
});
```

- Controller: `Controllers/ProductsController.cs` exposes GET/POST endpoints. Example method signatures:

```csharp
[HttpGet]
public async Task<IEnumerable<Product>> Get() => await _db.Products.ToListAsync();

[HttpPost]
public async Task<IActionResult> Create(Product p) { _db.Products.Add(p); await _db.SaveChangesAsync(); return CreatedAtAction(...); }
```

- Recommendations Controller: `Controllers/RecommendationsController.cs` calls `IRecommendationService.GetRecommendationsAsync(request)` which uses `HttpClient` to call the Python service's `/recommend` endpoint.

2) `Database` — creation and seeding

- Connection string: set `DefaultConnection` in `appsettings.json` or environment variables. Example:

```json
"ConnectionStrings": { "DefaultConnection": "Host=postgres;Database=app;Username=postgres;Password=postgres" }
```

- EF migrations:

```bash
cd src/Recommendation.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

- Seeding: `src/recommendation-ai/seed-products.sql` contains `INSERT INTO Products (...) VALUES (...);` statements. `seed_db.py` opens a DSN and executes the SQL file.

3) `Python FastAPI recommendation service`

- Structure: `src/recommendation-ai/app/main.py` defines FastAPI app and includes a `POST /recommend` endpoint that accepts a `RecommendationRequest` (userId, text, topN).
- `RecommendationEngine` (in `recommendation.py`) responsibilities:
	- Connect to Postgres and load `Products` table into memory (with IDs and embeddings). The embeddings are computed with `sentence-transformers` and cached.
	- For a request, encode the query text into a vector, normalize, compute cosine similarity to product embeddings, and return top-N product IDs ranked by similarity.
	- Handle exact-name boosts and zero-norm safety.

Example FastAPI endpoint in `main.py`:

```python
@app.post('/recommend')
def recommend(req: RecommendationRequest):
		engine = RecommendationEngine(dsn=os.getenv('DATABASE_URL'))
		return engine.recommend(req.text, top_n=req.top_n)
```

4) How the two services interact (runtime flow)

1. `docker-compose` starts `postgres`.
2. `recommendation-ai` waits for Postgres using `wait_for_postgres.py`, then `seed_db.py` seeds the `Products` table if empty.
3. `api-init` runs EF migrations to ensure schema exists.
4. `api` and `recommendation-ai` start. `api` controllers call `IRecommendationService` which makes an HTTP request to `http://recommendation-ai:8000/recommend` (service name from Compose network).
5. The Python service computes recommendations and returns product IDs; the .NET `RecommendationService` then queries the `Products` table (or returns product metadata from the API) and returns a `RecommendationResponse` to the client.

5) Example request flow (curl)

```bash
# Request recommendations from API
curl -X POST "http://localhost:5000/api/recommendations" -H "Content-Type: application/json" -d '{"userId": "u1", "text": "noise cancelling headphones", "topN": 5}'
```

6) Testing strategy

- Unit tests: Python uses `FakeModel` to avoid downloading heavy models. .NET uses EF InMemory provider.
- Integration: the `integration.yml` workflow runs `docker compose up --build -d` and then runs `scripts/smoke_test.sh` to ensure endpoints respond.

7) Environment variables and configuration

- Key settings:
	- `ConnectionStrings:DefaultConnection` for .NET
	- `Recommendation:ServiceUrl` in `appsettings.json` or env for the API `HttpClient`
	- `DATABASE_URL` or `DSN` for the Python service

Set them in `docker-compose.yml` under each service's `environment` block for runtime.

8) Performance and production notes

- The Python service currently computes embeddings on startup and caches them; for large catalogs, precompute and store embeddings in a vector DB (or a Postgres table with embedding columns).
- For production, move model hosting to a GPU-enabled instance or use a managed embeddings service. Add batching, async processing, and request caching.

If you'd like, I can also extract the concrete code snippets (full files) for `ProductsController.cs`, `RecommendationService.cs`, `RecommendationEngine` and `seed_db.py` into a new `docs/code-samples` folder.

Full code samples and exact commands used (copy-paste ready)

1) `src/Recommendation.Api/Models/Product.cs`

```csharp
namespace Recommendation.Api.Models;

public class Product
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Price { get; set; }
}
```

2) `src/Recommendation.Api/Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Recommendation.Api.Models;

namespace Recommendation.Api.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
	public DbSet<Product> Products { get; set; }
}
```

3) `src/Recommendation.Api/Controllers/ProductsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Data;
using Recommendation.Api.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
	private readonly AppDbContext _db;
	public ProductsController(AppDbContext db) => _db = db;

	[HttpGet]
	public async Task<IEnumerable<Product>> Get() => await _db.Products.ToListAsync();

	[HttpPost]
	public async Task<IActionResult> Create(Product p)
	{
		_db.Products.Add(p);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
	}
}
```

4) `src/Recommendation.Api/Controllers/RecommendationsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Recommendation.Api.Services;
using Recommendation.Api.Models;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
	private readonly IRecommendationService _svc;
	public RecommendationsController(IRecommendationService svc) => _svc = svc;

	[HttpPost]
	public async Task<IActionResult> Recommend([FromBody] RecommendationRequest req)
	{
		var res = await _svc.GetRecommendationsAsync(req);
		return Ok(res);
	}
}
```

5) `src/Recommendation.Api/Services/IRecommendationService.cs` and `RecommendationService.cs`

IRecommendationService:

```csharp
public interface IRecommendationService
{
	Task<RecommendationResponse> GetRecommendationsAsync(RecommendationRequest req);
}
```

RecommendationService implementation (simplified):

```csharp
using System.Net.Http.Json;
using Recommendation.Api.Models;

public class RecommendationService : IRecommendationService
{
	private readonly HttpClient _http;
	private readonly AppDbContext _db;
	public RecommendationService(HttpClient http, AppDbContext db) { _http = http; _db = db; }

	public async Task<RecommendationResponse> GetRecommendationsAsync(RecommendationRequest req)
	{
		var resp = await _http.PostAsJsonAsync("/recommend", req);
		var ids = await resp.Content.ReadFromJsonAsync<int[]>();
		var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
		return new RecommendationResponse { ProductIds = ids, Products = products };
	}
}
```

6) `src/Recommendation.Api/Program.cs` (important parts)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opts =>
	opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IRecommendationService, RecommendationService>(c =>
	c.BaseAddress = new Uri(builder.Configuration["Recommendation:ServiceUrl"]));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}

app.MapControllers();
app.Run();
```

7) `src/recommendation-ai/app/models.py`

```python
from pydantic import BaseModel
from typing import Optional

class RecommendationRequest(BaseModel):
	userId: Optional[str] = None
	text: str
	top_n: int = 5

class RecommendationResponse(BaseModel):
	product_ids: list[int]
```

8) `src/recommendation-ai/app/recommendation.py` (core engine, simplified)

```python
import os
from typing import List

class RecommendationEngine:
	def __init__(self, dsn: str):
		self.dsn = dsn
		self._products = None
		self._model = None

	def _ensure_model(self):
		if self._model is None:
			from sentence_transformers import SentenceTransformer
			import numpy as np
			self._model = SentenceTransformer('all-MiniLM-L6-v2')

	def _load_products(self):
		if self._products is None:
			import psycopg
			import numpy as np
			with psycopg.connect(self.dsn) as conn:
				cur = conn.cursor()
				cur.execute('SELECT id, name, description FROM products')
				rows = cur.fetchall()
			self._products = [{'id': r[0], 'text': f"{r[1]} {r[2]}"} for r in rows]
			# compute embeddings
			self._ensure_model()
			texts = [p['text'] for p in self._products]
			embs = self._model.encode(texts, convert_to_numpy=True)
			norms = (embs**2).sum(axis=1, keepdims=True)**0.5
			norms[norms == 0] = 1.0
			self._product_embeddings = embs / norms

	def recommend(self, query: str, top_n: int = 5) -> List[int]:
		import numpy as np
		self._load_products()
		self._ensure_model()
		q = self._model.encode([query], convert_to_numpy=True)
		qn = q / (np.linalg.norm(q, axis=1, keepdims=True) + 1e-12)
		sims = (self._product_embeddings @ qn.T).squeeze()
		idx = sims.argsort()[::-1][:top_n]
		return [self._products[i]['id'] for i in idx]
```

9) `src/recommendation-ai/app/main.py` (FastAPI)

```python
from fastapi import FastAPI
from .recommendation import RecommendationEngine
from .models import RecommendationRequest
import os

app = FastAPI()
engine = RecommendationEngine(os.getenv('DATABASE_URL', 'postgresql://postgres:postgres@postgres:5432/postgres'))

@app.post('/recommend')
def recommend(req: RecommendationRequest):
	ids = engine.recommend(req.text, top_n=getattr(req, 'top_n', 5))
	return { 'product_ids': ids }
```

10) `src/recommendation-ai/wait_for_postgres.py`

```python
import time
import psycopg
import os

dsn = os.getenv('DATABASE_URL', 'postgresql://postgres:postgres@postgres:5432/postgres')
while True:
	try:
		with psycopg.connect(dsn):
			break
	except Exception:
		time.sleep(1)

print('Postgres is ready')
```

11) `src/recommendation-ai/seed_db.py` (run SQL file)

```python
import psycopg
import os

dsn = os.getenv('DATABASE_URL', 'postgresql://postgres:postgres@postgres:5432/postgres')
sql_path = os.path.join(os.path.dirname(__file__), 'seed-products.sql')
with open(sql_path, 'r') as f:
	sql = f.read()
with psycopg.connect(dsn) as conn:
	conn.execute(sql)
```

12) Example `src/recommendation-ai/Dockerfile`

```dockerfile
FROM python:3.12-slim
WORKDIR /app
COPY src/recommendation-ai/requirements.txt ./requirements.txt
RUN pip install -r requirements.txt
COPY src/recommendation-ai/app ./app
COPY src/recommendation-ai/wait_for_postgres.py ./
COPY src/recommendation-ai/seed_db.py ./
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

13) Example `.NET Dockerfile` (multi-stage)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Recommendation.Api/*.csproj ./
RUN dotnet restore
COPY src/Recommendation.Api/. ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Recommendation.Api.dll"]
```

14) Example `docker-compose.yml` (excerpt)

```yaml
version: '3.8'
services:
  postgres:
	image: postgres:16
	environment:
	  POSTGRES_PASSWORD: postgres
	volumes: ['pgdata:/var/lib/postgresql/data']

  recommendation-ai:
	build: { context: ., dockerfile: src/recommendation-ai/Dockerfile }
	environment:
	  - DATABASE_URL=postgresql://postgres:postgres@postgres:5432/postgres
	depends_on: [postgres]

  api-init:
	build: { context: ., dockerfile: src/Recommendation.Api/Dockerfile }
	command: ["dotnet", "ef", "database", "update"]
	depends_on: [postgres]

  api:
	build: { context: ., dockerfile: src/Recommendation.Api/Dockerfile }
	environment:
	  - ConnectionStrings__DefaultConnection=Host=postgres;Database=app;Username=postgres;Password=postgres
	  - Recommendation__ServiceUrl=http://recommendation-ai:8000
	depends_on: [api-init, postgres]

volumes:
  pgdata: {}
```

15) `src/recommendation-ai/seed-products.sql` (table + sample insert)

```sql
CREATE TABLE IF NOT EXISTS products (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  description TEXT,
  price NUMERIC(10,2)
);

INSERT INTO products (name, description, price) VALUES
('Wireless Headphones','Noise cancelling over-ear headphones',199.99),
('Bluetooth Speaker','Portable speaker with deep bass',49.99)
ON CONFLICT DO NOTHING;
```

---

If you'd like, I will:
- commit and push these doc updates (I can do this now), or
- create a `docs/code-samples` folder with these files as real file examples so you can open them directly.



