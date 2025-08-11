# Todo System Backend (.NET)

This is the .NET backend API for the Todo System application. It provides the necessary endpoints for managing todos and user authentication that are consumed by the Next.js frontend.

## Technology Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server/Azure SQL Database
- JWT Authentication
- xUnit for testing

## Project Structure

```
backend/net/
├── src/                    # Source code
│   ├── TodoSystem.API/     # Web API project
│   ├── TodoSystem.Application/    # Core business logic and domain models
│   ├── TodoSystem.Domain/    # Data access and EF Core configurations
│   └── TodoSystem.Infrastructure/  # Shared utilities and helpers
├── tests/                  # Test projects
│   ├── TodoSystem.API.Tests/
│   ├── TodoSystem.Application.Tests/
│   └── TodoSystem.Domain.Tests/
└── TodoSystem.sln          # Solution file
```

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- SQL Server or Azure SQL Database
- Visual Studio 2022, VS Code, or JetBrains Rider

### Setup Project and Solution
```bash
# Create solution
dotnet new sln -n TodoSystem

# Create projects
dotnet new webapi -n TodoSystem.API
dotnet new classlib -n TodoSystem.Application
dotnet new classlib -n TodoSystem.Domain
dotnet new classlib -n TodoSystem.Infrastructure

# Add projects to solution
dotnet sln add TodoSystem.API/TodoSystem.API.csproj
dotnet sln add TodoSystem.Application/TodoSystem.Application.csproj
dotnet sln add TodoSystem.Domain/TodoSystem.Domain.csproj
dotnet sln add TodoSystem.Infrastructure/TodoSystem.Infrastructure.csproj

# Create test projects
dotnet new xunit -n TodoSystem.API.Tests
dotnet new xunit -n TodoSystem.Application.Tests
dotnet new xunit -n TodoSystem.Domain.Tests
dotnet new xunit -n TodoSystem.Infrastructure.Tests

# Add test projects to solution
dotnet sln add TodoSystem.API.Tests/TodoSystem.API.Tests.csproj
dotnet sln add TodoSystem.Application.Tests/TodoSystem.Application.Tests.csproj
dotnet sln add TodoSystem.Domain.Tests/TodoSystem.Domain.Tests.csproj
dotnet sln add TodoSystem.Infrastructure.Tests/TodoSystem.Infrastructure.Tests.csproj

# Add project references
dotnet add TodoSystem.API/TodoSystem.API.csproj reference TodoSystem.Application/TodoSystem.Application.csproj
dotnet add TodoSystem.API/TodoSystem.API.csproj reference TodoSystem.Domain/TodoSystem.Domain.csproj
dotnet add TodoSystem.Application/TodoSystem.Application.csproj reference TodoSystem.Domain/TodoSystem.Domain.csproj 
dotnet add TodoSystem.Infrastructure/TodoSystem.Infrastructure.csproj reference TodoSystem.Application/TodoSystem.Application.csproj 

# Add test project references
dotnet add TodoSystem.API.Tests/TodoSystem.API.Tests.csproj reference TodoSystem.API/TodoSystem.API.csproj
dotnet add TodoSystem.Application.Tests/TodoSystem.Application.Tests.csproj reference TodoSystem.Application/TodoSystem.Application.csproj
dotnet add TodoSystem.Domain.Tests/TodoSystem.Domain.Tests.csproj reference TodoSystem.Domain/TodoSystem.Domain.csproj
```

### Setup

1. Clone the repository:
   ```
   git clone https://github.com/your-org/todo-system.git
   cd todo-system/backend/net
   ```

2. Restore dependencies:
   ```
   dotnet restore
   ```

3. Update the connection string in `appsettings.json` or via User Secrets:
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=todos;Username=postgres;Password=postgres"
   ```

4. Apply migrations to create the database:
   ```
   dotnet ef migrations add InitialCreate --project TodoSystem.Infrastructure/TodoSystem.Infrastructure.csproj --startup-project TodoSystem.API/TodoSystem.API.csproj
   dotnet ef database update --project TodoSystem.Infrastructure/TodoSystem.Infrastructure.csproj --startup-project TodoSystem.API/TodoSystem.API.csproj
   ```

5. Run the application:
   ```
   dotnet run --project ./TodoSystem.API
   ```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

## Running the API with HTTPS

To ensure your API uses HTTPS (recommended for development and required for production), use the `https` launch profile:

```sh
dotnet run --project TodoSystem.API --launch-profile https
```

This will start the API on:

- https://localhost:7148
- http://localhost:5260

You can now make secure requests to `https://localhost:7148`.

If you use Visual Studio or VS Code, select the `https` profile when launching the project.

> **Tip:** If you see a browser warning about the development certificate, you can trust the .NET dev certificate by running:
> ```sh
> dotnet dev-certs https --trust
> ```

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Authenticate user and get JWT token
- `POST /api/auth/refresh-token` - Refresh JWT token

### Todo Management

- `GET /api/todos` - Get all todos for authenticated user
- `GET /api/todos/{id}` - Get a specific todo
- `POST /api/todos` - Create a new todo
- `PUT /api/todos/{id}` - Update an existing todo
- `DELETE /api/todos/{id}` - Delete a todo

## Authentication

The API uses JWT Bearer token authentication. To access protected endpoints, include the token in the Authorization header:

```
Authorization: Bearer {your-token}
```

## Development

### Adding Migrations

```
dotnet ef migrations add MigrationName --project src/TodoSystem.Domain --startup-project src/TodoSystem.API
```

### Running Tests

```
dotnet test
```

### Code Style and Conventions

- Follow Microsoft's .NET coding conventions
- Use async/await for database operations
- Use repository pattern for data access
- Implement proper error handling and validation

## Deployment

### Azure App Service

1. Create Azure resources:
   ```
   az group create --name TodoSystemGroup --location eastus
   az appservice plan create --name TodoSystemPlan --resource-group TodoSystemGroup --sku FREE
   az webapp create --name TodoSystemAPI --resource-group TodoSystemGroup --plan TodoSystemPlan
   ```

2. Deploy the application:
   ```
   dotnet publish -c Release
   az webapp deploy --resource-group TodoSystemGroup --name TodoSystemAPI --src-path ./publish.zip
   ```

## License

[MIT](../../LICENSE)
