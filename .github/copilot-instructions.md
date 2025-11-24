# Project Instructions for GitHub Copilot

## Project Overview

**Simple CRUD** is a full-stack web application implementing a User Management system with Create, Read, Update, and Delete operations. The project demonstrates production-ready architecture patterns using modern technologies.

**Tech Stack:**
- **Backend**: ASP.NET Core Web API (.NET 10), Entity Framework Core 10.0, PostgreSQL 16
- **Frontend**: React 18, Vite 6, JavaScript (ES6+)
- **Infrastructure**: Docker, Docker Compose, Nginx
- **Testing**: xUnit, Testcontainers for .NET

## Architecture Patterns

The backend follows **Clean Architecture** principles with clear separation of concerns:

### Layer Structure

```
Backend Architecture:
┌─────────────────────────────────────────────┐
│          Presentation Layer                 │
│   (Controllers - HTTP Endpoints)            │
└─────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────┐
│          Application Layer                  │
│   (Business Logic, Services, DTOs)          │
└─────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────┐
│          Infrastructure Layer               │
│   (Data Access, EF Core, DbContext)         │
└─────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────┐
│          Domain Layer                       │
│   (Entities, Domain Models)                 │
└─────────────────────────────────────────────┘
```

## Project Structure Details

### Backend Components (`src/backend/`)

#### 1. Domain Layer (`Domain/`)
**Purpose**: Core business entities and domain models - no external dependencies

**Files:**
- `Domain/Entities/User.cs` - User entity with validation attributes
  - Properties: `Guid Id`, `string Name` (max 50), `string Email` (max 20)
  - Uses `System.ComponentModel.DataAnnotations` for validation
  - Namespace: `Backend.Domain.Entities`

#### 2. Infrastructure Layer (`Infrastructure/`)
**Purpose**: Data access, database configuration, external integrations

**Files:**
- `Infrastructure/Persistence/AppDbContext.cs` - EF Core DbContext
  - Inherits from `DbContext`
  - Configures `DbSet<User> Users`
  - Uses Fluent API in `OnModelCreating()` for entity configuration
  - Namespace: `Backend.Infrastructure.Persistence`
  - Constructor: `AppDbContext(DbContextOptions<AppDbContext> options)`

**Key Configuration:**
```csharp
// Entity configuration example
entity.HasKey(e => e.Id);
entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
entity.Property(e => e.Email).IsRequired().HasMaxLength(20);
```

#### 3. Application Layer (`Application/`)
**Purpose**: Business logic, services, DTOs, mapping profiles

**Folder Structure:**
```
Application/
├── Common/
│   └── Mapping/
│       └── UserProfile.cs      # AutoMapper profile
└── Users/
    ├── IUserService.cs         # Service interface
    ├── UserService.cs          # Service implementation
    └── Dto/
        ├── UserDto.cs          # Response DTO
        ├── CreateUserRequest.cs # Create DTO
        └── UpdateUserRequest.cs # Update DTO
```

**UserService.cs** (`Backend.Application.Users` namespace):
- Constructor: `UserService(AppDbContext context, IMapper mapper)`
- Methods:
  - `Task<IEnumerable<UserDto>> GetAllAsync()` - Returns all users
  - `Task<UserDto?> GetByIdAsync(Guid id)` - Returns user by ID or null
  - `Task<UserDto> CreateAsync(CreateUserRequest request)` - Creates new user, generates Guid
  - `Task<bool> UpdateAsync(Guid id, UpdateUserRequest request)` - Updates user, returns success
  - `Task<bool> DeleteAsync(Guid id)` - Deletes user, returns success
- Dependencies: Injected `AppDbContext`, `IMapper`

**DTOs:**
- `UserDto`: Response model with `Guid Id`, `string Name`, `string Email`
- `CreateUserRequest`: Input model with `[Required]` and `[MaxLength]` validation
- `UpdateUserRequest`: Similar to CreateUserRequest (no Id field)

**AutoMapper Configuration** (`UserProfile.cs`):
```csharp
CreateMap<User, UserDto>();
CreateMap<CreateUserRequest, User>().ForMember(dest => dest.Id, opt => opt.Ignore());
CreateMap<UpdateUserRequest, User>().ForMember(dest => dest.Id, opt => opt.Ignore());
```

#### 4. Presentation Layer (`Presentation/`)
**Purpose**: API Controllers, HTTP endpoints, request/response handling

