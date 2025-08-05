import { renderHook, act } from '@testing-library/react';
import { useSignUp } from './useSignUp';

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
        signUp: {
            email: jest.fn(),
        },
    },
}));

// Import mocked modules after mocking
import { toast } from 'sonner';
import { authClient } from '@/lib/auth-client';

const toastMock = toast as jest.Mocked<typeof toast>;
const signUpEmailMock = authClient.signUp.email as jest.MockedFunction<typeof authClient.signUp.email>;

describe('useSignUp', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should initialize with default state', () => {
        // Act
        const { result } = renderHook(() => useSignUp());

        // Assert
        expect(result.current.error).toBeNull();
        expect(result.current.loading).toBe(false);
        expect(typeof result.current.signUp).toBe('function');
        expect(typeof result.current.setError).toBe('function');
    });

    it('should call signUp.email with correct arguments', async () => {
        // Arrange
        signUpEmailMock.mockResolvedValue(undefined);
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(signUpEmailMock).toHaveBeenCalledWith(
            {
                email: 'test@example.com',
                password: 'password123',
                name: 'John Doe',
            },
            expect.any(Object)
        );
    });

    it('should handle onRequest callback and set loading state', async () => {
        // Arrange
        signUpEmailMock.mockImplementation((params, opts) => {
            opts.onRequest();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Creating account...", {
            description: "Please wait while we create your account.",
        });
        expect(result.current.loading).toBe(true);
    });

    it('should handle onSuccess callback', async () => {
        // Arrange
        signUpEmailMock.mockImplementation((params, opts) => {
            opts.onSuccess();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.success).toHaveBeenCalledWith("Account created!", {
            description: "Welcome! Your account has been created successfully.",
            duration: 2000,
        });
        expect(pushMock).toHaveBeenCalledWith("/dashboard");
    });

    it('should handle onError callback and set error', async () => {
        // Arrange
        signUpEmailMock.mockImplementation((params, opts) => {
            opts.onError({ error: { message: "Email already exists" } });
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(result.current.error).toBe("Email already exists");
        expect(result.current.loading).toBe(false);
        expect(toastMock.error).toHaveBeenCalledWith("Sign up failed", {
            description: "Email already exists",
        });
    });

    it('should reset error when setError(null) is called', () => {
        // Arrange
        const { result } = renderHook(() => useSignUp());
        
        // Act
        act(() => {
            result.current.setError("Some error");
        });
        expect(result.current.error).toBe("Some error");
        
        act(() => {
            result.current.setError(null);
        });

        // Assert
        expect(result.current.error).toBeNull();
    });

    it('should handle complete sign up flow with all callbacks', async () => {
        // Arrange
        signUpEmailMock.mockImplementation((params, opts) => {
            // Simulate the complete success flow
            opts.onRequest();
            opts.onSuccess();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Creating account...", {
            description: "Please wait while we create your account.",
        });
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.success).toHaveBeenCalledWith("Account created!", {
            description: "Welcome! Your account has been created successfully.",
            duration: 2000,
        });
        expect(pushMock).toHaveBeenCalledWith("/dashboard");
    });

    it('should handle error flow correctly', async () => {
        // Arrange
        signUpEmailMock.mockImplementation((params, opts) => {
            // Simulate error flow
            opts.onRequest();
            opts.onError({ error: { message: "Password too weak" } });
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignUp());

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'weak', 'John Doe');
        });

        // Assert
        expect(toastMock.loading).toHaveBeenCalledWith("Creating account...", {
            description: "Please wait while we create your account.",
        });
        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.error).toHaveBeenCalledWith("Sign up failed", {
            description: "Password too weak",
        });
        expect(pushMock).not.toHaveBeenCalled();
        expect(result.current.loading).toBe(false);
        expect(result.current.error).toBe("Password too weak");
    });

    it('should handle rejected promise from authClient.signUp', async () => {
        // Arrange
        signUpEmailMock.mockRejectedValue(new Error('Network error'));
        const { result } = renderHook(() => useSignUp());

        // Act & Assert
        await act(async () => {
            await expect(result.current.signUp('test@example.com', 'password123', 'John Doe')).rejects.toThrow('Network error');
        });

        expect(signUpEmailMock).toHaveBeenCalled();
    });

    it('should clear error before new sign up attempt', async () => {
        // Arrange
        signUpEmailMock.mockResolvedValue(undefined);
        const { result } = renderHook(() => useSignUp());
        
        // Set initial error
        act(() => {
            result.current.setError("Previous error");
        });
        expect(result.current.error).toBe("Previous error");

        // Act
        await act(async () => {
            await result.current.signUp('test@example.com', 'password123', 'John Doe');
        });

        // Assert
        expect(result.current.error).toBeNull();
    });
});