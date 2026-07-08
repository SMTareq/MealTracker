# Meal & Expense Tracker API (ASP.NET Core 8 + JWT + PostgreSQL)

## 1. Prerequisites
- .NET 8 SDK
- PostgreSQL running locally (or update the connection string)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## 2. Setup
```bash
cd MealExpenseTracker.Api
dotnet restore
```

Update `appsettings.json`:
- `ConnectionStrings:DefaultConnection` — your PostgreSQL connection
- `JwtSettings:Secret` — a long random string (32+ chars). For local dev you can also use
  `dotnet user-secrets set "JwtSettings:Secret" "your-secret"` instead of storing it in the file.

## 3. Create the database
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 4. Run
```bash
dotnet run
```
Swagger UI will be available at `https://localhost:<port>/swagger`.

## 5. Making the first Admin user
There's no public "become admin" endpoint (by design). After registering a normal user via
`POST /api/auth/register`, promote them manually in the database:
```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'you@example.com';
```

## 6. Auth flow
1. `POST /api/auth/register` or `/api/auth/login` → returns `accessToken` (short-lived, 15 min)
   + `refreshToken` (7 days).
2. Send `Authorization: Bearer <accessToken>` on all subsequent requests.
3. When the access token expires, call `POST /api/auth/refresh` with the refresh token to get a new pair.
4. `POST /api/auth/logout` revokes a refresh token.

## 7. Endpoints summary

| Method | Route | Access |
|---|---|---|
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| POST | /api/auth/refresh | Public (valid refresh token) |
| POST | /api/auth/logout | Public (valid refresh token) |
| GET | /api/users/me | Authenticated |
| GET | /api/users | Admin |
| PATCH | /api/users/{id}/status | Admin |
| GET/POST/PUT/DELETE | /api/meals | Authenticated (own data; admin can pass `?userId=`) |
| GET/POST/PUT/DELETE | /api/expenses | Authenticated (own data; admin can pass `?userId=`) |
| GET | /api/reports/monthly-summary?year=&month= | Authenticated |
| GET | /api/reports/monthly-summary/all?year=&month= | Admin |

## 8. Notes
- Passwords hashed with BCrypt.
- CORS is pre-configured for `http://localhost:3000` (Next.js dev server) — edit `CorsSettings:AllowedOrigins`
  in `appsettings.json` for other origins.
- Role-based authorization uses `[Authorize(Roles = "Admin")]`.
