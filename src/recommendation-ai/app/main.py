from fastapi import FastAPI

from app.models import RecommendationRequest, RecommendationResponse
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

    return RecommendationResponse(
        user_id=request.user_id,
        query=request.query,
        recommendations=engine.recommend(request.query, request.top_n),
    )