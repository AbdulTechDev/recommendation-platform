FROM python:3.12-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app
COPY wait_for_postgres.py ./wait_for_postgres.py
COPY seed_db.py ./seed_db.py
COPY seed-products.sql ./seed-products.sql

EXPOSE 8000

CMD ["/bin/sh", "-c", "python wait_for_postgres.py && python seed_db.py && uvicorn app.main:app --host 0.0.0.0 --port 8000"]
