import os
import time
import urllib.parse

import psycopg


def wait_for_postgres(dsn: str, timeout: int = 60):
    start = time.time()
    while True:
        try:
            with psycopg.connect(dsn, timeout=3) as conn:
                return True
        except Exception:
            if time.time() - start > timeout:
                raise
            time.sleep(1)


if __name__ == "__main__":
    dsn = os.environ.get("RECOMMENDATION_DATABASE_URL")
    if not dsn:
        print("RECOMMENDATION_DATABASE_URL not set, skipping wait")
    else:
        print("Waiting for Postgres...", dsn)
        wait_for_postgres(dsn)
        print("Postgres ready")
