import { GET, DELETE } from './route';
import { todoController } from '@presentation/api/controllers/todoController';
import { NextRequest, NextResponse } from 'next/server';

jest.mock('@presentation/api/controllers/todoController', () => ({
    todoController: {
        getTodoById: jest.fn(),
        deleteTodo: jest.fn(),
    },
}));

jest.mock("next/server", () => ({
    NextRequest: jest.fn(),
    NextResponse: {
        json: jest.fn(),
    },
}));

describe('Todos API [id] route', () => {
    const mockId = 'abc123';
    const mockTodo = { id: mockId, title: 'Test Todo' };
    const request = {} as NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('GET', () => {
        it('should call todoController.getTodoById and return its result', async () => {
            // Arrange
            (todoController.getTodoById as jest.Mock).mockResolvedValue(NextResponse.json(mockTodo));
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await GET(request, context);

            // Assert
            expect(todoController.getTodoById).toHaveBeenCalledWith(request, mockId);
            expect(response).toEqual(NextResponse.json(mockTodo));
        });
    });

    describe('DELETE', () => {
        it('should call todoController.deleteTodo and return its result', async () => {
            // Arrange
            (todoController.deleteTodo as jest.Mock).mockResolvedValue(NextResponse.json({ success: true }));
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await DELETE(request, context);

            // Assert
            expect(todoController.deleteTodo).toHaveBeenCalledWith(request, mockId);
            expect(response).toEqual(NextResponse.json({ success: true }));
        });
    });
});