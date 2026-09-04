# Database Documentation

Document the database schemas, ERD, migrations and data dictionary.

## Suggested files

- erd.md — entity-relationship diagram and visual
- database-design.md — tables, columns, types, indexes
- data-dictionary.md — field-level descriptions

## Example entities

```
Users
 │
 │ 1:N
 ▼
Interactions
 │
 │ N:1
 ▼
Products
```

Include migrations located in `Recommendation.Api/Migrations` and seed data scripts.

## Entities (from `Recommendation.Api/Models`)

- `Product`
	- `Id` (int, PK)
	- `Name` (string)
	- `Category` (string)
	- `Description` (string)
	- `Price` (decimal)

- `User`
	- `Id` (int, PK)
	- `Username` (string)
	- `Email` (string)
	- `Role` (string)
	- `PasswordHash` (string, nullable)
	- `CreatedAt` (datetime)

- `UserInteraction`
	- `Id` (int, PK)
	- `UserId` (int, FK -> Users.Id)
	- `ProductId` (int, FK -> Products.Id)
	- `InteractionType` (string)
	- `Value` (int?, nullable)
	- `CreatedAt` (datetime)

- `Order`
	- `Id` (int, PK)
	- `UserId` (int, FK -> Users.Id)
	- `Total` (decimal)
	- `CreatedAt` (datetime)

- `Cart` and `CartItem`
	- `Cart`: `Id`, `UserId`, `CreatedAt`
	- `CartItem`: `Id`, `CartId` (FK -> Cart.Id), `ProductId` (FK -> Product.Id), `Quantity`, `UnitPrice`

- `Category`
	- `Id` (int, PK)
	- `Name` (string)
	- `Description` (string, nullable)

- `InventoryItem`
	- `Id` (int, PK)
	- `ProductId` (int, FK -> Products.Id)
	- `QuantityAvailable` (int)

- `Review`
	- `Id` (int, PK)
	- `ProductId` (int, FK -> Products.Id)
	- `UserId` (int, FK -> Users.Id)
	- `Rating` (int)
	- `Comment` (string, nullable)
	- `CreatedAt` (datetime)

- `Payment`
	- `Id` (int, PK)
	- `OrderId` (int, FK -> Orders.Id)
	- `Amount` (decimal)
	- `Status` (string)
	- `CreatedAt` (datetime)

### Notes
- Primary keys: properties named `Id` are used as PKs by convention.
- Foreign keys: inferred by property names like `UserId`, `ProductId`, `CartId`, `OrderId`.
- Migrations: see `src/Recommendation.Api/Migrations` for concrete schema and constraints.
