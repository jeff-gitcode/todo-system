"use client";
import React from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useSignOut } from "./useSignOut";
import { useRouter } from "next/navigation";

export default function SignOutPage() {
    const { signOut, loading } = useSignOut();
    const router = useRouter();

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
                            onClick={signOut}
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
