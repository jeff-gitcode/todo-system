# TodoSystem

## Setup

1. Install .NET 8 SDK, Docker, PostgreSQL.
2. Update `appsettings.json` with your PostgreSQL connection string.
3. Run database migrations:
   ```
   dotnet ef database update --project TodoSystem.Infrastructure
   ```
4. Start API:
   ```
   dotnet run --project TodoSystem.API
   ```
5. Access Swagger UI at `/swagger`.

## Features

- Clean Architecture, CQRS, MediatR
- Minimal API, JWT Auth, Role-based Authorization
- PostgreSQL, EF Core, Serilog, Health Checks
- Pagination, Filtering, Sorting
- Unit & Integration Tests
- Docker & CI/CD

## Example Requests

See Swagger UI for examples.

# todo-frontend

This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Features

- Next.js 15
- TypeScript
- Tailwind CSS
- shadcn/ui component library
- React Query for data fetching and caching
- Modular folder structure (domain, application, infrastructure, presentation)
- Mock API for TODO CRUD

## Getting Started

Install dependencies and run the development server:

```bash
# Install dependencies
npm install

# Run development server
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser to see the app.

## Project Structure

```
app/
  (presentation)/      # UI pages and components
  (infrastructure)/    # API and repository implementations
  application/         # Service layer
  domain/              # Models and interfaces
  hooks/               # Custom React hooks
  styles/              # Global styles
components/            # Shared UI components
lib/                   # Utility functions
public/                # Static assets
```

## API Endpoints

- `GET /api/todos` - Get all TODOs
- `POST /api/todos` - Create a new TODO
- `GET /api/todos/:id` - Get TODO by ID
- `PUT /api/todos` - Update TODO
- `DELETE /api/todos` - Delete TODO

## Customization

- UI components are built with [shadcn/ui](https://ui.shadcn.com/)
- Styles are managed with [Tailwind CSS](https://tailwindcss.com/)
- Data fetching uses [React Query](https://tanstack.com/query/v5)

## Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [shadcn/ui Documentation](https://ui.shadcn.com/docs)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [TanStack Query Documentation](https://tanstack.com/query/v5)

## Storybook Support

This project supports [Storybook](https://storybook.js.org/) for developing, testing, and documenting UI components in isolation.

### Install Storybook

```bash
npm install --save-dev storybook @storybook/react @storybook/addon-actions @storybook/addon-links @storybook/addon-essentials @storybook/addon-interactions
```

### Initialize Storybook

```bash
npx storybook init
```

### Run Storybook

```bash
npm run storybook
```

Storybook will be available at [http://localhost:6006](http://localhost:6006).

### Why Storybook?

- Build UI components and pages in isolation
- Mock hard-to-reach edge cases as stories
- Integrate with tools like Next.js, Tailwind CSS, Jest, and more
- Document UI for your team to reuse
- Automate UI workflows with CI

Learn more at [storybook.js.org](https://storybook.js.org/)

## Mock API with json-server

This project uses [json-server](https://github.com/typicode/json-server) to provide a mock REST API for TODO CRUD operations.

### Install json-server

```bash
npm install --save-dev json-server
```

### Configure json-server

Create a `db.json` file in the `frontend/nextjs` directory with initial data:

```json
{
  "todos": [
    { "id": "1", "title": "Learn React" },
    { "id": "2", "title": "Finish homework" },
    { "id": "3", "title": "Read documentation" }
  ]
}
```

### Run json-server

```bash
npm run json-server
```

This will start the mock API at [http://localhost:3001/todos](http://localhost:3001/todos).

### Add jest
```bash
yarn add -D jest jest-environment-jsdom @testing-library/react @testing-library/dom @testing-library/jest-dom ts-node @types/jest
yarn create jest@latest
```

### Playwright

```bash
yarn create playwright
```

## End-to-End Testing

This project uses Playwright for end-to-end testing. To run the tests:

### Prerequisites

- Make sure json-server is running: `npm run json-server`
- Build and start the application: `npm run dev`

### Running Tests

```bash
# Run all E2E tests
npm run test:e2e

# Run with UI mode
npm run test:e2e:ui

# Run with debugging
npm run test:e2e:debug

# Run a specific test file
npx playwright test e2e/todo-workflow.spec.ts
```

## License