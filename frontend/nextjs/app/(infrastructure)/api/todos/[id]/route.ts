import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';
import { Todo } from '@domain/models';
import { api } from '../../apiClient';

const BASE_API = 'http://localhost:3001/todos';
const TODOS_ENDPOINT = '/todos';
export async function GET(req: NextRequest, context: { params: { id: string } }) {
    const { id } = await context.params;
    console.log(id);
    try {
        const res = await api.get<Todo>(`${TODOS_ENDPOINT}/${encodeURIComponent(id)}`);
        return NextResponse.json(res.data);
    } catch (error: any) {
        return NextResponse.json({ error: 'Not found' }, { status: error?.response?.status || 500 });
    }
}

export async function DELETE(req: NextRequest, context: { params: { id: string } }) {
    const { id } = await context.params;
    console.log(id);
    try {
        await api.delete(`${TODOS_ENDPOINT}/${encodeURIComponent(id)}`);
        return NextResponse.json({ success: true });
    } catch (error: any) {
        return NextResponse.json({ error: 'Delete failed' }, { status: error?.response?.status || 500 });
    }
}