import { authUseCase } from './authUseCase';
import { auth } from '@/lib/auth';
import jwt from 'jsonwebtoken';

jest.mock('@/lib/auth', () => ({
    auth: {
        api: {
            signInEmail: jest.fn(),
            signUpEmail: jest.fn(),
        },
    },
}));

jest.mock('jsonwebtoken', () => ({
    sign: jest.fn(() => 'mocked-jwt-token'),
}));

describe('authUseCase', () => {
    const OLD_ENV = process.env;

    beforeEach(() => {
        jest.clearAllMocks();
        process.env = { ...OLD_ENV, JWT_SECRET: 'test-secret' };
    });

    afterAll(() => {
        process.env = OLD_ENV;
    });

    describe('loginWithEmail', () => {
        it('returns error if email or password is missing', async () => {
            expect(await authUseCase.loginWithEmail('', 'pass')).toEqual({ error: "Email and password are required", status: 400 });
            expect(await authUseCase.loginWithEmail('email', '')).toEqual({ error: "Email and password are required", status: 400 });
        });

        it('returns error if signInEmail returns null', async () => {
            (auth.api.signInEmail as unknown as jest.Mock).mockResolvedValue(null);
            const result = await authUseCase.loginWithEmail('a@b.com', 'pass');
            expect(result).toEqual({ error: "Invalid email or password", status: 401 });
        });

        it('returns error if JWT_SECRET is missing', async () => {
            process.env.JWT_SECRET = '';
            (auth.api.signInEmail as unknown as jest.Mock).mockResolvedValue({ user: { id: '1', email: 'a@b.com', name: 'Test' } });
            const result = await authUseCase.loginWithEmail('a@b.com', 'pass');
            expect(result).toEqual({ error: "Server configuration error", status: 500 });
        });

        it('returns success and token if login is valid', async () => {
            (auth.api.signInEmail as unknown as jest.Mock).mockResolvedValue({ user: { id: '1', email: 'a@b.com', name: 'Test' } });
            const result = await authUseCase.loginWithEmail('a@b.com', 'pass');
            expect(jwt.sign).toHaveBeenCalled();
            expect(result).toEqual({
                success: true,
                token: 'mocked-jwt-token',
                user: { id: '1', email: 'a@b.com', name: 'Test' },
                expiresIn: "365d",
                status: 200,
            });
        });
    });

    describe('registerWithEmail', () => {
        it('returns error if email or password is missing', async () => {
            expect(await authUseCase.registerWithEmail('', 'pass', 'Test')).toEqual({ error: "Email and password are required", status: 400 });
            expect(await authUseCase.registerWithEmail('email', '', 'Test')).toEqual({ error: "Email and password are required", status: 400 });
        });

        it('returns error if signUpEmail returns null', async () => {
            (auth.api.signUpEmail as unknown as jest.Mock).mockResolvedValue(null);
            const result = await authUseCase.registerWithEmail('a@b.com', 'pass', 'Test');
            expect(result).toEqual({ error: "Failed to create account", status: 400 });
        });

        it('returns error if JWT_SECRET is missing', async () => {
            process.env.JWT_SECRET = '';
            (auth.api.signUpEmail as unknown as jest.Mock).mockResolvedValue({ user: { id: '1', email: 'a@b.com', name: 'Test' } });
            const result = await authUseCase.registerWithEmail('a@b.com', 'pass', 'Test');
            expect(result).toEqual({ error: "Server configuration error", status: 500 });
        });

        it('returns success and token if registration is valid', async () => {
            (auth.api.signUpEmail as unknown as jest.Mock).mockResolvedValue({ user: { id: '1', email: 'a@b.com', name: 'Test' } });
            const result = await authUseCase.registerWithEmail('a@b.com', 'pass', 'Test');
            expect(jwt.sign).toHaveBeenCalled();
            expect(result).toEqual({
                success: true,
                token: 'mocked-jwt-token',
                user: { id: '1', email: 'a@b.com', name: 'Test' },
                expiresIn: "7d",
                status: 201,
            });
        });

        it('returns error for known error messages', async () => {
            const errorCases = [
                { msg: "User already exists", expected: { error: "An account with this email already exists", status: 409 } },
                { msg: "already registered", expected: { error: "An account with this email already exists", status: 409 } },
                { msg: "Invalid email", expected: { error: "Invalid email format", status: 400 } },
                { msg: "Password too short", expected: { error: "Password requirements not met", status: 400 } },
            ];
            for (const { msg, expected } of errorCases) {
                (auth.api.signUpEmail as unknown as jest.Mock).mockImplementation(() => { throw new Error(msg); });
                const result = await authUseCase.registerWithEmail('a@b.com', 'pass', 'Test');
                expect(result).toEqual(expected);
            }
        });

        it('returns generic error for unknown error', async () => {
            (auth.api.signUpEmail as unknown as jest.Mock).mockImplementation(() => { throw new Error('Something else'); });
            const result = await authUseCase.registerWithEmail('a@b.com', 'pass', 'Test');
            expect(result).toEqual({ error: "Registration failed", status: 500 });
        });
    });
});