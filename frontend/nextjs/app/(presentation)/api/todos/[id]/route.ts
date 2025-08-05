import { NextRequest } from 'next/server';
import { todoController } from '@presentation/api/controllers/todoController';

export async function GET(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const { id } = await context.params;
    return todoController.getTodoById(req, id);
}

export async function DELETE(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const { id } = await context.params;
    return todoController.deleteTodo(req, id);
}