# Kongroo.Identity

User registration, authentication, and JWT issuance microservice for FIAP Cloud Games.

## Endpoints

- `POST /identity/users` — Register user
- `POST /identity/tokens` — Authenticate and get JWT
- `GET /identity/users/me` — Get authenticated user profile
- `GET /identity/users` — List all users (Admin only)
- `GET /identity/users/{id}` — Get user by ID (Admin only)
- `PUT /identity/users/{id}/role` — Change user role (Admin only)
- `GET /health` — Health check

## Environment Variables

| Variable | Source | Description |
|---|---|---|
| `ConnectionStrings__Database` | Secret | PostgreSQL connection string |
| `Jwt__Issuer` | ConfigMap | JWT issuer |
| `Jwt__Audience` | ConfigMap | JWT audience |
| `Jwt__SigningKey` | Secret | JWT signing key (min 32 chars) |
| `Jwt__AccessTokenLifetimeMinutes` | ConfigMap | Token lifetime in minutes |
| `BootstrapAdmin__Username` | ConfigMap | Admin account username |
| `BootstrapAdmin__Email` | ConfigMap | Admin account email |
| `BootstrapAdmin__Password` | Secret | Admin account password |
| `BootstrapAdmin__Name` | ConfigMap | Admin display name |

## Running Locally

Start PostgreSQL first (from `Kongroo.Orchestration`):

```bash
docker compose up postgres -d
```

Then run the service:

```bash
dotnet run --project src/Kongroo.Identity.Api
```

## Running Tests

```bash
dotnet test
```

## Docker

```bash
dotnet restore   # generates packages.lock.json if missing
docker build -t kongroo-identity .
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=localhost;Database=kongroo_identity;Username=kongroo;Password=development" \
  -e Jwt__SigningKey="your-key-here" \
  kongroo-identity
```
