import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { authClient } from "@/lib/auth-client";

export function useSignIn() {
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    async function signIn(email: string, password: string) {
        setError(null);
        await authClient.signIn.email(
            {
                email,
                password,
                callbackURL: "/dashboard",
                rememberMe: false,
            },
            {
                onRequest: () => {
                    setLoading(true);
                    toast.loading("Signing in...", {
                        description: "Please wait while we sign you in.",
                    });
                },
                onSuccess: () => {
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

    return { signIn, loading, error, setError };
}