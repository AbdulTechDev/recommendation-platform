from fastapi import FastAPI

from app.models import RecommendationRequest
from app.recommendation import RecommendationEngine


app = FastAPI(
    title="Recommendation AI API",
    version="1.0.0"
)

engine = RecommendationEngine()


@app.get("/health")
def health():

    return {
        "status": "healthy"
    }


@app.post("/api/recommendations")
def recommendations(
    request: RecommendationRequest
):

    embedding = engine.generate_embedding(
        request.query
    )

    return {
        "user_id": request.user_id,
        "query": request.query,
        "embedding_size": len(embedding)
    }