**Files:**
- `Presentation/Controllers/UsersController.cs`
  - Inherits: `ControllerBase`
  - Attributes: `[ApiController]`, `[Route("api/users")]`
  - Namespace: `Backend.Presentation.Controllers`
  - Constructor: `UsersController(IUserService userService)`
  - Endpoints:
    - `[HttpGet]` → `GetAll()` returns `200 OK` with user list
    - `[HttpGet("{id}")]` → `GetById(Guid id)` returns `200 OK` or `404 Not Found`
    - `[HttpPost]` → `Create([FromBody] CreateUserRequest)` returns `201 Created` with location header
    - `[HttpPut("{id}")]` → `Update(Guid id, [FromBody] UpdateUserRequest)` returns `204 No Content` or `404`
    - `[HttpDelete("{id}")]` → `Delete(Guid id)` returns `204 No Content` or `404`
  - Uses ModelState validation, returns `BadRequest(ModelState)` on validation errors

#### 5. Program.cs - Application Entry Point
**Namespace**: `public partial class Program` (partial for testing access)

**Service Registration:**
```csharp
// DbContext with PostgreSQL
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// AutoMapper - scans assembly for Profile classes
services.AddAutoMapper(typeof(Program).Assembly);

// Application Services
services.AddScoped<IUserService, UserService>();

// CORS Policy
services.AddCors(options => {
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://frontend")
          .AllowAnyHeader()
          .AllowAnyMethod();
});
```

**Middleware Pipeline:**
1. `app.UseCors("AllowFrontend")` - Applied before routing
2. `app.MapControllers()` - Maps attribute-routed controllers
3. Automatic migrations applied on startup using `dbContext.Database.Migrate()`

**Configuration Files:**
- `appsettings.json` - Default connection string to localhost PostgreSQL
- `appsettings.Development.json` - Development-specific settings
- Connection string format: `Host=localhost;Port=5432;Database=simplecrud;Username=postgres;Password=postgres`

### Frontend Components (`src/frontend/`)

**Structure:**
```
frontend/
├── src/
│   ├── main.jsx              # React entry point
│   ├── App.jsx               # Root component
│   ├── App.css               # Application styles
│   ├── index.css             # Global styles
│   └── components/
│       └── UserManagement.jsx # Main CRUD component
├── index.html                # HTML template
├── package.json              # Dependencies
├── vite.config.js            # Vite configuration
├── Dockerfile                # Multi-stage build for production
└── nginx.conf                # Nginx server config
```

**UserManagement.jsx** - Main Component:
- **State Management:**
  - `users` - Array of user objects
  - `loading` - Boolean for loading state
  - `error` - String for error messages
  - `editingUser` - Current user being edited or null
  - `formData` - Form inputs `{ name: '', email: '' }`

- **API Integration:**
  - Base URL: `import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'`
  - Uses native `fetch()` API
  - Endpoints:
    - GET `/api/users` - Fetch all users
    - GET `/api/users/{id}` - Fetch single user
    - POST `/api/users` - Create user with JSON body
    - PUT `/api/users/{id}` - Update user with JSON body
    - DELETE `/api/users/{id}` - Delete user

- **Features:**
  - Form validation (required fields, max lengths)
  - Create/Update mode toggle
  - Inline edit functionality
  - Delete confirmation dialog
  - Error handling with user feedback
  - Auto-refresh after mutations

**Vite Configuration:**
- Environment variable: `VITE_API_BASE_URL` for API base URL
- Dev server typically runs on port 5173
- Production build outputs to `dist/`

**Package.json Scripts:**
- `npm run dev` - Start development server
- `npm run build` - Production build
- `npm run preview` - Preview production build

### Testing (`tests/`)

**UsersIntegrationTests.cs** - Integration Test Suite:
- Framework: xUnit
- Uses `WebApplicationFactory<Program>` for in-memory API testing
- Implements `IAsyncLifetime` for setup/teardown
- Testcontainers: Spins up PostgreSQL 16 Alpine container for each test run
- Namespace: `Api.Tests.Integration`

**Test Setup:**
```csharp
private readonly PostgreSqlContainer _postgresContainer;
private WebApplicationFactory<Program> _factory;
private HttpClient _client;

// Container configuration
new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("testdb")
    .WithUsername("testuser")
    .WithPassword("testpass")
    .Build();
```

**Test Methods:**
1. `CreateUser_ReturnsCreatedUser()` - Tests POST endpoint, verifies 201 status and Location header
2. `GetUser_ReturnsUser()` - Tests GET by ID, verifies data integrity
3. `GetUsers_ReturnsList()` - Tests GET all, verifies list contains created users
4. `UpdateUser_UpdatesFields()` - Tests PUT, verifies 204 status and data changes
5. `DeleteUser_RemovesUser()` - Tests DELETE, verifies removal with follow-up GET
6. `GetUser_NonExistent_ReturnsNotFound()` - Tests 404 for missing user
7. `UpdateUser_NonExistent_ReturnsNotFound()` - Tests 404 on update
8. `DeleteUser_NonExistent_ReturnsNotFound()` - Tests 404 on delete
9. `CreateUser_InvalidData_ReturnsBadRequest()` - Tests validation with empty fields

