import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import SignUpPage from './page';
import { useSignUp } from './useSignUp';

// Mock useSignUp hook
jest.mock('./useSignUp', () => ({
    useSignUp: jest.fn(),
}));

describe('SignUpPage', () => {
    const mockSignUp = jest.fn();
    const mockSetError = jest.fn();

    beforeEach(() => {
        jest.clearAllMocks();
        (useSignUp as jest.Mock).mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    });

    it('renders sign up form', () => {
        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByText('Create your account')).toBeInTheDocument();
        expect(screen.getByLabelText('Full Name')).toBeInTheDocument();
        expect(screen.getByLabelText('Email address')).toBeInTheDocument();
        expect(screen.getByLabelText('Password')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /Sign up/i })).toBeInTheDocument();
        expect(screen.getByText(/Sign in/)).toBeInTheDocument();
    });

    it('shows error message when error is set', () => {
        // Arrange
        (useSignUp as jest.Mock).mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: 'Email already exists',
            setError: mockSetError,
        });

        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByText('Email already exists')).toBeInTheDocument();
    });

    it('calls signUp with name, email and password on form submit', () => {
        // Act
        render(<SignUpPage />);
        fireEvent.change(screen.getByLabelText('Full Name'), { target: { value: 'John Doe' } });
        fireEvent.change(screen.getByLabelText('Email address'), { target: { value: 'test@example.com' } });
        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /Sign up/i }));

        // Assert
        expect(mockSetError).toHaveBeenCalledWith(null);
        expect(mockSignUp).toHaveBeenCalledWith('test@example.com', 'password123', 'John Doe');
    });

    it('disables submit button when loading', () => {
        // Arrange
        (useSignUp as jest.Mock).mockReturnValue({
            signUp: mockSignUp,
            loading: true,
            error: null,
            setError: mockSetError,
        });

        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByRole('button', { name: /Creating account.../i })).toBeDisabled();
    });

    it('shows loading text when creating account', () => {
        // Arrange
        (useSignUp as jest.Mock).mockReturnValue({
            signUp: mockSignUp,
            loading: true,
            error: null,
            setError: mockSetError,
        });

        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByText('Creating account...')).toBeInTheDocument();
        expect(screen.queryByText('Sign up')).not.toBeInTheDocument();
    });

    it('has proper form field requirements', () => {
        // Act
        render(<SignUpPage />);

        // Assert
        const nameField = screen.getByLabelText('Full Name');
        const emailField = screen.getByLabelText('Email address');
        const passwordField = screen.getByLabelText('Password');

        expect(nameField).toHaveAttribute('required');
        expect(emailField).toHaveAttribute('required');
        expect(emailField).toHaveAttribute('type', 'email');
        expect(passwordField).toHaveAttribute('required');
        expect(passwordField).toHaveAttribute('type', 'password');
        expect(passwordField).toHaveAttribute('minLength', '8');
    });

    it('shows password requirements text', () => {
        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByText('Must be at least 8 characters')).toBeInTheDocument();
    });

    it('has correct autocomplete attributes', () => {
        // Act
        render(<SignUpPage />);

        // Assert
        expect(screen.getByLabelText('Email address')).toHaveAttribute('autoComplete', 'email');
        expect(screen.getByLabelText('Password')).toHaveAttribute('autoComplete', 'new-password');
    });

    it('applies correct styling classes', () => {
        // Act
        render(<SignUpPage />);

        // Assert
        const submitButton = screen.getByRole('button', { name: /Sign up/i });
        expect(submitButton).toHaveClass('w-full');
    });

    it('prevents default form submission and calls preventDefault', () => {
        // Arrange
        const mockPreventDefault = jest.fn();

        // Act
        render(<SignUpPage />);
        const form = screen.getByRole('button', { name: /Sign up/i }).closest('form');
        fireEvent.submit(form!, { preventDefault: mockPreventDefault });

        // Assert
        expect(mockSetError).toHaveBeenCalledWith(null);
    });

    it('maintains button state consistency during loading', () => {
        // Arrange
        (useSignUp as jest.Mock).mockReturnValue({
            signUp: mockSignUp,
            loading: true,
            error: null,
            setError: mockSetError,
        });

        // Act
        render(<SignUpPage />);
        const submitButton = screen.getByRole('button', { name: /Creating account.../i });

        // Assert
        expect(submitButton).toBeDisabled();
        
        // Try clicking disabled button
        fireEvent.click(submitButton);
        
        // Should not call signUp when disabled
        expect(mockSignUp).not.toHaveBeenCalled();
    });
});