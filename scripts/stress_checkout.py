#!/usr/bin/env python3
import requests
import threading
import time
import sys

API_BASE = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5000'
CONCURRENCY = int(sys.argv[2]) if len(sys.argv) > 2 else 10

print(f"Using API: {API_BASE}, concurrency: {CONCURRENCY}")

# find a product
r = requests.get(f"{API_BASE}/api/products")
prods = r.json()
if not prods:
    print('No products found to test')
    sys.exit(1)
prod_id = prods[0]['id']
print('Using product id', prod_id)

# seed users and carts
tokens = []
for i in range(CONCURRENCY):
    username = f'stress{i}'
    payload = { 'username': username, 'email': f'{username}@example.com', 'password': 'pwd' }
    requests.post(f"{API_BASE}/api/users", json=payload)
    login = requests.post(f"{API_BASE}/api/auth/token", json={ 'username': username, 'password': 'pwd' })
    if login.status_code != 200:
        print('Login failed for', username, login.text)
        sys.exit(1)
    token = login.json().get('token')
    tokens.append((username, token))
    # add item to cart
    headers = { 'Authorization': f'Bearer {token}' }
    requests.post(f"{API_BASE}/api/cart/user/{i+1}/items", json={ 'productId': prod_id, 'quantity': 1, 'unitPrice': 1.0 }, headers=headers)

results = []

def checkout_thread(idx, token):
    headers = { 'Authorization': f'Bearer {token}' }
    resp = requests.post(f"{API_BASE}/api/cart/user/{idx+1}/checkout", headers=headers)
    results.append((idx, resp.status_code, resp.text))
    print('done', idx, resp.status_code)

threads = []
start = time.time()
for i, (u, t) in enumerate(tokens):
    th = threading.Thread(target=checkout_thread, args=(i, t))
    th.start()
    threads.append(th)

for th in threads:
    th.join()

print('All done in', time.time()-start)
for r in results:
    print(r)
