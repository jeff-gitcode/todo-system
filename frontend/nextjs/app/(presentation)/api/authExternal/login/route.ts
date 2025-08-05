import { NextRequest, NextResponse } from "next/server";
import { authUseCase } from "@application/frontend/authUseCase";

export async function POST(req: NextRequest) {
    try {
        const { email, password } = await req.json();
        const result = await authUseCase.loginWithEmail(email, password);

        if (result.error) {
            return NextResponse.json({ error: result.error }, { status: result.status });
        }

        return NextResponse.json(
            {
                success: true,
                token: result.token,
                user: result.user,
                expiresIn: result.expiresIn,
            },
            { status: result.status }
        );
    } catch (error) {
        console.error("Authentication error:", error);

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