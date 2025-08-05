import { POST } from './route';
import { authUseCase } from '@application/frontend/authUseCase';
import { NextRequest, NextResponse } from 'next/server';

jest.mock('@application/frontend/authUseCase', () => ({
    authUseCase: {
        registerWithEmail: jest.fn(),
    },
}));
jest.mock('next/server', () => ({
    NextRequest: jest.fn(),
    NextResponse: {
        json: jest.fn(),
    },
}));

describe('authExternal/register/route POST', () => {
    let request: NextRequest;

    beforeEach(() => {
        jest.clearAllMocks();
        request = {
            json: jest.fn(),
        } as Partial<NextRequest> as NextRequest;
    });

    it('should return success response on valid registration', async () => {
        const mockResult = {
            success: true,
            token: 'mock-token',
            user: { id: 'u1', email: 'test@example.com', name: 'Test' },
            expiresIn: '7d',
            status: 201,
        };
        (request.json as jest.Mock).mockResolvedValue({ email: 'test@example.com', password: 'pass', name: 'Test' });
        (authUseCase.registerWithEmail as jest.Mock).mockResolvedValue(mockResult);
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(request.json).toHaveBeenCalled();
        expect(authUseCase.registerWithEmail).toHaveBeenCalledWith('test@example.com', 'pass', 'Test');
        expect(response).toEqual({
            data: {
                success: true,
                token: 'mock-token',
                user: { id: 'u1', email: 'test@example.com', name: 'Test' },
                expiresIn: '7d',
            },
            opts: { status: 201 },
        });
    });

    it('should return error response if usecase returns error', async () => {
        const mockResult = { error: 'Email already exists', status: 409 };
        (request.json as jest.Mock).mockResolvedValue({ email: 'test@example.com', password: 'pass', name: 'Test' });
        (authUseCase.registerWithEmail as jest.Mock).mockResolvedValue(mockResult);
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(authUseCase.registerWithEmail).toHaveBeenCalledWith('test@example.com', 'pass', 'Test');
        expect(response).toEqual({
            data: { error: 'Email already exists' },
            opts: { status: 409 },
        });
    });

    it('should handle unexpected errors', async () => {
        (request.json as jest.Mock).mockResolvedValue({ email: 'x', password: 'y', name: 'Test' });
        (authUseCase.registerWithEmail as jest.Mock).mockImplementation(() => {
            throw new Error('Something else');
        });
        (NextResponse.json as jest.Mock).mockImplementation((data, opts) => ({ data, opts }));

        const response = await POST(request);

        expect(response).toEqual({
            data: { error: 'Registration failed' },
            opts: { status: 500 },
        });
    });
});