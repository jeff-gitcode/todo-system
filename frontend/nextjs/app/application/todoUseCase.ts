import { todoService } from '@infrastructure/services/todoService';

export const todoUseCase = {
    getAll: todoService.getAll,
    create: todoService.create,
    update: todoService.update,
    delete: todoService.delete,
};