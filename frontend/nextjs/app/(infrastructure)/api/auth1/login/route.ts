import { PrismaClient } from "@prisma/client";
import { NextRequest, NextResponse } from "next/server";
import bcrypt from "bcryptjs";
import { db } from '@lib/db';
import jwt from 'jsonwebtoken';

const JWT_SECRET = process.env.JWT_SECRET || "dev_secret";
export async function POST(req: NextRequest) {
    const { email, password } = await req.json();
    console.log(email, password);
    const user = await db.user.findUnique({
        where: { email },
    });

    if (!user) {
        return NextResponse.json(
            { error: "No user found with this email" },
            { status: 400 },
        );
    }

    const isValid = await bcrypt.compare(password, user.password!);
    console.log(isValid);
    if (!isValid) {
        return NextResponse.json({ error: "Invalid password" }, { status: 400 });
    }

    const token = jwt.sign({ id: user.id, email: user.email }, JWT_SECRET, { expiresIn: '7d' });
    return NextResponse.json({ token }, { status: 200 });
}