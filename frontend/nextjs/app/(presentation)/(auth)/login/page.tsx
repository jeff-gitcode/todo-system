"use client";

import { authClient } from "@/lib/auth-client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { toast } from "sonner";

export default function SignInPage() {
    const [error, setError] = useState<string | null>(null);
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    async function handleSignIn(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError(null);

        const form = e.currentTarget;
        const formData = new FormData(form);

        const data = await authClient.signIn.email({
            email: formData.get("email") as string,
            password: formData.get("password") as string,
            callbackURL: "/dashboard",
            rememberMe: false,
        },
            {
                onRequest: (ctx) => {
                    setLoading(true);
                    toast.loading("Signing in...", {
                        description: "Please wait while we sign you in.",
                    });
                },
                onSuccess: (ctx) => {
                    toast.dismiss();
                    toast.success("Success!", {
                        description: "You have been signed in successfully.",
                        duration: 2000,
                    });
                    router.push("/dashboard");
                },
                onError: (ctx) => {
                    toast.dismiss();
                    setError(ctx.error.message);
                    setLoading(false);
                    toast.error("Sign in failed", {
                        description: ctx.error.message,
                    });
                },
            }
        );
    }

    return (
        <div className="min-h-screen grid place-items-center p-8">
            <Card className="w-full max-w-md">
                <CardHeader className="text-center">
                    <CardTitle className="text-2xl font-bold">Sign in to your account</CardTitle>
                    <CardDescription>
                        Don&apos;t have an account?{" "}
                        <Link href="/signup" className="font-medium hover:underline text-primary">
                            Sign up
                        </Link>
                    </CardDescription>
                </CardHeader>

                <CardContent>
                    {error && (
                        <div className="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20 rounded-md mb-6">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSignIn} className="space-y-4">
                        <div className="space-y-2">
                            <label htmlFor="email" className="block text-sm font-medium">
                                Email address
                            </label>
                            <Input
                                id="email"
                                name="email"
                                type="email"
                                autoComplete="email"
                                required
                                placeholder="you@example.com"
                            />
                        </div>

                        <div className="space-y-2">
                            <label htmlFor="password" className="block text-sm font-medium">
                                Password
                            </label>
                            <Input
                                id="password"
                                name="password"
                                type="password"
                                autoComplete="current-password"
                                required
                                placeholder="••••••••"
                            />
                        </div>

                        <Button
                            type="submit"
                            disabled={loading}
                            className="w-full"
                        >
                            {loading ? "Signing in..." : "Sign in"}
                        </Button>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}