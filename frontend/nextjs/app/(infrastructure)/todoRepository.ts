import { ITodoRepository } from '@domain/repositories';
import { Todo } from '@domain/models';
import { localApi } from './api/apiClient';

const END_POINT = '/todos';

export const todoRepository: ITodoRepository = {
    async getAll() {
        const res = await localApi.get<Todo[]>(END_POINT);
        return res.data;
    },
    async create(title: string) {
        const res = await localApi.post<Todo>(END_POINT, { title });
        return res.data;
    },
    async update(id: string, title: string) {
        const res = await localApi.put<Todo>(END_POINT, { id, title });
        return res.data;
    },
    async delete(id: string) {
        await localApi.delete(`${END_POINT}/${encodeURIComponent(id)}`);
    }
};