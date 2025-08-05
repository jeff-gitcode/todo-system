import { renderHook, act } from '@testing-library/react';
import { useSignOut } from './useSignOut';

// Mocks
const pushMock = jest.fn();

jest.mock('next/navigation', () => ({
    useRouter: () => ({
        push: pushMock,
    }),
}));

jest.mock('sonner', () => ({
    toast: {
        loading: jest.fn(),
        success: jest.fn(),
        error: jest.fn(),
        dismiss: jest.fn(),
    },
}));

jest.mock('@/lib/auth-client', () => ({
    authClient: {
        signOut: jest.fn(),
    },
}));

// Import mocked modules after mocking
import { toast } from 'sonner';
import { authClient } from '@/lib/auth-client';

const toastMock = toast as jest.Mocked<typeof toast>;
const signOutMock = authClient.signOut as jest.MockedFunction<typeof authClient.signOut>;

describe('useSignOut', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should initialize with default state', () => {
        // Act
        const { result } = renderHook(() => useSignOut());

        // Assert
        expect(result.current.loading).toBe(false);
        expect(typeof result.current.signOut).toBe('function');
    });

    it('should call authClient.signOut with correct options', async () => {
        // Arrange
        signOutMock.mockResolvedValue(undefined);
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(signOutMock).toHaveBeenCalledWith({
            fetchOptions: expect.objectContaining({
                onRequest: expect.any(Function),
                onSuccess: expect.any(Function),
                onError: expect.any(Function),
                onResponse: expect.any(Function),
            }),
        });
    });

    it('should handle onRequest callback and set loading state', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            options.fetchOptions.onRequest();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Signing out...", {
            description: "Please wait while we sign you out.",
        });
        expect(result.current.loading).toBe(true);
    });

    it('should handle onSuccess callback', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            options.fetchOptions.onSuccess();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.success).toHaveBeenCalledWith("Signed out successfully!", {
            description: "You have been signed out of your account.",
            duration: 2000,
        });
        expect(pushMock).toHaveBeenCalledWith("/login");
    });

    it('should handle onError callback and reset loading state', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            options.fetchOptions.onError();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(result.current.loading).toBe(false);
        expect(toastMock.error).toHaveBeenCalledWith("Sign out failed", {
            description: "There was an error signing you out. Please try again.",
        });
    });

    it('should handle onResponse callback and reset loading state', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            options.fetchOptions.onResponse();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(result.current.loading).toBe(false);
    });

    it('should handle complete sign out flow with all callbacks', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            // Simulate the complete flow
            options.fetchOptions.onRequest();
            options.fetchOptions.onSuccess();
            options.fetchOptions.onResponse();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Signing out...", {
            description: "Please wait while we sign you out.",
        });
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.success).toHaveBeenCalledWith("Signed out successfully!", {
            description: "You have been signed out of your account.",
            duration: 2000,
        });
        expect(pushMock).toHaveBeenCalledWith("/login");
        expect(result.current.loading).toBe(false);
    });

    it('should handle error flow correctly', async () => {
        // Arrange
        signOutMock.mockImplementation((options) => {
            // Simulate error flow
            options.fetchOptions.onRequest();
            options.fetchOptions.onError();
            options.fetchOptions.onResponse();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignOut());

        // Act
        await act(async () => {
            await result.current.signOut();
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Signing out...", {
            description: "Please wait while we sign you out.",
        });
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.error).toHaveBeenCalledWith("Sign out failed", {
            description: "There was an error signing you out. Please try again.",
        });
        expect(pushMock).not.toHaveBeenCalled();
        expect(result.current.loading).toBe(false);
    });

    it('should handle rejected promise from authClient.signOut', async () => {
        // Arrange
        signOutMock.mockRejectedValue(new Error('Network error'));
        const { result } = renderHook(() => useSignOut());

        // Act & Assert
        await act(async () => {
            await expect(result.current.signOut()).rejects.toThrow('Network error');
        });

        expect(signOutMock).toHaveBeenCalled();
    });
});