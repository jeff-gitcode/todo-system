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

## License