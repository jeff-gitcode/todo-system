import { NextRequest, NextResponse } from "next/server";
import { auth } from "@/lib/auth"; // Import your Better Auth server instance
import jwt from "jsonwebtoken";

export async function POST(req: NextRequest) {
  try {
    const { email, password, name } = await req.json();

    // Validate input
    if (!email || !password) {
      return NextResponse.json(
        { error: "Email and password are required" },
        { status: 400 }
      );
    }

    // Use Better Auth's server-side sign up method
    // This will automatically handle password hashing and account table creation
    const result = await auth.api.signUpEmail({
      body: {
        email,
        password,
        name: name || undefined,
      },
    });

    if (!result) {
      return NextResponse.json(
        { error: "Failed to create account" },
        { status: 400 }
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

    // Return JWT token and user data
    return NextResponse.json(
      {
        success: true,
        token,
        user: {
          id: result.user.id,
          email: result.user.email,
          name: result.user.name
        },
        expiresIn: "7d"
      },
      { status: 201 }
    );

  } catch (error) {
    console.error("Registration error:", error);

    // Handle specific Better Auth errors
    if (error instanceof Error) {
      if (error.message.includes("User already exists") || error.message.includes("already registered")) {
        return NextResponse.json(
          { error: "An account with this email already exists" },
          { status: 409 }
        );
      }
      if (error.message.includes("Invalid email")) {
        return NextResponse.json(
          { error: "Invalid email format" },
          { status: 400 }
        );
      }
      if (error.message.includes("Password")) {
        return NextResponse.json(
          { error: "Password requirements not met" },
          { status: 400 }
        );
      }
    }

    return NextResponse.json(
      { error: "Registration failed" },
      { status: 500 }
    );
  }
}
