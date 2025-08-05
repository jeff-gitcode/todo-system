import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { authClient } from "@/lib/auth-client";

export function useSignOut() {
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    async function signOut() {
        await authClient.signOut({
            fetchOptions: {
                onRequest: () => {
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
                    router.push("/login");
                },
                onError: () => {
                    toast.dismiss();
                    setLoading(false);
                    toast.error("Sign out failed", {
                        description: "There was an error signing you out. Please try again.",
                    });
                },
                onResponse: () => {
                    setLoading(false);
                },
            },
        });
    }

    return { signOut, loading };
}