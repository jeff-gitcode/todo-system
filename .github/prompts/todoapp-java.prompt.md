Develop a Todo List RESTful API using Java Spring Boot 3.x implementing Clean Architecture patterns with the following specifications:

Architecture Requirements:
- Follow Clean Architecture layers: Controller, UseCase/Service, Domain, Repository
- Implement strict separation of concerns and dependency inversion
- Use DTOs for request/response with explicit validation annotations
- Create clear interfaces between layers

API Endpoints:
POST /api/v1/todos
- Create new todo item
- Request: TodoCreateDTO (title, description, dueDate)
- Response: TodoDTO, Status 201

GET /api/v1/todos
- List all todos with pagination
- Query params: page, size, sort
- Response: Page<TodoDTO>, Status 200

GET /api/v1/todos/{id}
- Get todo by ID
- Response: TodoDTO, Status 200/404

PUT /api/v1/todos/{id}
- Update existing todo
- Request: TodoUpdateDTO
- Response: TodoDTO, Status 200/404

DELETE /api/v1/todos/{id}
- Delete todo
- Response: Status 204

Domain Model:
Todo {
  id: Long (auto-generated)
  title: String (required, max 100 chars)
}

Technical Stack:
- Spring Boot 3.x
- Spring Data JPA
- PostgreSQL for production, H2 for tests
- SpringDoc OpenAPI 2.1.0 for Swagger
- JUnit 5 & Mockito for testing
- Slf4j for logging
- Maven/Gradle for build management

Implementation Requirements:
- Global exception handling with @ControllerAdvice
- Input validation using @Valid and custom validators
- Comprehensive unit tests (min 80% coverage)
- Integration tests for API endpoints
- Structured logging with correlation IDs
- CORS configuration for localhost development
- Database migrations using Flyway/Liquibase

Documentation:
- OpenAPI 3.0 specifications
- README with setup instructions
- Example cURL commands
- Environment variables documentation
- API response examples

Quality Requirements:
- Follow Google Java Style Guide
- Document public APIs and complex logic
- Handle all edge cases and validate inputs
- Use meaningful variable/method names
- Include error response schema
- Implement request/response logging

Testing Requirements:
- Unit tests for all layers
- Integration tests for API endpoints
- Test data factories
- Mock external dependencies
- Test error scenarios

Deployment:
- Dockerized application
- docker-compose for local development
- Environment-specific configurations
- Health check endpoints
- Monitoring endpoints (Actuator)

please generate the workspace and code for the above specifications.