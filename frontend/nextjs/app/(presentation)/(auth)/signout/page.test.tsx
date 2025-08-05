import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import SignOutPage from './page';
import { useSignOut } from './useSignOut';
import { useRouter } from 'next/navigation';

// Mock useSignOut hook
jest.mock('./useSignOut', () => ({
    useSignOut: jest.fn(),
}));

// Mock next/navigation
jest.mock('next/navigation', () => ({
    useRouter: jest.fn(),
}));

describe('SignOutPage', () => {
    const mockSignOut = jest.fn();
    const mockBack = jest.fn();

    beforeEach(() => {
        jest.clearAllMocks();
        (useRouter as jest.Mock).mockReturnValue({
            back: mockBack,
        });
        (useSignOut as jest.Mock).mockReturnValue({
            signOut: mockSignOut,
            loading: false,
        });
    });

    it('renders sign out confirmation page', () => {
        // Act
        render(<SignOutPage />);

        // Assert
        expect(screen.getByRole('button', { name: /sign out/i })).toBeInTheDocument();
        expect(screen.getByText('Are you sure you want to sign out of your account?')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /^Sign out$/i })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /Cancel/i })).toBeInTheDocument();
    });

    it('calls signOut when sign out button is clicked', () => {
        // Act
        render(<SignOutPage />);
        fireEvent.click(screen.getByRole('button', { name: /^Sign out$/i }));

        // Assert
        expect(mockSignOut).toHaveBeenCalled();
    });

    it('calls router.back when cancel button is clicked', () => {
        // Act
        render(<SignOutPage />);
        fireEvent.click(screen.getByRole('button', { name: /Cancel/i }));

        // Assert
        expect(mockBack).toHaveBeenCalled();
    });

    it('disables buttons when loading', () => {
        // Arrange
        (useSignOut as jest.Mock).mockReturnValue({
            signOut: mockSignOut,
            loading: true,
        });

        // Act
        render(<SignOutPage />);

        // Assert
        expect(screen.getByRole('button', { name: /Signing out.../i })).toBeDisabled();
        expect(screen.getByRole('button', { name: /Cancel/i })).toBeDisabled();
    });

    it('shows loading text when signing out', () => {
        // Arrange
        (useSignOut as jest.Mock).mockReturnValue({
            signOut: mockSignOut,
            loading: true,
        });

        // Act
        render(<SignOutPage />);

        // Assert
        expect(screen.getByText('Signing out...')).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: /^Sign out$/i })).not.toBeInTheDocument();
    });

    it('applies correct button variants and styling', () => {
        // Act
        render(<SignOutPage />);

        // Assert
        const signOutButton = screen.getByRole('button', { name: /^Sign out$/i });
        const cancelButton = screen.getByRole('button', { name: /Cancel/i });
        
        expect(signOutButton).toHaveClass('w-full');
        expect(cancelButton).toHaveClass('w-full');
    });

    it('maintains button state consistency during loading', () => {
        // Arrange
        (useSignOut as jest.Mock).mockReturnValue({
            signOut: mockSignOut,
            loading: true,
        });

        // Act
        render(<SignOutPage />);
        const signOutButton = screen.getByRole('button', { name: /Signing out.../i });
        const cancelButton = screen.getByRole('button', { name: /Cancel/i });

        // Assert
        expect(signOutButton).toBeDisabled();
        expect(cancelButton).toBeDisabled();
        
        // Try clicking disabled buttons
        fireEvent.click(signOutButton);
        fireEvent.click(cancelButton);
        
        expect(mockSignOut).not.toHaveBeenCalled();
        expect(mockBack).not.toHaveBeenCalled();
    });
});