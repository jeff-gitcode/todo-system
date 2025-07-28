This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

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
