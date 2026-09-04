# API Documentation

Document the .NET Web API endpoints and contracts. Include OpenAPI/Swagger where possible.

## Example endpoints

```
GET /api/products
GET /api/products/{id}
POST /api/products
PUT /api/products/{id}
DELETE /api/products/{id}

POST /api/recommendations

POST /api/auth/token

Cart endpoints:
GET /api/cart/user/{userId}
POST /api/cart/user/{userId}/items
DELETE /api/cart/items/{id}
POST /api/cart/user/{userId}/checkout

Categories:
GET /api/categories
POST /api/categories

Inventory:
GET /api/inventory

Orders:
GET /api/orders
POST /api/orders

Payments:
POST /api/payments/process
GET /api/payments/{id}

Reviews (product scoped):
GET /api/products/{productId}/reviews
POST /api/products/{productId}/reviews

User Interactions:
GET /api/userinteractions
POST /api/userinteractions

Users:
GET /api/users
POST /api/users
```

For each endpoint include:
- Purpose
- HTTP method
- Request schema
- Response schema
- Authentication/Authorization
- Status codes
- Errors
- Pagination, filtering, sorting
