Create a RESTful API for a Todo application with the following specifications:

Endpoints:
- GET /api/todos - Retrieve all todo items
- GET /api/todos/{id} - Retrieve a specific todo item
- POST /api/todos - Create a new todo item
- PUT /api/todos/{id} - Update an existing todo item
- DELETE /api/todos/{id} - Delete a todo item

Todo Item Structure:
- id: Unique identifier
- title: String, required, max 100 characters

Requirements:
1. Implement input validation
2. Return appropriate HTTP status codes
3. Include error handling
4. Use JSON for request/response payloads
5. Follow REST best practices
6. Include pagination for list endpoints
7. Support filtering by completion status
8. Implement basic authentication

Documentation:
- Provide OpenAPI/Swagger documentation
- Include example requests and responses
- Document error scenarios and status codes
- Add rate limiting details

Technologies:
- Choose a modern web framework
- Use a relational database
- Implement unit tests
- Follow security best practices