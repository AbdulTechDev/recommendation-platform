FROM python:3.12-slim

WORKDIR /app

COPY src/recommendation-ai/requirements.txt ./requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

COPY src/recommendation-ai/app ./app
COPY src/recommendation-ai/wait_for_postgres.py ./wait_for_postgres.py
COPY src/recommendation-ai/seed_db.py ./seed_db.py
COPY seed-products.sql ./seed-products.sql

EXPOSE 8000

CMD ["/bin/sh", "-c", "python wait_for_postgres.py && python seed_db.py && uvicorn app.main:app --host 0.0.0.0 --port 8000"]
