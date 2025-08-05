import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useTodos } from './useTodos';
import { todoUseCase } from '#app/application/todoUseCase.js';


jest.mock('@application/todoUseCase', () => ({
    todoUseCase: {
        getAll: jest.fn(),
        create: jest.fn(),
        update: jest.fn(),
        delete: jest.fn(),
    },
}));

const invalidateQueries = jest.fn();
const mockQueryClient = { invalidateQueries };

jest.mock('@tanstack/react-query', () => {
    const actual = jest.requireActual('@tanstack/react-query');
    return {
        ...actual,
        useQueryClient: jest.fn(() => mockQueryClient),
    };
});

const queryClient = new QueryClient();
const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
);

describe('useTodos - CRUD', () => {
    const mockId = '1';
    const mockTitle = 'Test Todo';
    const mockTodo = { id: mockId, title: mockTitle };
    const mockTodos = [mockTodo];

    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should fetch todos with useQuery', async () => {
        // Arrange
        (todoUseCase.getAll as jest.Mock).mockResolvedValue(mockTodos);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await waitFor(() => result.current.todos.length > 0);

        // Assert
        expect(todoUseCase.getAll).toHaveBeenCalled();
        expect(result.current.todos).toEqual(mockTodos);
        expect(result.current.loading).toBe(false);
    });

    it('should call todoUseCase.create and invalidate queries on success', async () => {
        // Arrange
        (todoUseCase.create as jest.Mock).mockResolvedValue(mockTodo);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.createTodo.mutateAsync(mockTitle);
        });

        // Assert
        expect(todoUseCase.create).toHaveBeenCalledWith(mockTitle);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoUseCase.create throws', async () => {
        // Arrange
        (todoUseCase.create as jest.Mock).mockRejectedValue(new Error('Create failed'));

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        let error;
        try {
            await act(async () => {
                await result.current.createTodo.mutateAsync(mockTitle);
            });
        } catch (e) {
            error = e;
        }

        // Assert
        expect(todoUseCase.create).toHaveBeenCalledWith(mockTitle);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Create failed');
    });

    it('should call todoUseCase.update and invalidate queries on success', async () => {
        // Arrange
        (todoUseCase.update as jest.Mock).mockResolvedValue(mockTodo);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.updateTodo.mutateAsync({ id: mockId, title: mockTitle });
        });

        // Assert
        expect(todoUseCase.update).toHaveBeenCalledWith(mockId, mockTitle);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoUseCase.update throws', async () => {
        // Arrange
        (todoUseCase.update as jest.Mock).mockRejectedValue(new Error('Update failed'));

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        let error;
        try {
            await act(async () => {
                await result.current.updateTodo.mutateAsync({ id: mockId, title: mockTitle });
            });
        } catch (e) {
            error = e;
        }

        // Assert
        expect(todoUseCase.update).toHaveBeenCalledWith(mockId, mockTitle);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Update failed');
    });

    it('should call todoUseCase.delete with correct id and invalidate queries on success', async () => {
        // Arrange
        (todoUseCase.delete as jest.Mock).mockResolvedValue(true);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.deleteTodo.mutateAsync(mockId);
        });

        // Assert
        expect(todoUseCase.delete).toHaveBeenCalledWith(mockId);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoUseCase.delete throws', async () => {
        // Arrange
        (todoUseCase.delete as jest.Mock).mockRejectedValue(new Error('Delete failed'));

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        let error;
        try {
            await act(async () => {
                await result.current.deleteTodo.mutateAsync(mockId);
            });
        } catch (e) {
            error = e;
        }

        // Assert
        expect(todoUseCase.delete).toHaveBeenCalledWith(mockId);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Delete failed');
    });
});