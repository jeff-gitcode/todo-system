import { GET, DELETE } from './route';
import { api } from '../../apiClient';
import { NextRequest, NextResponse } from 'next/server';
import { Todo } from '@domain/models';

jest.mock('../../apiClient', () => ({
    api: {
        get: jest.fn(),
        delete: jest.fn(),
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
    const mockTodo: Todo = { id: mockId, title: 'Test Todo' };
    const request = {} as NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('GET', () => {


        it('should return todo data on success', async () => {
            // Arrange
            (api.get as jest.Mock).mockResolvedValue({ data: mockTodo });
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await GET(request, context);

            // Assert
            expect(api.get).toHaveBeenCalledWith(`/todos/${encodeURIComponent(mockId)}`);
            expect(response).toEqual(NextResponse.json(mockTodo));
        });

        it('should return 404 error on failure', async () => {
            // Arrange
            (api.get as jest.Mock).mockRejectedValue(new Error('Not found'));
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await GET(request, context);

            // Assert
            expect(api.get).toHaveBeenCalledWith(`/todos/${encodeURIComponent(mockId)}`);
            expect(response).toEqual(
                NextResponse.json({ error: 'Not found' }, { status: 404 })
            );
        });
    });

    describe('DELETE', () => {
        it('should return success true on delete', async () => {
            // Arrange
            (api.delete as jest.Mock).mockResolvedValue({});
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await DELETE(request, context);

            // Assert
            expect(api.delete).toHaveBeenCalledWith(`/todos/${encodeURIComponent(mockId)}`);
            expect(response).toEqual(NextResponse.json({ success: true }));
        });

        it('should return 500 error on delete failure', async () => {
            // Arrange
            (api.delete as jest.Mock).mockRejectedValue(new Error('Delete failed'));
            const context = { params: Promise.resolve({ id: mockId }) };

            // Act
            const response = await DELETE(request, context);

            // Assert
            expect(api.delete).toHaveBeenCalledWith(`/todos/${encodeURIComponent(mockId)}`);
            expect(response).toEqual(
                NextResponse.json({ error: 'Delete failed' }, { status: 500 })
            );
        });
    });
});