import { verifyToken } from './verifyToken';
import { NextResponse } from 'next/server';
import jwt from 'jsonwebtoken';

jest.mock('next/server', () => ({
  NextResponse: {
    json: jest.fn(),
  },
}));

jest.mock('jsonwebtoken', () => ({
  verify: jest.fn(),
}));

describe('verifyToken', () => {
  let req: any;

  beforeEach(() => {
    jest.clearAllMocks();
    req = {
      headers: {
        get: jest.fn(),
      },
    };
  });

  it('should return 未登录 if authorization header is missing', () => {
    req.headers.get.mockReturnValue(undefined);
    (NextResponse.json as jest.Mock).mockReturnValue('no-auth-response');

    const result = verifyToken(req);

    expect(NextResponse.json).toHaveBeenCalledWith({ error: '未登录' }, { status: 401 });
    expect(result).toBe('no-auth-response');
  });

  it('should return 未登录 if authorization header does not start with Bearer', () => {
    req.headers.get.mockReturnValue('Basic xyz');
    (NextResponse.json as jest.Mock).mockReturnValue('no-bearer-response');

    const result = verifyToken(req);

    expect(NextResponse.json).toHaveBeenCalledWith({ error: '未登录' }, { status: 401 });
    expect(result).toBe('no-bearer-response');
  });

  it('should return payload if token is valid', () => {
    req.headers.get.mockReturnValue('Bearer validtoken');
    const mockPayload = { id: 'user1', email: 'user@example.com' };
    (jwt.verify as jest.Mock).mockReturnValue(mockPayload);

    const result = verifyToken(req);

    expect(jwt.verify).toHaveBeenCalledWith('validtoken', expect.any(String));
    expect(result).toBe(mockPayload);
  });

  it('should return Token失效 if token is invalid', () => {
    req.headers.get.mockReturnValue('Bearer invalidtoken');
    (jwt.verify as jest.Mock).mockImplementation(() => { throw new Error('invalid'); });
    (NextResponse.json as jest.Mock).mockReturnValue('invalid-token-response');

    const result = verifyToken(req);

    expect(jwt.verify).toHaveBeenCalledWith('invalidtoken', expect.any(String));
    expect(NextResponse.json).toHaveBeenCalledWith({ error: 'Token失效' }, { status: 401 });
    expect(result).toBe('invalid-token-response');
  });
});