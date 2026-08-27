# AI Recommendation Platform

A production-style recommendation platform using:

- ASP.NET Core
- Python
- FastAPI
- Sentence Transformers
- all-MiniLM-L6-v2
- PostgreSQL
- REST APIs
- Docker
- Azure

## Architecture

Client
   |
   v
ASP.NET Core API
   |
   v
Python Recommendation API
   |
   v
Sentence Transformer
   |
   v
PostgreSQL

## Features

- Product management
- User management
- Recommendation API
- Semantic product similarity
- User interaction tracking
- REST APIs
- Swagger
- Docker
- Automated testing
- CI/CD

## APIs

### .NET

GET /api/products

POST /api/products

POST /api/recommendations

### Python

GET /health

POST /api/recommendations

## AI Model

sentence-transformers/all-MiniLM-L6-v2

## Running locally

### .NET

dotnet restore
dotnet run

### Python

python3 -m venv .venv
source .venv/bin/activate

pip install -r requirements.txt

uvicorn app.main:app --reload

### PostgreSQL and Python with Docker Compose

docker compose up --build

The .NET API reads the Python service URL from `RecommendationApi:BaseUrl` and
the database connection from `ConnectionStrings:DefaultConnection`. Override
both values with environment-specific configuration when deploying.

The recommendation API loads catalog products exclusively from PostgreSQL,
embeds the query and those products with
`all-MiniLM-L6-v2`, compares normalized vectors with cosine similarity, and
returns the requested top-N products with scores. User history and popularity
signals are reserved for a later personalized-ranking phase.