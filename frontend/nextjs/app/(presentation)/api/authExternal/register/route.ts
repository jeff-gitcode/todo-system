import { NextRequest, NextResponse } from "next/server";
import { authUseCase } from "@application/frontend/authUseCase";

export async function POST(req: NextRequest) {
  try {
    const { email, password, name } = await req.json();
    const result = await authUseCase.registerWithEmail(email, password, name);

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
    console.error("Registration error:", error);
    return NextResponse.json(
      { error: "Registration failed" },
      { status: 500 }
    );
  }
}
