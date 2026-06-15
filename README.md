# Kongroo.Identity

User registration, authentication, and JWT issuance microservice for FIAP Cloud Games.

## Endpoints

- `POST /users` — Register user
- `POST /tokens` — Authenticate and get JWT
- `GET /users/me` — Get authenticated user profile
- `GET /users` — List all users (Admin only)
- `GET /users/{id}` — Get user by ID (Admin only)
- `PUT /users/{id}/role` — Change user role (Admin only)
- `GET /health` — Health check

## Messaging

On user registration the service publishes a `UserCreatedIntegrationEvent` to RabbitMQ (via MassTransit) for downstream consumers (e.g. notifications). Events are dispatched through a transactional outbox, so delivery is at-least-once and consumers must be idempotent.

## Environment Variables

| Variable | Source | Description |
|---|---|---|
| `ConnectionStrings__Database` | Secret | PostgreSQL connection string |
| `RabbitMq__Host` | ConfigMap | RabbitMQ broker hostname (e.g. `rabbitmq`) |
| `RabbitMq__User` | Secret | RabbitMQ username |
| `RabbitMq__Pass` | Secret | RabbitMQ password |
| `Jwt__Issuer` | ConfigMap | JWT issuer |
| `Jwt__Audience` | ConfigMap | JWT audience |
| `Jwt__SigningKey` | Secret | JWT signing key (min 32 chars) |
| `Jwt__AccessTokenLifetimeMinutes` | ConfigMap | Token lifetime in minutes |
| `BootstrapAdmin__Username` | ConfigMap | Admin account username |
| `BootstrapAdmin__Email` | ConfigMap | Admin account email |
| `BootstrapAdmin__Password` | Secret | Admin account password |
| `BootstrapAdmin__Name` | ConfigMap | Admin display name |

## Running Locally

Start PostgreSQL and RabbitMQ first (from `Kongroo.Orchestration`):

```bash
docker compose up postgres rabbitmq -d
```

Then run the service:

```bash
dotnet run --project src/Kongroo.Identity.Api
```

## Running Tests

Requires Docker — integration and BDD specs spin up PostgreSQL (and, for the specs, RabbitMQ) via Testcontainers. Unit tests need no Docker.

```bash
dotnet test
```

## Docker

```bash
dotnet restore
docker build -t kongroo-identity .
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=localhost;Database=kongroo_identity;Username=kongroo;Password=development" \
  -e RabbitMq__Host="localhost" \
  -e RabbitMq__User="kongroo" \
  -e RabbitMq__Pass="development" \
  -e Jwt__SigningKey="your-key-here" \
  kongroo-identity
```