**Running Tests:**
- Command: `dotnet test` from solution root or `tests/` directory
- Tests automatically start/stop PostgreSQL container
- Each test class instance gets isolated database
- Migrations applied automatically via `dbContext.Database.Migrate()`

### Infrastructure

**Docker Compose Configuration** (`docker-compose.yml`):

**Services:**

1. **db** (PostgreSQL):
   - Image: `postgres:16-alpine`
   - Port: 5432:5432
   - Environment: `POSTGRES_USER=postgres`, `POSTGRES_PASSWORD=postgres`, `POSTGRES_DB=simplecrud`
   - Volume: `postgres_data` for data persistence
   - Health check: `pg_isready` every 5s

2. **api** (Backend):
   - Build: `src/backend/Dockerfile`
   - Port: 8080:8080
   - Environment:
     - `ASPNETCORE_ENVIRONMENT=Development`
     - `ASPNETCORE_HTTP_PORTS=8080`
     - `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=simplecrud;Username=postgres;Password=postgres`
   - Depends on: `db` (waits for healthy status)

3. **frontend** (React):
   - Build: `src/frontend/Dockerfile` with build arg `VITE_API_BASE_URL=http://localhost:8080`
   - Port: 3000:80 (Nginx serves on port 80)
   - Depends on: `api`

**Networks:**
- `simplecrud-network` (bridge driver) - All services communicate via service names

**Volumes:**
- `postgres_data` - Persists PostgreSQL data

**Commands:**
- Start: `docker-compose up --build`
- Stop: `docker-compose down`
- Clean: `docker-compose down -v` (removes volumes)

### Database Migrations

**Entity Framework Core Migrations:**

**Location:** `src/backend/Migrations/`
- `20251124211353_InitialMigration.cs` - Initial schema
- `20251124211353_InitialMigration.Designer.cs` - Migration metadata
- `AppDbContextModelSnapshot.cs` - Current model snapshot

**Commands:**
```bash
cd src/backend

# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

**Automatic Migration:**
- Migrations run automatically on application startup via `dbContext.Database.Migrate()`
- Applied in Docker containers on first run
- Applied in test setup for integration tests

## Development Patterns & Best Practices

### Dependency Injection
- All services registered in `Program.cs`
- Use `AddScoped` for per-request services (UserService, DbContext)
- Use `AddSingleton` for application-wide services
- Constructor injection pattern throughout

### Async/Await
- All I/O operations are async
- Service methods return `Task<T>` or `Task<bool>`
- Controllers use `async Task<ActionResult<T>>`
- Database operations use `ToListAsync()`, `FindAsync()`, `SaveChangesAsync()`

### Error Handling
- Controllers return appropriate HTTP status codes
- 404 for not found resources
- 400 for validation errors with ModelState
- 204 for successful updates/deletes
- 201 for created resources with Location header

### Validation
- Data annotations on DTOs: `[Required]`, `[MaxLength]`
- ModelState validation in controllers
- Database constraints via Fluent API
- Frontend validation matches backend constraints

### Naming Conventions
- PascalCase for C# classes, methods, properties
- camelCase for JavaScript/React variables, functions
- kebab-case for URLs (`api/users`)
- Namespace structure follows folder structure
- DTOs suffixed with descriptive names (Request, Response, Dto)

### Adding New Entities

When adding a new entity (e.g., `Product`), follow this pattern:

1. **Domain Layer**: Create `Domain/Entities/Product.cs`
2. **Infrastructure Layer**: Add `DbSet<Product>` to `AppDbContext.cs`, configure in `OnModelCreating`
3. **Application Layer**:
   - Create `Application/Products/Dto/` folder with DTOs
   - Create `IProductService.cs` and `ProductService.cs`
   - Create AutoMapper profile `Application/Common/Mapping/ProductProfile.cs`
4. **Presentation Layer**: Create `Presentation/Controllers/ProductsController.cs`
5. **Register Service**: Add `services.AddScoped<IProductService, ProductService>()` in `Program.cs`
6. **Migration**: Run `dotnet ef migrations add AddProduct`
7. **Frontend**: Create React component in `src/frontend/src/components/`

### API Response Format

**Success Responses:**
```json
// GET /api/users
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "John Doe",
    "email": "john@example.com"
  }
]

