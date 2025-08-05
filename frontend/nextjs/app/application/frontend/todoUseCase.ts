import { todoRepository } from '@infrastructure/todoRepository';
import { Todo } from '@domain/models';

// Application layer: Use case functions call repository
export const todoUseCase = {
    async getAll(): Promise<Todo[]> {
        return await todoRepository.getAll();
    },
    async create(title: string): Promise<Todo> {
        return await todoRepository.create(title);
    },
    async update(id: string, title: string): Promise<Todo> {
        return await todoRepository.update(id, title);
    },
    async delete(id: string): Promise<void> {
        await todoRepository.delete(id);
    },
    async getById(id: string): Promise<Todo | null> {
        return await todoRepository.getById(id);
    },
};