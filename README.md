# Invoice Delivery Management API
REST API (Minimal API) for managing invoices, vehicles, and deliveries.

## ✅ Features
- User Management with Roles
- Vehicle & Client Management
- Invoice Creation & Tracking
- Pagination & Filtering
- JWT Authentication
- Comprehensive Error Handling

## 🧰 Tech Stack
- .NET 9 ASP.NET Core
- PostgreSQL
- Entity Framework Core
- FluentValidation
- Docker & Docker Compose
- xUnit, FluentAssertions, Moq

## 📍 Quick Start
You need to create a new .env file in the same folder that Dockerfile and compose file are set, the file should contain values for:
```dotenv
CONNECTIONSTRINGS__PGCONNECTION='POSTGRESQL DATABASE CONNECTIONSTRING'
JWT__SECRETKEY=my-super-long-secret-key-value-jwt-validation
JWT__ISSUER=http://localhost:8080
JWT__AUDIENCE=http://localhost:8080
JWT__EXPIRATIONINMINUTES=2
```
Then call:
```
docker compose up
```

API runs at: http://localhost:8080
