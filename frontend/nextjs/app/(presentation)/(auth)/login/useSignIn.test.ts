import { renderHook, act } from '@testing-library/react';
import { useSignIn } from './useSignIn';

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

// Create the mock function inside the mock
jest.mock('@/lib/auth-client', () => ({
    authClient: {
        signIn: {
            email: jest.fn(),
        },
    },
}));

// Import mocked modules after mocking
import { toast } from 'sonner';
import { authClient } from '@/lib/auth-client';

const toastMock = toast as jest.Mocked<typeof toast>;
const signInEmailMock = authClient.signIn.email as jest.MockedFunction<typeof authClient.signIn.email>;

describe('useSignIn', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should initialize with default state', () => {
        const { result } = renderHook(() => useSignIn());
        expect(result.current.error).toBeNull();
        expect(result.current.loading).toBe(false);
        expect(typeof result.current.signIn).toBe('function');
        expect(typeof result.current.setError).toBe('function');
    });

    it('should call signIn.email with correct arguments', async () => {
        signInEmailMock.mockResolvedValue(undefined);
        const { result } = renderHook(() => useSignIn());

        await act(async () => {
            await result.current.signIn('test@example.com', 'password123');
        });

        expect(signInEmailMock).toHaveBeenCalledWith(
            {
                email: 'test@example.com',
                password: 'password123',
                callbackURL: '/dashboard',
                rememberMe: false,
            },
            expect.any(Object)
        );
    });

    it('should handle onRequest callback', async () => {
        signInEmailMock.mockImplementation((_params, opts) => {
            opts.onRequest();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignIn());

        await act(async () => {
            await result.current.signIn('test@example.com', 'password123');
        });

        expect(toastMock.loading).toHaveBeenCalledWith("Signing in...", {
            description: "Please wait while we sign you in.",
        });
        expect(result.current.loading).toBe(true);
    });

    it('should handle onSuccess callback', async () => {
        signInEmailMock.mockImplementation((_params, opts) => {
            opts.onSuccess();
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignIn());

        await act(async () => {
            await result.current.signIn('test@example.com', 'password123');
        });

        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(toastMock.success).toHaveBeenCalledWith("Success!", {
            description: "You have been signed in successfully.",
            duration: 2000,
        });
        expect(pushMock).toHaveBeenCalledWith("/dashboard");
    });

    it('should handle onError callback and set error', async () => {
        signInEmailMock.mockImplementation((_params, opts) => {
            opts.onError({ error: { message: "Invalid credentials" } });
            return Promise.resolve();
        });
        const { result } = renderHook(() => useSignIn());

        await act(async () => {
            await result.current.signIn('test@example.com', 'wrongpassword');
        });

        expect(toastMock.dismiss).toHaveBeenCalled();
        expect(result.current.error).toBe("Invalid credentials");
        expect(result.current.loading).toBe(false);
        expect(toastMock.error).toHaveBeenCalledWith("Sign in failed", {
            description: "Invalid credentials",
        });
    });

    it('should reset error when setError(null) is called', () => {
        const { result } = renderHook(() => useSignIn());
        act(() => {
            result.current.setError("Some error");
        });
        expect(result.current.error).toBe("Some error");
        act(() => {
            result.current.setError(null);
        });
        expect(result.current.error).toBeNull();
    });
});