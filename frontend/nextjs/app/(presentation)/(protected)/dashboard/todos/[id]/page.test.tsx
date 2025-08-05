import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import TodoDetailPage from './page';
import { useTodos } from '@hooks/useTodos';
import { useRouter, useParams, useSearchParams } from 'next/navigation';

// Mocks
jest.mock('@hooks/useTodos');
jest.mock('next/navigation', () => ({
    useRouter: jest.fn(),
    useParams: jest.fn(),
    useSearchParams: jest.fn(),
}));

describe('TodoDetailPage', () => {
    const mockPush = jest.fn();
    const mockCreateMutate = jest.fn();
    const mockUpdateMutate = jest.fn();
    const mockTodos = [{ id: '1', title: 'First' }, { id: '2', title: 'Second' }];

    beforeEach(() => {
        jest.clearAllMocks();
        (useRouter as jest.Mock).mockReturnValue({ push: mockPush });
        (useTodos as jest.Mock).mockReturnValue({
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            todos: mockTodos,
            loading: false,
        });
    });

    it('renders loading state', () => {
        // Arrange
        (useTodos as jest.Mock).mockReturnValue({
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            todos: mockTodos,
            loading: true,
        });

        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });
        // Act
        render(<TodoDetailPage />);

        // Assert
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders add todo form if isNew', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: 'new' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });

        // Act
        render(<TodoDetailPage />);

        // Assert
        expect(screen.getByText('Add TODO')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Enter TODO title')).toBeInTheDocument();
        expect(screen.getByText('Add')).toBeInTheDocument();
    });

    it('calls createTodo.mutate and navigates after add', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: 'new' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });
        mockCreateMutate.mockImplementation((title, { onSuccess }) => onSuccess({ id: '3' }));

        // Act
        render(<TodoDetailPage />);
        fireEvent.change(screen.getByPlaceholderText('Enter TODO title'), { target: { value: 'New Task' } });
        fireEvent.click(screen.getByText('Add'));

        // Assert
        expect(mockCreateMutate).toHaveBeenCalledWith('New Task', expect.objectContaining({ onSuccess: expect.any(Function) }));
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos/3');
    });

    it('renders not found if todo does not exist', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '404' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });
        (useTodos as jest.Mock).mockReturnValue({
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            todos: [],
            loading: false,
        });

        // Act
        render(<TodoDetailPage />);

        // Assert
        expect(screen.getByText('TODO not found.')).toBeInTheDocument();
    });

    it('renders todo detail and switches to edit mode', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '1' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });

        // Act
        render(<TodoDetailPage />);
        fireEvent.click(screen.getByText('Edit'));

        // Assert
        expect(screen.getByText('TODO Detail')).toBeInTheDocument();
        expect(screen.getByDisplayValue('First')).toBeInTheDocument();
        expect(screen.getByText('Save')).toBeInTheDocument();
    });

    it('calls updateTodo.mutate and navigates after save', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '1' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => '1' });
        mockUpdateMutate.mockImplementation((_data, { onSuccess }) => onSuccess());

        // Act
        render(<TodoDetailPage />);
        fireEvent.change(screen.getByDisplayValue('First'), { target: { value: 'Updated Title' } });
        fireEvent.click(screen.getByText('Save'));

        // Assert
        expect(mockUpdateMutate).toHaveBeenCalledWith(
            { id: '1', title: 'Updated Title' },
            expect.objectContaining({ onSuccess: expect.any(Function) })
        );
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos/1');
    });

    it('does not call updateTodo.mutate if editTitle is empty', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '1' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => '1' });

        // Act
        render(<TodoDetailPage />);
        fireEvent.change(screen.getByDisplayValue('First'), { target: { value: '' } });
        fireEvent.click(screen.getByText('Save'));

        // Assert
        expect(mockUpdateMutate).not.toHaveBeenCalled();
        expect(mockPush).not.toHaveBeenCalledWith('/todos/1');
    });

    it('can cancel edit mode', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '1' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => '1' });

        // Act
        render(<TodoDetailPage />);
        fireEvent.click(screen.getByText('Cancel'));

        // Assert
        expect(screen.getByText('Edit')).toBeInTheDocument();
    });

    it('navigates back to list from detail', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: '1' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });

        // Act
        render(<TodoDetailPage />);
        fireEvent.click(screen.getByText('Back to List'));

        // Assert
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos');
    });

    it('navigates back to list from add', () => {
        // Arrange
        (useParams as jest.Mock).mockReturnValue({ id: 'new' });
        (useSearchParams as jest.Mock).mockReturnValue({ get: () => null });

        // Act
        render(<TodoDetailPage />);
        fireEvent.click(screen.getByText('Back to List'));

        // Assert
        expect(mockPush).toHaveBeenCalledWith('/todos');
    });
})