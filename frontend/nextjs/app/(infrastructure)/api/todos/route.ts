import { NextRequest, NextResponse } from 'next/server';
import { Todo } from '@domain/models';

let todos: Todo[] = [
    { id: '1', title: 'Learn React' },
    { id: '2', title: 'Finish homework' },
    { id: '3', title: 'Read documentation' }
];

export async function GET() {
    return NextResponse.json(todos);
}

export async function POST(req: NextRequest) {
    const { title } = await req.json();
    const newTodo: Todo = {
        id: (Date.now() + Math.random()).toString(),
        title
    };
    todos.push(newTodo);
    return NextResponse.json(newTodo, { status: 201 });
}

export async function PUT(req: NextRequest) {
    const { id, title } = await req.json();
    const todo = todos.find(t => t.id === id);
    if (todo) {
        todo.title = title;
        return NextResponse.json(todo);
    }
    return NextResponse.json({ error: 'Not found' }, { status: 404 });
}

export async function DELETE(req: NextRequest) {
    const { id } = await req.json();
    todos = todos.filter(t => t.id !== id);
    return NextResponse.json({ success: true });
}