import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { authClient } from "@/lib/auth-client";

export function useSignUp() {
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    async function signUp(email: string, password: string, name: string) {
        setError(null);
        await authClient.signUp.email(
            { email, password, name },
            {
                onRequest: () => {
                    setLoading(true);
                    toast.loading("Creating account...", {
                        description: "Please wait while we create your account.",
                    });
                },
                onSuccess: () => {
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

    return { signUp, loading, error, setError };
}