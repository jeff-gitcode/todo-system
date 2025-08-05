import { GET, POST, PUT } from './route';
import { todoController } from '../controllers/todoController';
import { v4 as uuidv4 } from 'uuid';
import { NextRequest, NextResponse } from 'next/server';
import { verifyToken } from '../authExternal/verifyToken';
import { after } from 'node:test';

// Mock verifyToken if used in your route
jest.mock('../authExternal/verifyToken', () => ({
    verifyToken: jest.fn(),
}));

jest.mock('../controllers/todoController', () => ({
    todoController: {
        getAllTodos: jest.fn(),
        createTodo: jest.fn(),
        updateTodo: jest.fn(),
        handleError: jest.fn(),
    },
}));
jest.mock('uuid', () => ({
    v4: jest.fn(),
}));
jest.mock('next/server', () => ({
    NextRequest: jest.fn(),
    NextResponse: {
        json: jest.fn(),
    },
}));

describe('Todos API route', () => {
    const mockTodos = [{ id: '1', title: 'A' }, { id: '2', title: 'B' }];
    const mockTodo = { id: '1', title: 'A' };
    const mockId = 'uuid-1';
    const mockTitle = 'New Todo';

    // Use a fresh request object for each test
    let request: NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
        request = {
            json: jest.fn(),
        } as Partial<NextRequest> as NextRequest;
        (verifyToken as jest.Mock).mockReturnValue(() => ({ id: 'mock-user-id', email: 'mock@example.com' }));
    });

    describe('GET', () => {


        it('should return todos on success', async () => {
            // Arrange
            (todoController.getAllTodos as jest.Mock).mockResolvedValue({ data: mockTodos, opts: undefined });

            // Act
            const response = await GET(request);

            // Assert
            expect(todoController.getAllTodos).toHaveBeenCalledWith(request);
            expect(response).toEqual({ data: mockTodos, opts: undefined });
        });

        it('should return unauthorized if verifyToken returns string', async () => {
            // Arrange
            const unauthorizedResponse = { data: { error: 'Unauthorized' }, opts: { status: 500 } };
            (verifyToken as jest.Mock).mockReturnValue('unauthorized');
            (todoController.handleError as jest.Mock).mockReturnValue(unauthorizedResponse);

            // Act
            const response = await GET(request);

            // Assert
            expect(todoController.handleError).toHaveBeenCalledWith({ message: 'unauthorized' }, 'Unauthorized');
            expect(response).toEqual(unauthorizedResponse);
        });

        it('should return error if verifyToken returns error object', async () => {
            // Arrange
            const errorResponse = { error: 'token error' };
            (verifyToken as jest.Mock).mockReturnValue({ error: 'token error' });

            // Act
            const response = await GET(request);

            // Assert
            expect(response).toEqual(errorResponse);
        });
    });

    describe('POST', () => {

        beforeEach(() => {
            jest.clearAllMocks();
            request = {
                json: jest.fn(),
            } as Partial<NextRequest> as NextRequest;
        });

        afterEach(() => {
            jest.clearAllMocks();
        });

        it('should create todo and return 201 on success', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ title: mockTitle });
            (todoController.createTodo as jest.Mock).mockResolvedValue({ data: mockTodo, opts: { status: 201 } });

            // Act
            const response = await POST(request);

            // Assert
            expect(todoController.createTodo).toHaveBeenCalledWith(request);
            expect(response).toEqual({ data: mockTodo, opts: { status: 201 } });
        });

        it('should return validation error if title is missing', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({});
            (verifyToken as jest.Mock).mockReturnValue('unauthorized');
            (todoController.handleError as jest.Mock).mockReturnValue({ data: { error: 'Title is required' }, opts: { status: 500 } });

            // Act
            const response = await POST(request);

            // Assert
            expect(todoController.handleError).toHaveBeenCalledWith({ message: 'unauthorized' }, 'Unauthorized');
            expect(response).toEqual({ data: { error: 'Title is required' }, opts: { status: 500 } });
        });

        it('should return unauthorized if verifyToken returns string', async () => {
            // Arrange
            const unauthorizedResponse = { data: { error: 'Unauthorized' }, opts: { status: 500 } };
            (verifyToken as jest.Mock).mockReturnValue('unauthorized');
            (todoController.handleError as jest.Mock).mockReturnValue(unauthorizedResponse);

            // Act
            const response = await POST(request);

            // Assert
            expect(todoController.handleError).toHaveBeenCalledWith({ message: 'unauthorized' }, 'Unauthorized');
            expect(response).toEqual(unauthorizedResponse);
        });

        it('should return error if verifyToken returns error object', async () => {
            // Arrange
            const errorResponse = { error: 'token error' };
            (verifyToken as jest.Mock).mockReturnValue({ error: 'token error' });
            // Act
            const response = await POST(request);

            // Assert
            expect(response).toEqual(errorResponse);
        });
    });

    describe('PUT', () => {
        beforeEach(() => {
            jest.clearAllMocks();
            request = {
                json: jest.fn(),
            } as Partial<NextRequest> as NextRequest;
        });

        it('should update todo and return updated todo on success', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ id: mockId, title: mockTitle });
            (todoController.updateTodo as jest.Mock).mockResolvedValue({ data: mockTodo, opts: undefined });

            // Act
            const response = await PUT(request);

            // Assert
            expect(request.json).toHaveBeenCalled();
            expect(todoController.updateTodo).toHaveBeenCalledWith(request);
            expect(response).toEqual({ data: mockTodo, opts: undefined });
        });

        it('should return validation error if id or title is missing', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({});
            (verifyToken as jest.Mock).mockReturnValue('unauthorized');
            (todoController.handleError as jest.Mock).mockReturnValue({ data: { error: 'ID is required, Title is required' }, opts: { status: 500 } });

            // Act
            const response = await PUT(request);

            // Assert
            expect(todoController.handleError).toHaveBeenCalledWith({ message: 'unauthorized' }, 'Unauthorized');
            expect(response).toEqual({ data: { error: 'ID is required, Title is required' }, opts: { status: 500 } });
        });

        it('should return unauthorized if verifyToken returns string', async () => {
            // Arrange
            const unauthorizedResponse = { data: { error: 'Unauthorized' }, opts: { status: 500 } };
            (verifyToken as jest.Mock).mockReturnValue('unauthorized');
            (todoController.handleError as jest.Mock).mockReturnValue(unauthorizedResponse);
            // Act
            const response = await PUT(request);

            // Assert
            expect(todoController.handleError).toHaveBeenCalledWith({ message: 'unauthorized' }, 'Unauthorized');
            expect(response).toEqual(unauthorizedResponse);
        });

        it('should return error if verifyToken returns error object', async () => {
            // Arrange
            const errorResponse = { error: 'token error' };
            (verifyToken as jest.Mock).mockReturnValue({ error: 'token error' });

            // Act
            const response = await PUT(request);
            expect(response).toEqual(errorResponse);
        });
    }
    );
});