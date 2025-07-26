import { GET, POST, PUT } from './route';
import { api } from '../apiClient';
import axios from 'axios';
import { v4 as uuidv4 } from 'uuid';
import { NextRequest, NextResponse } from 'next/server';

jest.mock('../apiClient', () => ({
    api: {
        get: jest.fn(),
        post: jest.fn(),
    },
}));
jest.mock('axios');
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
    const request = {
        json: jest.fn(),
    } as Partial<NextRequest> as NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
    });

    describe('GET', () => {
        it('should return todos on success', async () => {
            // Arrange
            (api.get as jest.Mock).mockResolvedValue({ data: mockTodos });
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

            // Act
            const response = await GET();

            // Assert
            expect(api.get).toHaveBeenCalledWith('/todos');
            expect(response).toEqual({ data: mockTodos, opts: undefined });
        });

        it('should return 500 error on failure', async () => {
            // Arrange
            (api.get as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

            // Act
            const response = await GET();

            // Assert
            expect(api.get).toHaveBeenCalledWith('/todos');
            expect(response).toEqual({ data: { error: 'Failed to fetch todos' }, opts: { status: 500 } });
        });
    });

    describe('POST', () => {
        it('should create todo and return 201 on success', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ title: mockTitle });
            (uuidv4 as jest.Mock).mockReturnValue(mockId);
            (api.post as jest.Mock).mockResolvedValue({ data: mockTodo });
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

            // Act
            const response = await POST(request);

            // Assert
            expect(request.json).toHaveBeenCalled();
            expect(uuidv4).toHaveBeenCalled();
            expect(api.post).toHaveBeenCalledWith('/todos', { id: mockId, title: mockTitle });
            expect(response).toEqual({ data: mockTodo, opts: { status: 201 } });
        });

        it('should return 500 error on failure', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ title: mockTitle });
            (uuidv4 as jest.Mock).mockReturnValue(mockId);
            (api.post as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

            // Act
            const response = await POST(request);

            // Assert
            expect(request.json).toHaveBeenCalled();
            expect(uuidv4).toHaveBeenCalled();
            expect(api.post).toHaveBeenCalledWith('/todos', { id: mockId, title: mockTitle });
            expect(response).toEqual({ data: { error: 'Failed to create todo' }, opts: { status: 500 } });
        });
    });

    describe('PUT', () => {
        it('should update todo and return updated todo on success', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ id: mockId, title: mockTitle });
            (axios.put as jest.Mock).mockResolvedValue({ data: mockTodo });
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));
            const consoleSpy = jest.spyOn(console, 'log').mockImplementation(() => { });

            // Act
            const response = await PUT(request);

            // Assert
            expect(request.json).toHaveBeenCalled();
            expect(consoleSpy).toHaveBeenCalledWith('title:', mockTitle);
            expect(axios.put).toHaveBeenCalledWith(
                `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
                { title: mockTitle }
            );
            expect(response).toEqual({ data: mockTodo, opts: undefined });

            consoleSpy.mockRestore();
        });

        it('should return 500 error on failure', async () => {
            // Arrange
            (request.json as jest.Mock).mockResolvedValue({ id: mockId, title: mockTitle });
            (axios.put as jest.Mock).mockRejectedValue(new Error('fail'));
            (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));
            const consoleSpy = jest.spyOn(console, 'log').mockImplementation(() => { });

            // Act
            const response = await PUT(request);

            // Assert
            expect(request.json).toHaveBeenCalled();
            expect(consoleSpy).toHaveBeenCalledWith('title:', mockTitle);
            expect(axios.put).toHaveBeenCalledWith(
                `http://localhost:3001/todos/${encodeURIComponent(mockId)}`,
                { title: mockTitle }
            );
            expect(response).toEqual({ data: { error: 'Failed to update todo' }, opts: { status: 500 } });

            consoleSpy.mockRestore();
        });
    });
});