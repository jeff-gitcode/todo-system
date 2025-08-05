import React from 'react';
import { render } from '@testing-library/react';
import { DashboardClient } from './DashboardClient';
import { toast } from 'sonner';

// Mock the toast library
jest.mock('sonner', () => ({
    toast: {
        success: jest.fn(),
    },
}));

// Mock TodosPage component
jest.mock('@presentation/(protected)/dashboard/todos/page', () => {
    const MockTodosPage = () => <div data-testid="todos-page" />;
    MockTodosPage.displayName = 'MockTodosPage';
    return MockTodosPage;
});

describe('DashboardClient', () => {
    const user = {
        id: 'user-1',
        name: 'Test User',
        email: 'test@example.com',
    };

    const initialTodos = [
        { id: '1', title: 'Todo 1', completed: false },
        { id: '2', title: 'Todo 2', completed: true },
    ];

    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should render TodosPage', () => {
        // Arrange
        // (No setup needed beyond beforeEach)

        // Act
        const { getByTestId } = render(
            <DashboardClient user={user} initialTodos={initialTodos} />
        );

        // Assert
        expect(getByTestId('todos-page')).toBeInTheDocument();
    });

    it('should call toast.success with correct message and description on mount (with name)', () => {
        // Arrange
        // (No setup needed beyond beforeEach)

        // Act
        render(<DashboardClient user={user} initialTodos={initialTodos} />);

        // Assert
        expect(toast.success).toHaveBeenCalledWith("Welcome back!", {
            description: `Good to see you again, ${user.name}`,
            duration: 3000,
        });
    });

    it('should call toast.success with email if name is not provided', () => {
        // Arrange
        const userWithoutName = { ...user, name: undefined };

        // Act
        render(<DashboardClient user={userWithoutName} initialTodos={initialTodos} />);

        // Assert
        expect(toast.success).toHaveBeenCalledWith("Welcome back!", {
            description: `Good to see you again, ${user.email}`,
            duration: 3000,
        });
    });
});