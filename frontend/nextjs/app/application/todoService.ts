import { todoRepository } from '@infrastructure/todoRepository';

export const todoService = {
    getAll: todoRepository.getAll,
    create: todoRepository.create,
    update: todoRepository.update,
    delete: todoRepository.delete,
};