# Todo System

## Setup

```sh
mvn clean package
docker-compose up --build
```

## API

- OpenAPI docs: http://localhost:8080/swagger-ui.html

### Example cURL

```sh
curl -X POST http://localhost:8080/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","description":"desc","dueDate":"2024-12-31"}'
```

## Environment Variables

- `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASS`

## Health

- `/actuator/health`

## Error Response Example

```json
{
  "error": "Validation Failed",
  "details": [
    "title: must not be blank"
  ]
}
```