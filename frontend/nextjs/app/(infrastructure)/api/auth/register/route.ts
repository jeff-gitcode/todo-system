import { NextRequest, NextResponse } from 'next/server';
import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { db } from '@lib/db';
// 如用Prisma
// import { PrismaClient } from '@prisma/client';
// const prisma = new PrismaClient();

const JWT_SECRET = process.env.JWT_SECRET || 'dev_secret';
// DEMO: 用内存，生产用数据库

async function findUserByEmail(email: string) {
  return await db.user.findUnique({
    where: { email }
  });
}

async function findUserById(id: string) {
  return await db.user.findUnique({
    where: { id }
  });
}
export async function POST(req: NextRequest) {
  const body = await req.json();

  // register
  const { email, password } = body;
  if (!email || !password) return NextResponse.json({ error: '参数错误' }, { status: 400 });
  if (await findUserByEmail(email)) return NextResponse.json({ error: '邮箱已注册' }, { status: 409 });
  const hash = await bcrypt.hash(password, 10);
  const user = { id: crypto.randomUUID(), email, password: hash };
  await db.user.create({ data: user });
  return NextResponse.json({ id: user.id, email: user.email }, { status: 201 });
}
