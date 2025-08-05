import { todoRepository } from './todoRepository';
import { Todo } from '@domain/models';
import { localApi } from './services/apiClient';

// Mock localApi
jest.mock('./services/apiClient', () => ({
    localApi: {
        get: jest.fn(),
        post: jest.fn(),
        put: jest.fn(),
        delete: jest.fn(),
    },
}));

describe('todoRepository CRUD', () => {
    const mockTodo: Todo = { id: '123', title: 'Test Todo' };
    const mockTodos: Todo[] = [
        { id: '1', title: 'Todo 1' },
        { id: '2', title: 'Todo 2' },
    ];

    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should get all todos', async () => {
        // Arrange
        (localApi.get as jest.Mock).mockResolvedValue({ data: mockTodos });

        // Act
        const result = await todoRepository.getAll();

        // Assert
        expect(localApi.get).toHaveBeenCalledWith('/todos');
        expect(result).toEqual(mockTodos);
    });

    it('should create a new todo', async () => {
        // Arrange
        (localApi.post as jest.Mock).mockResolvedValue({ data: mockTodo });

        // Act
        const result = await todoRepository.create('Test Todo');

        // Assert
        expect(localApi.post).toHaveBeenCalledWith('/todos', { title: 'Test Todo' });
        expect(result).toEqual(mockTodo);
    });

    it('should update a todo', async () => {
        // Arrange
        (localApi.put as jest.Mock).mockResolvedValue({ data: mockTodo });

        // Act
        const result = await todoRepository.update('123', 'Updated Title');

        // Assert
        expect(localApi.put).toHaveBeenCalledWith('/todos', { id: '123', title: 'Updated Title' });
        expect(result).toEqual(mockTodo);
    });

    it('should delete a todo', async () => {
        // Arrange
        (localApi.delete as jest.Mock).mockResolvedValue({});

        // Act
        await todoRepository.delete('123');

        // Assert
        expect(localApi.delete).toHaveBeenCalledWith('/todos/123');
    });

    it('should throw if localApi.get rejects', async () => {
        // Arrange
        (localApi.get as jest.Mock).mockRejectedValue(new Error('API error'));

        // Act & Assert
        await expect(todoRepository.getAll()).rejects.toThrow('API error');
    });

    it('should throw if localApi.post rejects', async () => {
        // Arrange
        (localApi.post as jest.Mock).mockRejectedValue(new Error('API error'));

        // Act & Assert
        await expect(todoRepository.create('Test Todo')).rejects.toThrow('API error');
    });

    it('should throw if localApi.put rejects', async () => {
        // Arrange
        (localApi.put as jest.Mock).mockRejectedValue(new Error('API error'));

        // Act & Assert
        await expect(todoRepository.update('123', 'Updated Title')).rejects.toThrow('API error');
    });

    it('should throw if localApi.delete rejects', async () => {
        // Arrange
        (localApi.delete as jest.Mock).mockRejectedValue(new Error('API error'));

        // Act & Assert
        await expect(todoRepository.delete('123')).rejects.toThrow('API error');
    });
});