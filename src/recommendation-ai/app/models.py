from pydantic import BaseModel


class RecommendationRequest(BaseModel):

    user_id: int
    query: str
    top_n: int = 5