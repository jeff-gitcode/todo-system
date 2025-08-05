import { NextRequest, NextResponse } from 'next/server';
import { todoUseCase } from '@application/todoUseCase';

// Group all controller functions into a single object for clean architecture
export const todoController = {
    async getAllTodos(req: NextRequest) {
        try {
            const todos = await todoUseCase.getAll();
            return NextResponse.json(todos);
        } catch (error) {
            return todoController.handleError(error, 'Failed to fetch todos');
        }
    },

    async createTodo(req: NextRequest) {
        try {
            const { title } = await req.json();
            const todo = await todoUseCase.create(title);
            return NextResponse.json(todo, { status: 201 });
        } catch (error) {
            return todoController.handleError(error, 'Failed to create todo');
        }
    },

    async updateTodo(req: NextRequest) {
        try {
            const { id, title } = await req.json();
            const todo = await todoUseCase.update(id, title);
            return NextResponse.json(todo);
        } catch (error) {
            return todoController.handleError(error, 'Failed to update todo');
        }
    },

    async getTodoById(req: NextRequest, id: string) {
        try {
            const todo = await todoUseCase.getById(id);
            if (!todo) {
                return NextResponse.json({ error: 'Not found' }, { status: 404 });
            }
            return NextResponse.json(todo);
        } catch (error) {
            return todoController.handleError(error, 'Failed to fetch todo');
        }
    },

    async deleteTodo(req: NextRequest, id: string) {
        try {
            await todoUseCase.delete(id);
            return NextResponse.json({ success: true });
        } catch (error) {
            return todoController.handleError(error, 'Delete failed');
        }
    },

    handleError(error: unknown, defaultMessage: string) {
        if (error instanceof Error) {
            return NextResponse.json({ error: error.message }, { status: 500 });
        }
        return NextResponse.json({ error: defaultMessage }, { status: 500 });
    }
}