'use client';

import Link from 'next/link';
import { useState } from 'react';
import { authClient } from '@/lib/auth-client';
import { Button } from '@/components/ui/button';
import {
    Sheet,
    SheetContent,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from '@/components/ui/sheet';
import { Menu } from 'lucide-react';
import { toast } from 'sonner';

export default function Navigation() {
    const [isOpen, setIsOpen] = useState(false);
    const { data: session, isPending } = authClient.useSession();
    const isAuthenticated = !!session?.user;
    const user = session?.user;

    const handleLogout = async () => {
        try {
            await authClient.signOut({
                fetchOptions: {
                    onRequest: () => {
                        toast.loading("Signing out...");
                    },
                    onSuccess: () => {
                        toast.dismiss();
                        toast.success("Signed out successfully!");
                        window.location.href = '/';
                    },
                    onError: () => {
                        toast.dismiss();
                        toast.error("Failed to sign out");
                    }
                }
            });
        } catch (error) {
            console.error('Logout failed:', error);
            toast.error("Logout failed");
        }
    };

    return (
        <nav className="border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
            <div className="container flex h-14 items-center">
                <div className="mr-4 flex">
                    <Link href="/" className="mr-6 flex items-center space-x-2">
                        <span className="font-bold">Todo System</span>
                    </Link>
                </div>

                {/* Desktop Navigation */}
                <div className="flex flex-1 items-center justify-between space-x-2 md:justify-end">
                    <nav className="flex items-center space-x-6 text-sm font-medium">
                        {isAuthenticated && (
                            <Link
                                href="/dashboard/todos"
                                className="transition-colors hover:text-foreground/80 text-foreground/60"
                            >
                                Todos
                            </Link>
                        )}
                    </nav>

                    <div className="flex items-center space-x-2">
                        {isPending ? (
                            <div className="text-sm text-muted-foreground">Loading...</div>
                        ) : isAuthenticated ? (
                            <>
                                <span className="text-sm text-muted-foreground hidden sm:inline">
                                    {user?.name || user?.email}
                                </span>
                                <Button onClick={handleLogout} variant="ghost" size="sm">
                                    Logout
                                </Button>
                            </>
                        ) : (
                            <>
                                <Button asChild variant="ghost" size="sm">
                                    <Link href="/login">Login</Link>
                                </Button>
                                <Button asChild size="sm">
                                    <Link href="/signup">Sign Up</Link>
                                </Button>
                            </>
                        )}

                        {/* Mobile Menu */}
                        <Sheet open={isOpen} onOpenChange={setIsOpen}>
                            <SheetTrigger asChild>
                                <Button variant="ghost" size="icon" className="md:hidden">
                                    <Menu className="h-5 w-5" />
                                    <span className="sr-only">Toggle menu</span>
                                </Button>
                            </SheetTrigger>
                            <SheetContent side="right">
                                <SheetHeader>
                                    <SheetTitle>Todo System</SheetTitle>
                                </SheetHeader>
                                <div className="grid gap-4 py-6">
                                    <Link
                                        href="/"
                                        className="flex items-center text-sm font-medium"
                                        onClick={() => setIsOpen(false)}
                                    >
                                        Home
                                    </Link>
                                    {isAuthenticated && (
                                        <Link
                                            href="/todos"
                                            className="flex items-center text-sm font-medium"
                                            onClick={() => setIsOpen(false)}
                                        >
                                            Todos
                                        </Link>
                                    )}

                                    <div className="border-t pt-4">
                                        {isAuthenticated ? (
                                            <div className="space-y-2">
                                                <div className="text-sm text-muted-foreground">
                                                    {user?.name || user?.email}
                                                </div>
                                                <Button
                                                    onClick={() => {
                                                        handleLogout();
                                                        setIsOpen(false);
                                                    }}
                                                    variant="outline"
                                                    className="w-full justify-start"
                                                >
                                                    Logout
                                                </Button>
                                            </div>
                                        ) : (
                                            <div className="space-y-2">
                                                <Button asChild variant="outline" className="w-full justify-start">
                                                    <Link href="/login" onClick={() => setIsOpen(false)}>
                                                        Login
                                                    </Link>
                                                </Button>
                                                <Button asChild className="w-full justify-start">
                                                    <Link href="/signup" onClick={() => setIsOpen(false)}>
                                                        Sign Up
                                                    </Link>
                                                </Button>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </SheetContent>
                        </Sheet>
                    </div>
                </div>
            </div>
        </nav>
    );
}
