import { NextRequest, NextResponse } from "next/server";
import { auth } from "@/lib/auth"; // Import your Better Auth server instance
import jwt from "jsonwebtoken";

export async function POST(req: NextRequest) {
    try {
        const { email, password } = await req.json();

        // Validate input
        if (!email || !password) {
            return NextResponse.json(
                { error: "Email and password are required" },
                { status: 400 }
            );
        }

        // Use Better Auth's server-side sign in method
        // This will automatically handle password verification against the account table
        const result = await auth.api.signInEmail({
            body: {
                email,
                password,
            },
        });

        if (!result) {
            return NextResponse.json(
                { error: "Invalid email or password" },
                { status: 401 }
            );
        }

        // Validate JWT_SECRET exists
        const jwtSecret = process.env.JWT_SECRET;
        if (!jwtSecret) {
            console.error("JWT_SECRET environment variable is not set");
            return NextResponse.json(
                { error: "Server configuration error" },
                { status: 500 }
            );
        }

        // Generate JWT token for external API access
        const tokenPayload = {
            userId: result.user.id,
            email: result.user.email,
            name: result.user.name,
            iat: Math.floor(Date.now() / 1000),
            exp: Math.floor(Date.now() / 1000) + (60 * 60 * 24 * 7), // 7 days
        };

        const token = jwt.sign(tokenPayload, jwtSecret);

        // Return JWT token instead of session
        return NextResponse.json(
            {
                success: true,
                token,
                user: {
                    id: result.user.id,
                    email: result.user.email,
                    name: result.user.name
                },
                expiresIn: "365d"
            },
            { status: 200 }
        );

    } catch (error) {
        console.error("Authentication error:", error);

        // Handle specific Better Auth errors
        if (error instanceof Error) {
            if (error.message.includes("Invalid credentials")) {
                return NextResponse.json(
                    { error: "Invalid email or password" },
                    { status: 401 }
                );
            }
            if (error.message.includes("User not found")) {
                return NextResponse.json(
                    { error: "No account found with this email" },
                    { status: 404 }
                );
            }
        }

        return NextResponse.json(
            { error: "Authentication failed" },
            { status: 500 }
        );
    }
}