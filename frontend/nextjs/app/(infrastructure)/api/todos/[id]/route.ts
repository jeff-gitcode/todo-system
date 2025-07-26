import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';
import { Todo } from '@domain/models';
import { api } from '../../apiClient';

const TODOS_ENDPOINT = '/todos';
export async function GET(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const { id } = await context.params;
    try {
        const res = await api.get<Todo>(`${TODOS_ENDPOINT}/${encodeURIComponent(id)}`);
        return NextResponse.json(res.data);
    } catch (error: unknown) {
        return NextResponse.json({ error: 'Not found' }, { status: 404 });
    }
}

export async function DELETE(req: NextRequest, context: { params: Promise<{ id: string }> }) {
    const { id } = await context.params;
    try {
        await api.delete(`${TODOS_ENDPOINT}/${encodeURIComponent(id)}`);
        return NextResponse.json({ success: true });
    } catch (error: unknown) {
        return NextResponse.json({ error: 'Delete failed' }, { status: 500 });
    }
}