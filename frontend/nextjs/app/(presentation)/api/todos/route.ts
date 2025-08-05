import { NextRequest } from 'next/server';
import { todoController } from '../controllers/todoController';
import { verifyToken } from '../authExternal/verifyToken';
import { z } from 'zod';

const TodoCreateSchema = z.object({
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
    dueDate: z.string().optional(),
});

const TodoUpdateSchema = z.object({
    id: z.string().min(1, 'ID is required'),
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
    dueDate: z.string().optional(),
});

export async function GET(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    return await todoController.getAllTodos(req);
}

export async function POST(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    const body = await req.json();
    const parseResult = TodoCreateSchema.safeParse(body);
    if (!parseResult.success) {
        return todoController.handleError({ message: parseResult.error.issues.map(e => e.message).join(', ') }, 'Validation failed');
    }

    return await todoController.createTodo(req);
}

export async function PUT(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    const body = await req.json();
    const parseResult = TodoUpdateSchema.safeParse(body);
    if (!parseResult.success) {
        return todoController.handleError({ message: parseResult.error.issues.map(e => e.message).join(', ') }, 'Validation failed');
    }

    return await todoController.updateTodo(req);
}
