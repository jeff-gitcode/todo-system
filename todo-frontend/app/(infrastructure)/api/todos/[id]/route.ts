import { NextRequest, NextResponse } from 'next/server';
import { Todo } from '@domain/models';

// mock data, should be replaced with real data source in production
const todos: Todo[] = [
    { id: '1', title: 'Learn React' },
    { id: '2', title: 'Finish homework' },
    { id: '3', title: 'Read documentation' }
];

export async function GET(req: NextRequest, context: { params: { id: string } }) {
    const { id } = await context.params;
    const todo = todos.find(t => t.id === id);
    if (todo) {
        return NextResponse.json(todo);
    }
    return NextResponse.json({ error: 'Not found' }, { status: 404 });
}