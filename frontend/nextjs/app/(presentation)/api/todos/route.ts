import { NextRequest } from 'next/server';
import { todoController } from '../controllers/todoController';
import { verifyToken } from '../authExternal/verifyToken';

export async function GET(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    return todoController.getAllTodos(req);
}

export async function POST(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    return todoController.createTodo(req);
}

export async function PUT(req: NextRequest) {
    const authResult = verifyToken(req);
    if (typeof authResult === 'string') {
        return todoController.handleError({ message: authResult }, 'Unauthorized');
    }
    if ('error' in authResult) return authResult;

    return todoController.updateTodo(req);
}
