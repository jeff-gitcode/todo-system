import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import SignInPage from './page';
import { useSignIn } from './useSignIn';

// Arrange: Mock useSignIn hook
jest.mock('./useSignIn', () => {
    return {
        useSignIn: jest.fn(),
    };
});

jest.mock('next/navigation', () => ({
    useRouter: () => ({
        push: jest.fn(),
        back: jest.fn(),
    }),
    usePathname: () => '/',
}))

// Mock nanostores if needed
jest.mock('nanostores', () => ({
    atom: jest.fn(() => ({
        get: jest.fn(),
        set: jest.fn(),
        subscribe: jest.fn(),
    })),
}))

describe('SignInPage', () => {
    const mockSignIn = jest.fn();
    const mockSetError = jest.fn();

    beforeEach(() => {
        jest.clearAllMocks();
        (useSignIn as jest.Mock).mockReturnValue({
            signIn: mockSignIn,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    });

    it('renders sign in form', () => {
        // Act
        render(<SignInPage />);

        // Assert
        expect(screen.getByText('Sign in to your account')).toBeInTheDocument();
        expect(screen.getByLabelText('Email address')).toBeInTheDocument();
        expect(screen.getByLabelText('Password')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /Sign in/i })).toBeInTheDocument();
        expect(screen.getByText(/Sign up/)).toBeInTheDocument();
    });

    it('shows error message when error is set', () => {
        // Arrange
        (useSignIn as jest.Mock).mockReturnValue({
            signIn: mockSignIn,
            loading: false,
            error: 'Invalid credentials',
            setError: mockSetError,
        });

        // Act
        render(<SignInPage />);

        // Assert
        expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });

    it('calls signIn with email and password on form submit', () => {
        // Act
        render(<SignInPage />);
        fireEvent.change(screen.getByLabelText('Email address'), { target: { value: 'test@example.com' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /Sign in/i }));

        // Assert
        expect(mockSetError).toHaveBeenCalledWith(null);
        expect(mockSignIn).toHaveBeenCalledWith('test@example.com', 'password123');
    });

    it('disables submit button when loading', () => {
        // Arrange
        (useSignIn as jest.Mock).mockReturnValue({
            signIn: mockSignIn,
            loading: true,
            error: null,
            setError: mockSetError,
        });

        // Act
        render(<SignInPage />);

        // Assert
        expect(screen.getByRole('button', { name: /Signing in.../i })).toBeDisabled();
    });
});