#!/usr/bin/env bash
set -euo pipefail

AI_OPENAPI_URL="http://localhost:8000/openapi.json"
API_PRODUCTS_URL="http://localhost:5000/api/products"

wait_for() {
  url="$1"
  name="$2"
  timeout=${3:-60}
  echo -n "Waiting for $name at $url"
  for i in $(seq 1 $timeout); do
    if curl -sSf --fail --max-time 3 "$url" >/dev/null 2>&1; then
      echo " -> ok"
      return 0
    fi
    echo -n "."
    sleep 1
  done
  echo "\nERROR: $name did not become available within ${timeout}s"
  return 1
}

echo "Running smoke tests against the local stack"

wait_for "$AI_OPENAPI_URL" "recommendation-ai (openapi)" 60
wait_for "$API_PRODUCTS_URL" "Recommendation API /api/products" 60

echo "Fetching /api/products output (first 2KB):"
curl -sS "$API_PRODUCTS_URL" | head -c 2048 || true

echo "Smoke tests passed (endpoints responded)."
