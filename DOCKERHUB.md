# Kongroo Identity

User registration, authentication, and JWT issuance microservice for the
Kongroo platform. Built with ASP.NET Core and PostgreSQL, following
Domain-Driven Design with a transactional outbox for reliable event publishing.

## Tags

- `latest` — most recent stable release
- `x.y.z`  — specific version (e.g. `0.0.2`)
- `dev`    — in-progress development build

## Quick start

The container listens on port **8080** and requires a PostgreSQL database.

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=postgres;Database=kongroo_identity;Username=kongroo;Password=development" \
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

## Configuration

Configured via environment variables. The double underscore (`__`) maps to
nested configuration sections.

| Variable | Description |
|---|---|
| `ConnectionStrings__Database` | PostgreSQL connection string |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Jwt__SigningKey` | JWT signing key (min 32 chars) |
| `Jwt__AccessTokenLifetimeMinutes` | Access token lifetime in minutes |
| `BootstrapAdmin__Username` | Seed admin username |
| `BootstrapAdmin__Email` | Seed admin email |
| `BootstrapAdmin__Password` | Seed admin password |
| `BootstrapAdmin__Name` | Seed admin display name |

The bootstrap admin account is created on startup only when the users table is
empty.

## Requirements

- A reachable PostgreSQL database

## Source

Part of the Kongroo platform.
