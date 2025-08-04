import { Suspense } from "react";
import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import { auth } from "@/lib/auth"; // Server-side auth
// Update this import to match the actual export from the skeleton module
import { Skeleton } from "@/components/ui/skeleton";
// If DashboardSkeleton is needed, you can use Skeleton or create DashboardSkeleton in the skeleton module
import { DashboardClient } from "@presentation/components/DashboardClient";

// Remove "use client" - this becomes a server component
export default async function DashboardPage() {
    // Server-side session validation
    const session = await auth.api.getSession({
        headers: {
            cookie: cookies().toString(),
        },
    });

    // Server-side redirect instead of useRouter
    if (!session?.user) {
        redirect("/login");
    }

    // Server-side data prefetching (optional)
    let initialTodos = [];
    try {
        const response = await fetch(`${process.env.NEXTAUTH_URL}/api/todos`, {
            headers: {
                Authorization: `Bearer ${session.session.token}`,
            },
            cache: "no-store", // Always fresh for user data
        });

        if (response.ok) {
            initialTodos = await response.json();
        }
    } catch (error) {
        console.error("Failed to prefetch todos:", error);
    }

    return (
        <div className="min-h-screen grid place-items-center p-8">
            <Suspense fallback={<Skeleton />}>
                <div className="w-full max-w-md space-y-8">
                    <div className="text-center">
                        <h1 className="text-2xl font-bold">Dashboard</h1>
                        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                            Welcome to your personal dashboard
                        </p>
                    </div>

                    <div className="mt-8 space-y-6">
                        <div className="space-y-4">
                            <div className="p-4 rounded-md border border-black/[.08] dark:border-white/[.145] bg-transparent">
                                <h2 className="text-sm font-medium mb-2">
                                    Profile Information
                                </h2>
                                <div className="space-y-2">
                                    <p className="text-sm">
                                        <span className="text-gray-600 dark:text-gray-400">
                                            Name:{" "}
                                        </span>
                                        {session.user.name || "Not set"}
                                    </p>
                                    <p className="text-sm">
                                        <span className="text-gray-600 dark:text-gray-400">
                                            Email:{" "}
                                        </span>
                                        {session.user.email}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Client component for interactive features */}
                    <DashboardClient
                        user={session.user}
                        initialTodos={initialTodos}
                    />
                </div>
            </Suspense>
        </div>
    );
}

// Force dynamic rendering for user-specific content
export const dynamic = "force-dynamic";
export const revalidate = 0;