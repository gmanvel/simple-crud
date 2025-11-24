# Simple CRUD - ASP.NET Core + React + PostgreSQL

A complete, production-ready CRUD starter application with ASP.NET Core Web API (.NET 10), React (Vite), and PostgreSQL.

## Features

- **Backend**: ASP.NET Core Web API (.NET 10) with Entity Framework Core
- **Frontend**: React with Vite
- **Database**: PostgreSQL with EF Core migrations
- **Testing**: Integration tests using Testcontainers
- **Containerization**: Full Docker Compose setup

## Project Structure

```
simple-crud/
├── src/
│   ├── backend/
│   │   ├── Domain/
│   │   │   └── Entities/
│   │   │       └── User.cs
│   │   ├── Infrastructure/
│   │   │   └── Persistence/
│   │   │       └── AppDbContext.cs
│   │   ├── Application/
│   │   │   ├── Users/
│   │   │   │   ├── Dto/
│   │   │   │   │   ├── UserDto.cs
│   │   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   │   └── UpdateUserRequest.cs
│   │   │   │   ├── IUserService.cs
│   │   │   │   └── UserService.cs
│   │   │   └── Common/
│   │   │       └── Mapping/
│   │   │           └── UserProfile.cs
│   │   ├── Presentation/
│   │   │   └── Controllers/
│   │   │       └── UsersController.cs
│   │   ├── Program.cs
│   │   ├── Api.csproj
│   │   └── Dockerfile
│   ├── frontend/
│   │   ├── src/
│   │   │   ├── components/
│   │   │   │   └── UserManagement.jsx
│   │   │   ├── App.jsx
│   │   │   ├── App.css
│   │   │   ├── main.jsx
│   │   │   └── index.css
│   │   ├── package.json
│   │   ├── vite.config.js
│   │   ├── Dockerfile
│   │   └── nginx.conf
├── tests/
│   ├── UsersIntegrationTests.cs
│   └── Api.Tests.Integration.csproj
├── docker-compose.yml
└── SimpleCrud.sln
```

## Domain Model

### User Entity

- `Id` (Guid) - Primary key
- `Name` (string, max 50 chars) - Required
- `Email` (string, max 20 chars) - Required

### API Endpoints

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update existing user
- `DELETE /api/users/{id}` - Delete user

## Prerequisites

- Docker and Docker Compose
- .NET 10 SDK (for local development)
- Node.js 20+ (for local development)
- PostgreSQL (for local development without Docker)

## Quick Start with Docker

Run the entire stack with a single command:

```bash
docker-compose up --build
```

This will:
- Start PostgreSQL on port 5432
- Build and start the API on port 8080
- Build and start the frontend on port 3000
- Automatically apply database migrations

Access the application:
- Frontend: http://localhost:3000
- API: http://localhost:8080/api/users

To stop:
```bash
docker-compose down
```

To remove volumes (database data):
```bash
docker-compose down -v
```

## Local Development Setup

### Backend

1. Navigate to the API project:
```bash
cd src/backend
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update the connection string in `appsettings.json` if needed.

4. Apply migrations:
```bash
dotnet ef database update
```

5. Run the API:
```bash
dotnet run
```

The API will be available at http://localhost:8080

### Frontend

1. Navigate to the frontend directory:
```bash
cd src/frontend
```

2. Install dependencies:
```bash
npm install
```

3. Update the API URL in `.env` if needed:
```
VITE_API_BASE_URL=http://localhost:8080
```

4. Run the development server:
```bash
npm run dev
```

The frontend will be available at http://localhost:5173

## Database Migrations

### Create a new migration:
```bash
cd src/backend
dotnet ef migrations add MigrationName
```

### Apply migrations:
```bash
dotnet ef database update
```

### Remove last migration:
```bash
dotnet ef migrations remove
```

## Running Tests

The integration tests use Testcontainers to spin up a PostgreSQL container automatically.

1. Navigate to the test project:
```bash
cd src/tests
```

2. Run tests:
```bash
dotnet test
```

Or run tests from the solution root:
```bash
dotnet test
```

The tests will:
- Start a PostgreSQL test container
- Apply migrations
- Run all CRUD operation tests
- Clean up containers automatically

## Building for Production

### Build API Docker image:
```bash
docker build -f src/backend/Dockerfile -t simplecrud-api .
```

### Build Frontend Docker image:
```bash
docker build -f src/frontend/Dockerfile -t simplecrud-frontend ./src/frontend
```

### Run with Docker Compose:
```bash
docker-compose up --build
```

## Technology Stack

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10.0
- Npgsql (PostgreSQL provider)
- AutoMapper 12.0

### Frontend
- React 18
- Vite 6
- JavaScript (ES6+)

### Database
- PostgreSQL 16

### Testing
- xUnit
- Testcontainers for .NET
- Microsoft.AspNetCore.Mvc.Testing

### DevOps
- Docker
- Docker Compose
- Nginx (for frontend serving)

## API Response Examples

### Get all users
```bash
curl http://localhost:8080/api/users
```

Response:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "John Doe",
    "email": "john@example.com"
  }
]
```

### Create user
```bash
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Smith","email":"jane@example.com"}'
```

Response:
```json
{
  "id": "8b7c2a1e-9f3d-4e5a-b8c7-1d2e3f4a5b6c",
  "name": "Jane Smith",
  "email": "jane@example.com"
}
```

## Troubleshooting

### Port conflicts
If ports 3000, 5432, or 8080 are already in use, you can modify them in `docker-compose.yml`.

### Database connection issues
Ensure PostgreSQL is running and the connection string in `appsettings.json` or Docker Compose environment variables is correct.

### Frontend can't reach API
Check that:
- API is running on the correct port
- CORS is properly configured in `Program.cs`
- `VITE_API_BASE_URL` in `.env` points to the correct API URL

## License

This project is provided as-is for educational and starter purposes.
# simple-crud
