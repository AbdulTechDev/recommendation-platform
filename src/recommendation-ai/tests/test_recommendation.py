import numpy as np

from app.models import Product, RecommendationRequest
from app.recommendation import RecommendationEngine


class FakeModel:
	vectors = {
		"Wireless Bluetooth Headphones. Electronics. Wireless over-ear headphones with clear audio, comfortable cushions, and long battery life.": [1, 0],
		"USB-C Fast Charger. Electronics. Fast USB-C wall charger designed for phones, tablets, and compatible devices.": [0, 1],
		"Running Shoes. Sports. Lightweight running shoes designed for daily workouts, jogging, and long-distance running.": [0, 0.5],
		"Python Programming Basics. Books. Beginner-friendly introduction to Python programming, syntax, functions, and data structures.": [0, 0],
		"Portable Bluetooth Speaker. Electronics. Compact Bluetooth speaker with rich sound, rechargeable battery, and portable design.": [0.5, 0.5],
		"Yoga Mat. Sports. Non-slip exercise mat designed for yoga, stretching, and home workouts.": [0, 0],
		"Slim Fit Jeans. Fashion. Modern slim-fit denim jeans designed for comfortable everyday styling.": [0.5, 1],
		"wireless headphones": [1, 0],
		"Slim Fit Jeans": [0.5, 1],
	}

	def encode(self, texts, normalize_embeddings=True):
		return np.array([self.vectors[text] for text in texts], dtype=np.float32)


PRODUCTS = [
	Product(id=1, name="Wireless Bluetooth Headphones", category="Electronics", description="Wireless over-ear headphones with clear audio, comfortable cushions, and long battery life.", price=99.99),
	Product(id=2, name="USB-C Fast Charger", category="Electronics", description="Fast USB-C wall charger designed for phones, tablets, and compatible devices.", price=29.99),
	Product(id=3, name="Running Shoes", category="Sports", description="Lightweight running shoes designed for daily workouts, jogging, and long-distance running.", price=79.99),
	Product(id=4, name="Python Programming Basics", category="Books", description="Beginner-friendly introduction to Python programming, syntax, functions, and data structures.", price=24.99),
	Product(id=5, name="Portable Bluetooth Speaker", category="Electronics", description="Compact Bluetooth speaker with rich sound, rechargeable battery, and portable design.", price=49.99),
	Product(id=6, name="Yoga Mat", category="Sports", description="Non-slip exercise mat designed for yoga, stretching, and home workouts.", price=34.99),
	Product(id=7, name="Slim Fit Jeans", category="Fashion", description="Modern slim-fit denim jeans designed for comfortable everyday styling.", price=49.99),
]


def test_request_accepts_dotnet_aliases():
	request = RecommendationRequest(userId=7, query="headphones", topN=3)

	assert request.user_id == 7
	assert request.top_n == 3


def test_recommendations_are_ranked_and_limited():
	engine = RecommendationEngine(model=FakeModel(), products=PRODUCTS)

	results = engine.recommend("wireless headphones", top_n=2)

	assert len(results) == 2
	assert results[0].product.name == "Wireless Bluetooth Headphones"
	assert results[0].score >= results[1].score


def test_exact_product_query_ranks_matching_product_first():
	engine = RecommendationEngine(model=FakeModel(), products=PRODUCTS)

	results = engine.recommend("Slim Fit Jeans", top_n=5)

	assert results[0].product.name == "Slim Fit Jeans"
