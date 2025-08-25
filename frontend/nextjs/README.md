This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## System Design
```mermaid
graph TD
  subgraph Frontend
    A[Next.js App]
    B[Shadcn/UI Components]
    C[Tailwind CSS]
    D[React Query - TanStack]
    E[Storybook]
    F[Playwright/Jest/Vitest]
    G[Better Auth Client]
  end

  subgraph API_Layer
    H[api/auth - Auth Endpoints]
    I[api/todos - Todo Endpoints]
    J[Middleware - Auth Routing]
  end

  subgraph Backend
    K[Prisma ORM]
    L[Postgres DB]
    M[Better Auth Server]
    N[json-server - Mock API]
  end

  A -->|UI| B
  A -->|Styling| C
  A -->|State| D
  A -->|Testing| E
  A -->|Testing| F
  A -->|Auth| G
  A -->|API Calls| H
  A -->|API Calls| I
  A -->|Protected Routing| J

  H -->|DB Access| K
  I -->|DB Access| K
  K -->|Data| L
  G -->|Session| M
  M -->|DB| K

  N -->|Mock Data| A

  J -->|Route Protection| A
```

## Getting Started

First, run the development server:

```bash
# create project
npx create-next-app@latest todo-frontend --typescript
cd todo-frontend

# tailwind css
npm install tailwindcss @tailwindcss/postcss postcss
npx tailwindcss init -p

# shadcn/ui
npx shadcn@latest init
npx shadcn@latest add button
# For navigation menu/navbar components:
npx shadcn@latest add navigation-menu

# For breadcrumb navigation:
npx shadcn@latest add breadcrumb

# For sidebar navigation:
npx shadcn@latest add sidebar

# For command menu/search:
npx shadcn@latest add command

# For dropdown menu (often used in navigation):
npx shadcn@latest add dropdown-menu

# For sheet (mobile navigation drawer):
npx shadcn@latest add sheet

# Prisma & Postgres
npm install @prisma/client prisma pg
npx prisma init

# Edit .env to set your Postgres connection string, e.g.:
# DATABASE_URL="postgresql://user:password@localhost:5432/todo_db?schema=public"

# Edit prisma/schema.prisma to define User and Todo models, then:
npx prisma migrate dev --name init
npx prisma generate

# better auth
yarn add better-auth better-auth/next-js better-auth/react

# Run a migration to create the corresponding tables
npx prisma migrate reset
npx @better-auth/cli generate
npx prisma migrate dev --name add-auth-models


npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

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

![Todo Storybook Demo](./doc/storybook.png)

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

![Todo Jest Demo](./doc/jest-test.png)

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
## User Authentication & Control

- User registration, login, logout, JWT token generation and validation are implemented in `/api/auth`.
- User data is stored in Postgres via Prisma ORM.
- All TODO API endpoints require `Authorization: Bearer <token>` header after login.
- See `prisma/schema.prisma` for the User and Todo models.

## Database: Prisma & Postgres

- Edit your `prisma/schema.prisma` to define your data models.
- Use `npx prisma migrate dev` to apply migrations.
- Use `npx prisma studio` to view and edit your data in a web UI.
- The app reads the Postgres connection string from `.env` as `DATABASE_URL`.

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
