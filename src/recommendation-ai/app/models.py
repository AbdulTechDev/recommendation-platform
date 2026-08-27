from pydantic import AliasChoices, BaseModel, ConfigDict, Field


class Product(BaseModel):

    id: int
    name: str
    category: str
    description: str
    price: float


class Recommendation(BaseModel):

    product: Product
    score: float


class RecommendationResponse(BaseModel):

    user_id: int
    query: str
    recommendations: list[Recommendation]


class RecommendationRequest(BaseModel):

    model_config = ConfigDict(populate_by_name=True)

    user_id: int = Field(validation_alias=AliasChoices("user_id", "userId"))
    query: str
    top_n: int = Field(
        default=5,
        validation_alias=AliasChoices("top_n", "topN"),
        ge=1,
        le=100,
    )