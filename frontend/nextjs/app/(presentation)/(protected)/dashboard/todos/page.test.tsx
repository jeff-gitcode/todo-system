import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import TodosPage from './page';
import { useTodos } from '@hooks/useTodos';
import { useRouter } from 'next/navigation';


// Arrange: Mock dependencies
jest.mock('@hooks/useTodos');
jest.mock('next/navigation', () => ({
    useRouter: jest.fn(),
}));

const mockRefresh = jest.fn();
const mockPush = jest.fn();

const mockTodos = [
    { id: '1', title: 'First' },
    { id: '2', title: 'Second' },
];

const mockDeleteMutate = jest.fn();

beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({
        push: mockPush,
        refresh: mockRefresh,
    });
    (useTodos as jest.Mock).mockReturnValue({
        todos: mockTodos,
        loading: false,
        deleteTodo: { mutate: mockDeleteMutate },
    });
});

describe('TodosPage', () => {
    it('renders loading state', () => {
        // Arrange
        (useTodos as jest.Mock).mockReturnValue({
            todos: [],
            loading: true,
            deleteTodo: { mutate: mockDeleteMutate },
        });

        // Act
        render(<TodosPage />);

        // Assert
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders todos list', () => {
        // Act
        render(<TodosPage />);

        // Assert
        expect(screen.getByText('TODO List')).toBeInTheDocument();
        // expect(screen.getByText('First')).toBeInTheDocument();
        // expect(screen.getByText('Second')).toBeInTheDocument();
    });

    it('navigates to todo detail on todo click', () => {
        // Act
        render(<TodosPage />);
        fireEvent.click(screen.getByText(/First/));

        // Assert
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos/1');
    });

    it('navigates to edit page on Edit button click', () => {
        // Act
        render(<TodosPage />);
        const editButtons = screen.getAllByText('Edit');
        fireEvent.click(editButtons[0]);

        // Assert
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos/1?edit=1');
    });

    it('calls deleteTodo.mutate and refreshes on Delete button click', () => {
        // Arrange
        mockDeleteMutate.mockImplementation((_id, { onSuccess }) => onSuccess());

        // Act
        render(<TodosPage />);
        const deleteButtons = screen.getAllByText('Delete');
        fireEvent.click(deleteButtons[0]);

        // Assert
        expect(mockDeleteMutate).toHaveBeenCalledWith('1', expect.objectContaining({
            onSuccess: expect.any(Function),
        }));
        expect(mockRefresh).toHaveBeenCalled();
    });

    it('navigates to add todo page on Add TODO button click', () => {
        // Act
        render(<TodosPage />);
        fireEvent.click(screen.getByText('Add TODO'));

        // Assert
        expect(mockPush).toHaveBeenCalledWith('/dashboard/todos/new?edit=1');
    });
});