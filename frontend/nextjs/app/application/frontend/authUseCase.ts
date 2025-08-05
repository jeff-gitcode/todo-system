import { auth } from "@/lib/auth";
import jwt from "jsonwebtoken";

export const authUseCase = {
    async loginWithEmail(email: string, password: string): Promise<{ success?: boolean; token?: string; user?: any; expiresIn?: string; status?: number; error?: string }> {
        if (!email || !password) {
            return { error: "Email and password are required", status: 400 };
        }

        const result = await auth.api.signInEmail({ body: { email, password } });
        if (!result) {
            return { error: "Invalid email or password", status: 401 };
        }

        const jwtSecret = process.env.JWT_SECRET;
        if (!jwtSecret) {
            return { error: "Server configuration error", status: 500 };
        }

        const tokenPayload = {
            userId: result.user.id,
            email: result.user.email,
            name: result.user.name,
            iat: Math.floor(Date.now() / 1000),
            exp: Math.floor(Date.now() / 1000) + (60 * 60 * 24 * 7),
        };

        const token = jwt.sign(tokenPayload, jwtSecret);

        return {
            success: true,
            token,
            user: {
                id: result.user.id,
                email: result.user.email,
                name: result.user.name,
            },
            expiresIn: "365d",
            status: 200,
        };
    },

    async registerWithEmail(email: string, password: string, name?: string) {
        if (!email || !password) {
            return { error: "Email and password are required", status: 400 };
        }

        try {
            const result = await auth.api.signUpEmail({
                body: { email, password, name: name || '' },
            });

            if (!result) {
                return { error: "Failed to create account", status: 400 };
            }

            const jwtSecret = process.env.JWT_SECRET;
            if (!jwtSecret) {
                return { error: "Server configuration error", status: 500 };
            }

            const tokenPayload = {
                userId: result.user.id,
                email: result.user.email,
                name: result.user.name,
                iat: Math.floor(Date.now() / 1000),
                exp: Math.floor(Date.now() / 1000) + (60 * 60 * 24 * 7),
            };

            const token = jwt.sign(tokenPayload, jwtSecret);

            return {
                success: true,
                token,
                user: {
                    id: result.user.id,
                    email: result.user.email,
                    name: result.user.name,
                },
                expiresIn: "7d",
                status: 201,
            };
        } catch (error: unknown) {
            if (error instanceof Error) {
                if (
                    error.message.includes("User already exists") ||
                    error.message.includes("already registered")
                ) {
                    return { error: "An account with this email already exists", status: 409 };
                }
                if (error.message.includes("Invalid email")) {
                    return { error: "Invalid email format", status: 400 };
                }
                if (error.message.includes("Password")) {
                    return { error: "Password requirements not met", status: 400 };
                }
            }
            return { error: "Registration failed", status: 500 };
        }
    },
};