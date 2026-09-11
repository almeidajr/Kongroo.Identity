# Kongroo Identity

User registration, authentication, and JWT issuance microservice for the
Kongroo platform. Built with ASP.NET Core and PostgreSQL, following
Domain-Driven Design with a transactional outbox that reliably publishes
integration events (e.g. `UserCreatedIntegrationEvent`) via MassTransit. The
transport is selected by `Messaging__Transport`: RabbitMQ by default, or
Amazon SQS/SNS in Kubernetes, publishing to topics `kongroo-user-created` and
`kongroo-user-role-changed`.

## Tags

- `latest` — most recent stable release
- `x.y.z`  — specific version (e.g. `0.1.0`)
- `dev`    — in-progress development build

## Quick start

The container listens on port **8080** and requires a PostgreSQL database and a
RabbitMQ broker.

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=postgres;Database=kongroo_identity;Username=kongroo;Password=development" \
  -e RabbitMq__Host="rabbitmq" \
  -e RabbitMq__User="kongroo" \
  -e RabbitMq__Pass="development" \
  -e Jwt__Issuer="kongroo" \
  -e Jwt__Audience="kongroo" \
  -e Jwt__SigningKey="<a-secret-key-at-least-32-characters-long>" \
  josealmeidajr/kongroo-identity:latest
```

## Endpoints

| Method & path | Description |
|---|---|
| `POST /users` | Register user |
| `POST /tokens` | Authenticate and get a JWT |
| `GET /users/me` | Get authenticated user profile |
| `GET /users` | List all users (Admin only) |
| `GET /users/{id}` | Get user by ID (Admin only) |
| `PUT /users/{id}/role` | Change user role (Admin only) |
| `GET /health` | Health check |
| `GET /metrics` | Prometheus metrics (unauthenticated, scraped in-cluster) |

## Configuration

Configured via environment variables. The double underscore (`__`) maps to
nested configuration sections.

| Variable | Description |
|---|---|
| `ConnectionStrings__Database` | PostgreSQL connection string |
| `RabbitMq__Host` | RabbitMQ broker hostname |
| `RabbitMq__User` | RabbitMQ username |
| `RabbitMq__Pass` | RabbitMQ password |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Jwt__SigningKey` | JWT signing key (min 32 chars) |
| `Jwt__AccessTokenLifetimeMinutes` | Access token lifetime in minutes |
| `BootstrapAdmin__Username` | Seed admin username |
| `BootstrapAdmin__Email` | Seed admin email |
| `BootstrapAdmin__Password` | Seed admin password |
| `BootstrapAdmin__Name` | Seed admin display name |
| `Messaging__Transport` | `RabbitMq` (default, used by Docker Compose and the tests) or `AmazonSqs` (Kubernetes) |
| `Aws__Region` | AWS region for SQS/SNS, required when `Messaging__Transport=AmazonSqs` (e.g. `us-east-1`) |
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN` | AWS SDK default credential chain, supplied by the `aws-credentials` Secret |

The bootstrap admin account is created on startup only when the users table is
empty.

## Requirements

- A reachable PostgreSQL database
- A reachable RabbitMQ broker

## Source

Part of the Kongroo platform.
