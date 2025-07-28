import { NextRequest, NextResponse } from 'next/server';
import jwt from 'jsonwebtoken';

const JWT_SECRET = process.env.JWT_SECRET || 'dev_secret';

export function verifyToken(req: NextRequest) {
  const auth = req.headers.get('authorization');
  if (!auth || !auth.startsWith('Bearer ')) {
    return NextResponse.json({ error: '未登录' }, { status: 401 });
  }
  try {
    const payload = jwt.verify(auth.slice(7), JWT_SECRET);
    return payload;
  } catch {
    return NextResponse.json({ error: 'Token失效' }, { status: 401 });
  }
}