import axios from 'axios';
import { ITodoRepository } from '@domain/repositories';
import { Todo } from '@domain/models';

const API_URL = '/api/todos';

export const todoRepository: ITodoRepository = {
    async getAll() {
        const res = await axios.get<Todo[]>(API_URL);
        return res.data;
    },
    async create(title: string) {
        const res = await axios.post<Todo>(API_URL, { title });
        return res.data;
    },
    async update(id: string, title: string) {
        const res = await axios.put<Todo>(`${API_URL}/${encodeURIComponent(id)}`, { title });
        return res.data;
    },
    async delete(id: string) {
        await axios.delete(`${API_URL}/${encodeURIComponent(id)}`);
    }
};