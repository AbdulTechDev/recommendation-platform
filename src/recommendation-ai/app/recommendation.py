from sentence_transformers import SentenceTransformer


class RecommendationEngine:

    def __init__(self):

        self.model = SentenceTransformer(
            "sentence-transformers/all-MiniLM-L6-v2"
        )

    def generate_embedding(self, text: str):

        return self.model.encode(
            text,
            normalize_embeddings=True
        )