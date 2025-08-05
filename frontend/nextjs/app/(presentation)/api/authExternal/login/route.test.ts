import { POST } from './route';
import { authUseCase } from '@application/frontend/authUseCase';
import { NextRequest, NextResponse } from 'next/server';

jest.mock('@application/frontend/authUseCase', () => ({
    authUseCase: {
        loginWithEmail: jest.fn(),
    },
}));
jest.mock('next/server', () => ({
    NextRequest: jest.fn(),
    NextResponse: {
        json: jest.fn(),
    },
}));

describe('authExternal/login/route POST', () => {
    let request: NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
        request = {
            json: jest.fn(),
        } as Partial<NextRequest> as NextRequest;
    });

    it('should return success response on valid login', async () => {
        const mockResult = {
            success: true,
            token: 'mock-token',
            user: { id: 'u1', email: 'test@example.com', name: 'Test' },
            expiresIn: '7d',
            status: 200,
        };
        (request.json as jest.Mock).mockResolvedValue({ email: 'test@example.com', password: 'pass' });
        (authUseCase.loginWithEmail as jest.Mock).mockResolvedValue(mockResult);
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(request.json).toHaveBeenCalled();
        expect(authUseCase.loginWithEmail).toHaveBeenCalledWith('test@example.com', 'pass');
        expect(response).toEqual({
            data: {
                success: true,
                token: 'mock-token',
                user: { id: 'u1', email: 'test@example.com', name: 'Test' },
                expiresIn: '7d',
            },
            opts: { status: 200 },
        });
    });

    it('should return error response if usecase returns error', async () => {
        const mockResult = { error: 'Invalid email or password', status: 401 };
        (request.json as jest.Mock).mockResolvedValue({ email: 'bad@example.com', password: 'badpass' });
        (authUseCase.loginWithEmail as jest.Mock).mockResolvedValue(mockResult);
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(authUseCase.loginWithEmail).toHaveBeenCalledWith('bad@example.com', 'badpass');
        expect(response).toEqual({
            data: { error: 'Invalid email or password' },
            opts: { status: 401 },
        });
    });

    it('should handle thrown Error with "Invalid credentials"', async () => {
        (request.json as jest.Mock).mockResolvedValue({ email: 'x', password: 'y' });
        (authUseCase.loginWithEmail as jest.Mock).mockImplementation(() => {
            throw new Error('Invalid credentials');
        });
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(response).toEqual({
            data: { error: 'Invalid email or password' },
            opts: { status: 401 },
        });
    });

    it('should handle thrown Error with "User not found"', async () => {
        (request.json as jest.Mock).mockResolvedValue({ email: 'x', password: 'y' });
        (authUseCase.loginWithEmail as jest.Mock).mockImplementation(() => {
            throw new Error('User not found');
        });
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(response).toEqual({
            data: { error: 'No account found with this email' },
            opts: { status: 404 },
        });
    });

    it('should handle unexpected errors', async () => {
        (request.json as jest.Mock).mockResolvedValue({ email: 'x', password: 'y' });
        (authUseCase.loginWithEmail as jest.Mock).mockImplementation(() => {
            throw new Error('Something else');
        });
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(response).toEqual({
            data: { error: 'Authentication failed' },
            opts: { status: 500 },
        });
    });
});