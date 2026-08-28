import os

import numpy as np

from app.models import Product, Recommendation


class RecommendationEngine:

    def __init__(self, model=None, products: list[Product] | None = None):
        if model is not None:
            self.model = model
        else:
            from sentence_transformers import SentenceTransformer

            self.model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2")
        self.products = products or self._load_products()
        product_text = [self._product_text(product) for product in self.products]
        self.product_embeddings = self._encode(product_text)

    @staticmethod
    def _load_products() -> list[Product]:

        import psycopg

        database_url = os.environ.get(
            "RECOMMENDATION_DATABASE_URL",
            "host=localhost port=5432 dbname=recommendationdb user=recommendation_user password=Recommendation@123",
        )
        with psycopg.connect(database_url) as connection:
            with connection.cursor() as cursor:
                cursor.execute('SELECT "Id", "Name", "Category", "Description", "Price" FROM "Products" ORDER BY "Id"')
                return [
                    Product(
                        id=row[0],
                        name=row[1],
                        category=row[2],
                        description=row[3],
                        price=float(row[4]),
                    )
                    for row in cursor.fetchall()
                ]

    def recommend(self, query: str, top_n: int) -> list[Recommendation]:

        query_embedding = self._encode([query])[0]
        scores = self.product_embeddings @ query_embedding
        ranked_indexes = sorted(
            range(len(self.products)),
            key=lambda index: (
                self.products[index].name.casefold() != query.casefold(),
                -scores[index],
            ),
        )[:top_n]

        return [
            Recommendation(
                product=self.products[index],
                score=round(float(scores[index]), 6),
            )
            for index in ranked_indexes
        ]

    def _encode(self, texts: list[str]) -> np.ndarray:

        embeddings = np.asarray(
            self.model.encode(texts, normalize_embeddings=True),
            dtype=np.float32,
        )
        if embeddings.ndim == 1:
            embeddings = embeddings.reshape(1, -1)
        norms = np.linalg.norm(embeddings, axis=1, keepdims=True)
        return np.divide(embeddings, norms, out=np.zeros_like(embeddings), where=norms != 0)

    @staticmethod
    def _product_text(product: Product) -> str:

        return f"{product.name}. {product.category}. {product.description}"