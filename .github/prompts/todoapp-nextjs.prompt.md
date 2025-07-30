Build a Next.js 15 Full-Stack TODO Application with TypeScript

Technical Stack:
- Frontend: Next.js 15 with App Router and Server Components
- Backend: Node.js RESTful API
- Database: MongoDB with Mongoose ODM
- Authentication: NextAuth.js
- API Documentation: Swagger/OpenAPI
- Testing: Jest and React Testing Library

Core Requirements:

1. Database Schema
```typescript
Task {
  id: ObjectId
  title: string [required, max: 100 chars]
}

User {
  id: ObjectId
  email: string [required, unique]
  password: string [required, hashed]
  name: string [required]
}
```

2. API Endpoints
- Authentication:
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/logout

- Tasks:
  - GET /api/tasks - List tasks with filtering/pagination
  - POST /api/tasks - Create task
  - GET /api/tasks/:id - Get task details
  - PUT /api/tasks/:id - Update task
  - DELETE /api/tasks/:id - Delete task
  - GET /api/tasks/stats - Get task statistics

3. Frontend Features
- Responsive dashboard layout (mobile-first)
- Task list with infinite scroll
- Filters: status, priority, date range, labels
- Sort by: due date, priority, creation date
- Real-time form validation
- Loading states and error boundaries
- Toast notifications for actions
- Optimistic updates for better UX

4. Security Requirements
- Input sanitization
- CSRF protection
- Rate limiting
- JWT token validation
- XSS prevention
- Secure HTTP headers
- Password hashing (bcrypt)

5. Performance Optimization
- Implement caching strategy
- Use React Suspense for loading states
- Optimize images and assets
- Implement database indexing
- Use connection pooling

6. Testing Requirements
- Unit tests for API endpoints
- Component tests for UI elements
- Integration tests for critical flows
- Minimum 80% code coverage

7. Documentation Requirements
- API documentation using Swagger
- Environment variables template
- Setup instructions
- Development guidelines
- Deployment instructions

Folder Structure:
```
/app
  /api - Route handlers
  /(auth) - Authentication pages
  /(dashboard) - Protected routes
/components
  /ui - Reusable UI components
  /features - Feature-specific components
/lib
  /db - Database configuration
  /validation - Schema validation
  /utils - Helper functions
/models - Database models
/types - TypeScript definitions
/tests - Test files
```

Delivery Format:
- GitHub repository with proper branching strategy
- Docker configuration for development
- CI/CD pipeline configuration
- Comprehensive README.md
- API documentation in OpenAPI format
- Environment variables template (.env.example)

The implementation must follow clean code principles, use proper TypeScript types, and include comprehensive error handling throughout the application.