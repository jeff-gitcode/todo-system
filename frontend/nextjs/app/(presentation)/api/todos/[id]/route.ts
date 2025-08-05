import { NextRequest } from 'next/server';
import { todoController } from '@presentation/api/controllers/todoController';
import { z } from 'zod';

const IdSchema = z.object({
    id: z.string().min(1, 'ID is required'),
});

export async function GET(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const params = await context.params;
    const parseResult = IdSchema.safeParse(params);
    if (!parseResult.success) {
        return todoController.handleError({ message: parseResult.error.issues.map(e => e.message).join(', ') }, 'Validation failed');
    }
    return todoController.getTodoById(req, parseResult.data.id);
}

export async function DELETE(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const params = await context.params;
    const parseResult = IdSchema.safeParse(params);
    if (!parseResult.success) {
        return todoController.handleError({ message: parseResult.error.issues.map(e => e.message).join(', ') }, 'Validation failed');
    }
    return todoController.deleteTodo(req, parseResult.data.id);
}