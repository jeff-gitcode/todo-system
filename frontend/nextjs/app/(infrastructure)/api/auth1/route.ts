// import { NextRequest, NextResponse } from 'next/server';
// import bcrypt from 'bcryptjs';
// import jwt from 'jsonwebtoken';
// // 如用Prisma
// // import { PrismaClient } from '@prisma/client';
// // const prisma = new PrismaClient();

// const JWT_SECRET = process.env.JWT_SECRET || 'dev_secret';

// const users: { id: string; email: string; password: string }[] = []; // DEMO: 用内存，生产用数据库

// function findUserByEmail(email: string) {
//   return users.find(u => u.email === email);
// }

// function findUserById(id: string) {
//   return users.find(u => u.id === id);
// }

// export async function POST(req: NextRequest) {
//   const { pathname } = new URL(req.url);
//   const body = await req.json();

//   // 注册
//   if (pathname.endsWith('/register')) {
//     const { email, password } = body;
//     if (!email || !password) return NextResponse.json({ error: '参数错误' }, { status: 400 });
//     if (findUserByEmail(email)) return NextResponse.json({ error: '邮箱已注册' }, { status: 409 });
//     const hash = await bcrypt.hash(password, 10);
//     const user = { id: crypto.randomUUID(), email, password: hash };
//     users.push(user);
//     return NextResponse.json({ id: user.id, email: user.email }, { status: 201 });
//   }

//   // 登录
//   if (pathname.endsWith('/login')) {
//     const { email, password } = body;
//     const user = findUserByEmail(email);
//     if (!user || !(await bcrypt.compare(password, user.password))) {
//       return NextResponse.json({ error: '认证失败' }, { status: 401 });
//     }
//     const token = jwt.sign({ id: user.id, email: user.email }, JWT_SECRET, { expiresIn: '7d' });
//     return NextResponse.json({ token }, { status: 200 });
//   }

//   // 登出（前端清除token即可）
//   if (pathname.endsWith('/logout')) {
//     return NextResponse.json({ message: '已登出' }, { status: 200 });
//   }

//   return NextResponse.json({ error: '未知操作' }, { status: 404 });
// }