"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { authClient } from "@/lib/auth-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { toast } from "sonner";

export default function SignUpPage() {
    const [error, setError] = useState<string | null>(null);
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    async function handleSignUp(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        setError(null);

        const form = e.currentTarget;
        const formData = new FormData(form);

        const data = await authClient.signUp.email({
            email: formData.get("email") as string,
            password: formData.get("password") as string,
            name: formData.get("name") as string,
        },
            {
                onRequest: (ctx) => {
                    setLoading(true);
                    toast.loading("Creating account...", {
                        description: "Please wait while we create your account.",
                    });
                },
                onSuccess: (ctx) => {
                    toast.dismiss();
                    toast.success("Account created!", {
                        description: "Welcome! Your account has been created successfully.",
                        duration: 2000,
                    });
                    router.push("/dashboard");
                },
                onError: (ctx) => {
                    toast.dismiss();
                    setError(ctx.error.message);
                    setLoading(false);
                    toast.error("Sign up failed", {
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
                    <CardTitle className="text-2xl font-bold">Create your account</CardTitle>
                    <CardDescription>
                        Already have an account?{" "}
                        <Link href="/login" className="font-medium hover:underline text-primary">
                            Sign in
                        </Link>
                    </CardDescription>
                </CardHeader>

                <CardContent>
                    {error && (
                        <div className="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20 rounded-md mb-6">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSignUp} className="space-y-4">
                        <div className="space-y-2">
                            <label htmlFor="name" className="block text-sm font-medium">
                                Full Name
                            </label>
                            <Input
                                id="name"
                                name="name"
                                type="text"
                                required
                                placeholder="John Doe"
                            />
                        </div>

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
                                autoComplete="new-password"
                                required
                                minLength={8}
                                placeholder="••••••••"
                            />
                            <p className="text-xs text-muted-foreground">
                                Must be at least 8 characters
                            </p>
                        </div>

                        <Button
                            type="submit"
                            disabled={loading}
                            className="w-full"
                        >
                            {loading ? "Creating account..." : "Sign up"}
                        </Button>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}