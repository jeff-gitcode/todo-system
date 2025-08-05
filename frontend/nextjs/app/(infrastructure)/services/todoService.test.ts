import { todoService } from './todoService';
import { api } from './apiClient';
import { Todo } from '@domain/models';

jest.mock('./apiClient', () => ({
    api: {
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn(),
    },
}));

describe('todoService', () => {
    const mockTodo: Todo = { id: '1', title: 'Test Todo' };
    const mockTodos: Todo[] = [
        { id: '1', title: 'Test Todo 1' },
        { id: '2', title: 'Test Todo 2' },
    ];

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('getAll', () => {
        it('should return all todos', async () => {
            (api.get as jest.Mock).mockResolvedValue({ data: mockTodos });

            const result = await todoService.getAll();

            expect(api.get).toHaveBeenCalledWith('/todos');
            expect(result).toEqual(mockTodos);
        });
    });

    describe('getById', () => {
        it('should return todo by id', async () => {
            (api.get as jest.Mock).mockResolvedValue({ data: mockTodo });

            const result = await todoService.getById('1');

            expect(api.get).toHaveBeenCalledWith('/todos/1');
            expect(result).toEqual(mockTodo);
        });

        it('should return null if api throws', async () => {
            (api.get as jest.Mock).mockRejectedValue(new Error('Not found'));

            const result = await todoService.getById('1');

            expect(api.get).toHaveBeenCalledWith('/todos/1');
            expect(result).toBeNull();
        });
    });

    describe('create', () => {
        it('should create a todo', async () => {
            (api.post as jest.Mock).mockResolvedValue({ data: mockTodo });

            const result = await todoService.create('Test Todo');

            expect(api.post).toHaveBeenCalledWith('/todos', { title: 'Test Todo' });
            expect(result).toEqual(mockTodo);
        });
    });

    describe('update', () => {
        it('should update a todo', async () => {
            (api.put as jest.Mock).mockResolvedValue({ data: mockTodo });

            const result = await todoService.update('1', 'Updated Title');

            expect(api.put).toHaveBeenCalledWith('/todos/1', { title: 'Updated Title' });
            expect(result).toEqual(mockTodo);
        });
    });

    describe('delete', () => {
        it('should delete a todo', async () => {
            (api.delete as jest.Mock).mockResolvedValue(undefined);

            await todoService.delete('1');

            expect(api.delete).toHaveBeenCalledWith('/todos/1');
        });
    });
});