// POST /api/users (201 Created)
{
  "id": "8b7c2a1e-9f3d-4e5a-b8c7-1d2e3f4a5b6c",
  "name": "Jane Smith",
  "email": "jane@example.com"
}
```

**Error Responses:**
```json
// 400 Bad Request
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  }
}

// 404 Not Found (empty body)
```

## Environment Configuration

### Development URLs
- Backend API: http://localhost:8080
- Frontend Dev: http://localhost:5173 (Vite dev server)
- Frontend Prod (Docker): http://localhost:3000
- PostgreSQL: localhost:5432

### Environment Variables

**Backend:**
- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `ASPNETCORE_HTTP_PORTS` - Port number (8080)
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string

**Frontend:**
- `VITE_API_BASE_URL` - Backend API base URL

### Package Versions

**Backend (NuGet):**
- `Microsoft.NET.Sdk.Web` - .NET 10.0
- `Npgsql.EntityFrameworkCore.PostgreSQL` - 10.0.0
- `Microsoft.EntityFrameworkCore.Design` - 10.0.0
- `AutoMapper.Extensions.Microsoft.DependencyInjection` - 12.0.1

**Testing (NuGet):**
- `xUnit` - Test framework
- `Testcontainers.PostgreSql` - PostgreSQL test containers
- `Microsoft.AspNetCore.Mvc.Testing` - WebApplicationFactory

**Frontend (npm):**
- `react` - 18.x
- `react-dom` - 18.x
- `vite` - 6.x

## Code Generation Guidelines

When GitHub Copilot generates code for this project:

1. **Follow the layer structure** - Place files in correct folders based on responsibility
2. **Use existing patterns** - Match the service/repository/controller patterns already established
3. **Include namespaces** - Use folder-based namespace structure (e.g., `Backend.Application.Users`)
4. **Add validation** - Use data annotations on DTOs, Fluent API in DbContext
5. **Make it async** - All I/O operations should be async/await
6. **Register dependencies** - Don't forget to add new services to DI container in `Program.cs`
7. **Create migrations** - After modifying entities, generate a migration
8. **Write tests** - Add integration tests following the existing xUnit + Testcontainers pattern
9. **Handle errors properly** - Return appropriate HTTP status codes with meaningful messages
10. **Update CORS if needed** - Add frontend URLs to CORS policy when necessary

## Troubleshooting Common Issues

### Port Already in Use
- Check if ports 3000, 5432, or 8080 are in use: `lsof -i :PORT`
- Modify ports in `docker-compose.yml` if needed

### Database Connection Errors
- Verify PostgreSQL is running: `docker ps` or `pg_isready`
- Check connection string matches database credentials
- Ensure database health check passes before API starts

### CORS Errors
- Verify frontend URL is in CORS policy in `Program.cs`
- Check `VITE_API_BASE_URL` matches actual API location
- Ensure `app.UseCors()` is called before `app.MapControllers()`

### Migration Issues
- Ensure you're in `src/backend/` directory
- Check connection string in `appsettings.json`
- Remove failed migrations: `dotnet ef migrations remove`
- Reset database: `docker-compose down -v`

### Frontend Build Errors
- Clear node_modules: `rm -rf node_modules && npm install`
- Check Node version: `node --version` (should be 20+)
- Verify Vite config is correct

## File Locations Quick Reference

```
Program.cs → src/backend/Program.cs
DbContext → src/backend/Infrastructure/Persistence/AppDbContext.cs
User Entity → src/backend/Domain/Entities/User.cs
UserService → src/backend/Application/Users/UserService.cs
UsersController → src/backend/Presentation/Controllers/UsersController.cs
AutoMapper Profile → src/backend/Application/Common/Mapping/UserProfile.cs
React Component → src/frontend/src/components/UserManagement.jsx
Integration Tests → tests/UsersIntegrationTests.cs
Docker Compose → docker-compose.yml
Backend Dockerfile → src/backend/Dockerfile
Frontend Dockerfile → src/frontend/Dockerfile
```

## Current Implementation Status

✅ **Completed:**
- Full CRUD operations for User entity
- Clean Architecture implementation
- RESTful API with proper status codes
- React frontend with form validation
- PostgreSQL integration with EF Core
- Docker containerization
- Integration tests with Testcontainers
- AutoMapper configuration
- CORS setup for frontend communication
- Automatic migrations on startup

🎯 **Ready for Extension:**
- Add more entities following the User pattern
- Implement authentication/authorization
- Add pagination and filtering
- Implement logging and monitoring
- Add API documentation (Swagger/OpenAPI)
- Implement global error handling middleware
- Add caching layer
- Implement soft delete pattern