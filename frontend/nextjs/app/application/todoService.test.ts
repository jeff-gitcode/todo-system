import { todoService } from './todoService';
import { todoRepository } from '@infrastructure/todoRepository';


jest.mock('@infrastructure/todoRepository', () => ({
    todoRepository: {
        getAll: jest.fn(),
        create: jest.fn(),
        update: jest.fn(),
        delete: jest.fn(),
    },
}));


describe('todoService', () => {
    const mockTodos = [{ id: '1', title: 'A' }, { id: '2', title: 'B' }];
    const mockTodo = { id: '1', title: 'A' };
    const mockId = '1';
    const mockData = { title: 'New Todo' };

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('getAll', () => {
        it('should call todoRepository.getAll and return todos', async () => {
            // Arrange
            (todoRepository.getAll as jest.Mock).mockResolvedValue(mockTodos);

            // Act
            const result = await todoService.getAll();

            // Assert
            expect(todoRepository.getAll).toHaveBeenCalled();
            expect(result).toBe(mockTodos);
        });
    });

    describe('create', () => {
        it('should call todoRepository.create with data and return todo', async () => {
            // Arrange
            (todoRepository.create as jest.Mock).mockResolvedValue(mockTodo);

            // Act
            const result = await todoService.create(mockData.title);

            // Assert
            expect(todoRepository.create).toHaveBeenCalledWith(mockData.title);
            expect(result).toBe(mockTodo);
        });
    });

    describe('update', () => {
        it('should call todoRepository.update with id and data and return updated todo', async () => {
            // Arrange
            (todoRepository.update as jest.Mock).mockResolvedValue(mockTodo);

            // Act
            const result = await todoService.update(mockId, mockData.title);

            // Assert
            expect(todoRepository.update).toHaveBeenCalledWith(mockId, mockData.title);
            expect(result).toBe(mockTodo);
        });
    });

    describe('delete', () => {
        it('should call todoRepository.delete with id and return result', async () => {
            // Arrange
            (todoRepository.delete as jest.Mock).mockResolvedValue(true);

            // Act
            const result = await todoService.delete(mockId);

            // Assert
            expect(todoRepository.delete).toHaveBeenCalledWith(mockId);
            expect(result).toBe(true);
        });
    });
});