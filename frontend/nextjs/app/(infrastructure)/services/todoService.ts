import { Todo } from '@domain/models';
import { api } from './apiClient';

const JSON_SERVER_BASE = '/todos';

export const todoService = {
    async getAll(): Promise<Todo[]> {
        const res = await api.get<Todo[]>(JSON_SERVER_BASE);
        return res.data;
    },
    async getById(id: string): Promise<Todo | null> {
        try {
            const res = await api.get<Todo>(`${JSON_SERVER_BASE}/${encodeURIComponent(id)}`);
            return res.data;
        } catch (error) {
            return null;
        }
    },
    async create(title: string): Promise<Todo> {
        const res = await api.post<Todo>(JSON_SERVER_BASE, { title });
        return res.data;
    },
    async update(id: string, title: string): Promise<Todo> {
        const res = await api.put<Todo>(`${JSON_SERVER_BASE}/${encodeURIComponent(id)}`, { title });
        return res.data;
    },
    async delete(id: string): Promise<void> {
        await api.delete(`${JSON_SERVER_BASE}/${encodeURIComponent(id)}`);
    },
};