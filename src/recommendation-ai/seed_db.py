import os
import psycopg


def seed_if_empty(dsn: str, sql_path: str):
    with psycopg.connect(dsn) as conn:
        with conn.cursor() as cur:
            cur.execute('SELECT COUNT(*) FROM "Products"')
            cnt = cur.fetchone()[0]
            if cnt > 0:
                print(f"Products table already has {cnt} rows, skipping seed")
                return
            print("Seeding products from", sql_path)
            with open(sql_path, 'r') as f:
                sql = f.read()
            cur.execute(sql)
        conn.commit()


if __name__ == "__main__":
    dsn = os.environ.get("RECOMMENDATION_DATABASE_URL")
    sql_path = os.environ.get("SEED_SQL_PATH", "./seed-products.sql")
    if not dsn:
        print("RECOMMENDATION_DATABASE_URL not set, skipping seed")
    else:
        try:
            seed_if_empty(dsn, sql_path)
        except Exception as e:
            print("Seeding failed:", e)
            # don't fail container start; continue
