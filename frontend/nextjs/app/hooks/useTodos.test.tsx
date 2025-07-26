import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useTodos } from './useTodos';
import { todoService } from '@application/todoService';


jest.mock('@application/todoService', () => ({
    todoService: {
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
        (todoService.getAll as jest.Mock).mockResolvedValue(mockTodos);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await waitFor(() => result.current.todos.length > 0);

        // Assert
        expect(todoService.getAll).toHaveBeenCalled();
        expect(result.current.todos).toEqual(mockTodos);
        expect(result.current.loading).toBe(false);
    });

    it('should call todoService.create and invalidate queries on success', async () => {
        // Arrange
        (todoService.create as jest.Mock).mockResolvedValue(mockTodo);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.createTodo.mutateAsync(mockTitle);
        });

        // Assert
        expect(todoService.create).toHaveBeenCalledWith(mockTitle);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoService.create throws', async () => {
        // Arrange
        (todoService.create as jest.Mock).mockRejectedValue(new Error('Create failed'));

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
        expect(todoService.create).toHaveBeenCalledWith(mockTitle);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Create failed');
    });

    it('should call todoService.update and invalidate queries on success', async () => {
        // Arrange
        (todoService.update as jest.Mock).mockResolvedValue(mockTodo);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.updateTodo.mutateAsync({ id: mockId, title: mockTitle });
        });

        // Assert
        expect(todoService.update).toHaveBeenCalledWith(mockId, mockTitle);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoService.update throws', async () => {
        // Arrange
        (todoService.update as jest.Mock).mockRejectedValue(new Error('Update failed'));

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
        expect(todoService.update).toHaveBeenCalledWith(mockId, mockTitle);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Update failed');
    });

    it('should call todoService.delete with correct id and invalidate queries on success', async () => {
        // Arrange
        (todoService.delete as jest.Mock).mockResolvedValue(true);

        // Act
        const { result } = renderHook(() => useTodos(), { wrapper });
        await act(async () => {
            await result.current.deleteTodo.mutateAsync(mockId);
        });

        // Assert
        expect(todoService.delete).toHaveBeenCalledWith(mockId);
        expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['todos'] });
    });

    it('should handle error if todoService.delete throws', async () => {
        // Arrange
        (todoService.delete as jest.Mock).mockRejectedValue(new Error('Delete failed'));

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
        expect(todoService.delete).toHaveBeenCalledWith(mockId);
        expect(invalidateQueries).not.toHaveBeenCalled();
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Delete failed');
    });
});