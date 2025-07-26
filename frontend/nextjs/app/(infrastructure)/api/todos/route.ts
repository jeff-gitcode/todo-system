import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';
import { v4 as uuidv4 } from 'uuid';
import { Todo } from '@domain/models';
import { api } from '../apiClient';

const BASE_API = 'http://localhost:3001/todos';
const TODOS_ENDPOINT = '/todos';
export async function GET() {
    try {
        const res = await api.get<Todo[]>(TODOS_ENDPOINT);
        return NextResponse.json(res.data);
    } catch (error: unknown) {
        return NextResponse.json({ error: 'Failed to fetch todos' }, { status: 500 });
    }
}

export async function POST(req: NextRequest) {
    const { title } = await req.json();
    const id = uuidv4();
    try {
        const res = await api.post<Todo>(TODOS_ENDPOINT, { id, title });
        return NextResponse.json(res.data, { status: 201 });
    } catch (error: unknown) {
        return NextResponse.json({ error: 'Failed to create todo' }, { status: 500 });
    }
}

export async function PUT(req: NextRequest) {
    const { id, title } = await req.json();
    console.log('title:', title);
    try {
        const res = await axios.put<Todo>(`${BASE_API}/${encodeURIComponent(id)}`, { title });
        return NextResponse.json(res.data);
    } catch (error: unknown) {
        return NextResponse.json({ error: 'Failed to update todo' }, { status: 500 });
    }
}
