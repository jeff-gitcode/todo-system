import { todoController } from './todoController';
import { NextResponse } from 'next/server';
import { todoUseCase } from '@application/todoUseCase';

jest.mock('@application/todoUseCase', () => ({
    todoUseCase: {
        getAll: jest.fn(),
        create: jest.fn(),
        update: jest.fn(),
        getById: jest.fn(),
        delete: jest.fn(),
    },
}));

jest.mock('next/server', () => ({
    NextResponse: {
        json: jest.fn(),
    },
}));

describe('todoController', () => {
    const mockTodos = [{ id: '1', title: 'A' }, { id: '2', title: 'B' }];
    const mockTodo = { id: '1', title: 'A' };
    const mockId = '1';
    const mockTitle = 'A';

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('getAllTodos', () => {
        it('should return todos on success', async () => {
            (todoUseCase.getAll as jest.Mock).mockResolvedValue(mockTodos);
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            const result = await todoController.getAllTodos();

            expect(todoUseCase.getAll).toHaveBeenCalled();
            expect(NextResponse.json).toHaveBeenCalledWith(mockTodos);
            expect(result).toBe('json-response');
        });

        it('should handle error', async () => {
            (todoUseCase.getAll as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');

            const result = await todoController.getAllTodos();

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });
    });

    describe('createTodo', () => {
        it('should create todo and return 201 on success', async () => {
            const todoData = { title: mockTitle };
            (todoUseCase.create as jest.Mock).mockResolvedValue(mockTodo);
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            const result = await todoController.createTodo(todoData);

            expect(todoUseCase.create).toHaveBeenCalledWith(mockTitle, undefined, undefined);
            expect(NextResponse.json).toHaveBeenCalledWith(mockTodo, { status: 201 });
            expect(result).toBe('json-response');
        });

        it('should handle error', async () => {
            const todoData = { title: mockTitle };
            (todoUseCase.create as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');

            const result = await todoController.createTodo(todoData);

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });
        
        it('should pass optional fields to todoUseCase.create', async () => {
            const todoData = { 
                title: mockTitle,
                description: 'Test description',
                dueDate: '2023-12-31'
            };
            (todoUseCase.create as jest.Mock).mockResolvedValue({...mockTodo, ...todoData});
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            await todoController.createTodo(todoData);

            expect(todoUseCase.create).toHaveBeenCalledWith(
                mockTitle, 
                'Test description', 
                '2023-12-31'
            );
        });
    });

    describe('updateTodo', () => {
        it('should update todo and return updated todo on success', async () => {
            const updateData = { id: mockId, title: mockTitle };
            (todoUseCase.update as jest.Mock).mockResolvedValue(mockTodo);
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            const result = await todoController.updateTodo(updateData);

            expect(todoUseCase.update).toHaveBeenCalledWith(mockId, mockTitle, undefined, undefined);
            expect(NextResponse.json).toHaveBeenCalledWith(mockTodo);
            expect(result).toBe('json-response');
        });

        it('should handle error', async () => {
            const updateData = { id: mockId, title: mockTitle };
            (todoUseCase.update as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');

            const result = await todoController.updateTodo(updateData);

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });
        
        it('should pass optional fields to todoUseCase.update', async () => {
            const updateData = { 
                id: mockId, 
                title: mockTitle,
                description: 'Updated description',
                dueDate: '2024-01-15'
            };
            (todoUseCase.update as jest.Mock).mockResolvedValue({...mockTodo, ...updateData});
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            await todoController.updateTodo(updateData);

            expect(todoUseCase.update).toHaveBeenCalledWith(
                mockId, 
                mockTitle, 
                'Updated description', 
                '2024-01-15'
            );
        });
    });

    describe('getTodoById', () => {
        it('should return todo if found', async () => {
            (todoUseCase.getById as jest.Mock).mockResolvedValue(mockTodo);
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            const result = await todoController.getTodoById({} as any, mockId);

            expect(todoUseCase.getById).toHaveBeenCalledWith(mockId);
            expect(NextResponse.json).toHaveBeenCalledWith(mockTodo);
            expect(result).toBe('json-response');
        });

        it('should return 404 if not found', async () => {
            (todoUseCase.getById as jest.Mock).mockResolvedValue(null);
            (NextResponse.json as jest.Mock).mockReturnValue('not-found-response');

            const result = await todoController.getTodoById({} as any, mockId);

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'Not found' }, { status: 404 });
            expect(result).toBe('not-found-response');
        });

        it('should handle error', async () => {
            (todoUseCase.getById as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');

            const result = await todoController.getTodoById({} as any, mockId);

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });
    });

    describe('deleteTodo', () => {
        it('should delete todo and return success', async () => {
            (todoUseCase.delete as jest.Mock).mockResolvedValue(undefined);
            (NextResponse.json as jest.Mock).mockReturnValue('json-response');

            const result = await todoController.deleteTodo({} as any, mockId);

            expect(todoUseCase.delete).toHaveBeenCalledWith(mockId);
            expect(NextResponse.json).toHaveBeenCalledWith({ success: true });
            expect(result).toBe('json-response');
        });

        it('should handle error', async () => {
            (todoUseCase.delete as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');

            const result = await todoController.deleteTodo({} as any, mockId);

            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });
    });

    describe('handleError', () => {
        it('should return error message from Error object', () => {
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');
            const error = new Error('fail');
            const result = todoController.handleError(error, 'default');
            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'fail' }, { status: 500 });
            expect(result).toBe('error-response');
        });

        it('should return default error message for unknown error', () => {
            (NextResponse.json as jest.Mock).mockReturnValue('error-response');
            const result = todoController.handleError('oops', 'default');
            expect(NextResponse.json).toHaveBeenCalledWith({ error: 'default' }, { status: 500 });
            expect(result).toBe('error-response');
        });
    });
});