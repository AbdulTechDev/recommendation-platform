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