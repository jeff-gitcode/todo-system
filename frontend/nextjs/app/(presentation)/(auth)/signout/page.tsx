"use client";
import React, { useState } from "react";

import { authClient } from "@/lib/auth-client";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { toast } from "sonner";

export default function SignOutPage() {
    const router = useRouter();
    const [loading, setLoading] = useState(false);

    async function handleSignOut() {
        await authClient.signOut({
            fetchOptions: {
                onRequest: (ctx) => {
                    setLoading(true);
                    toast.loading("Signing out...", {
                        description: "Please wait while we sign you out.",
                    });
                },
                onSuccess: () => {
                    toast.dismiss();
                    toast.success("Signed out successfully!", {
                        description: "You have been signed out of your account.",
                        duration: 2000,
                    });
                    router.push("/login"); // redirect to login page
                },
                onError: (ctx) => {
                    toast.dismiss();
                    setLoading(false);
                    toast.error("Sign out failed", {
                        description: "There was an error signing you out. Please try again.",
                    });
                },
                onResponse: (ctx) => {
                    setLoading(false);
                },
            },
        });
    }

    return (
        <div className="min-h-screen grid place-items-center p-8">
            <Card className="w-full max-w-md">
                <CardHeader className="text-center">
                    <CardTitle className="text-2xl font-bold">Sign out</CardTitle>
                    <CardDescription>
                        Are you sure you want to sign out of your account?
                    </CardDescription>
                </CardHeader>

                <CardContent>
                    <div className="flex flex-col gap-4">
                        <Button
                            onClick={handleSignOut}
                            disabled={loading}
                            variant="destructive"
                            className="w-full"
                        >
                            {loading ? "Signing out..." : "Sign out"}
                        </Button>

                        <Button
                            onClick={() => router.back()}
                            variant="outline"
                            className="w-full"
                            disabled={loading}
                        >
                            Cancel
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
