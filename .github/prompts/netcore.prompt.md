Create a production-ready RESTful Todo API using ASP.NET Core 8.0+ with the following specifications:

Architecture & Design:
- Implement Clean Architecture with distinct layers: Domain, Application, Infrastructure, and API
- Follow SOLID principles and Domain-Driven Design concepts
- Use CQRS pattern with MediatR for handling commands and queries
- Implement Repository pattern for data access abstraction

API Requirements:
- Support CRUD operations for Todo items (title, description, due date, status, priority)
- Include pagination, filtering, and sorting for GET endpoints
- Implement proper HTTP status codes and response formats
- Use DTOs for request/response models
- Add API versioning and Swagger documentation
- Implement request validation using FluentValidation

Database:
- Use PostgreSQL as the primary database
- Implement Entity Framework Core as ORM
- Include database migrations
- Follow best practices for entity configuration
- Implement optimistic concurrency control

Security & Performance:
- Add JWT-based authentication
- Implement role-based authorization
- Include rate limiting
- Add request/response caching where appropriate
- Implement logging using Serilog
- Add health checks for the API and database

Testing:
- Include unit tests using xUnit
- Add integration tests for API endpoints
- Implement test database seeding
- Use in-memory database for testing

Documentation:
- Include API documentation using Swagger/OpenAPI
- Add XML comments for public APIs
- Include README with setup instructions and examples

Deployment:
- Add Docker support
- Include environment-specific configuration
- Implement proper error handling and middleware
- Add GitHub Actions for CI/CD

