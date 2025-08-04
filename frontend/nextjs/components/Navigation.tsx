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
import {
    NavigationMenu,
    NavigationMenuContent,
    NavigationMenuItem,
    NavigationMenuLink,
    NavigationMenuList,
    NavigationMenuTrigger,
} from '@/components/ui/navigation-menu';
import { Menu, X } from 'lucide-react';

export default function Navigation() {
    const [isOpen, setIsOpen] = useState(false);

    // Use the auth client to get session data
    const { data: session, isPending } = authClient.useSession();
    const isAuthenticated = !!session?.user;
    const user = session?.user;

    const handleLogout = async () => {
        try {
            await authClient.signOut({
                fetchOptions: {
                    onSuccess: () => {
                        // Optionally redirect to home page after logout
                        window.location.href = '/';
                    }
                }
            });
        } catch (error) {
            console.error('Logout failed:', error);
        }
    };

    return (
        <nav className="bg-blue-600 text-white shadow-lg">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">
                    <div className="flex items-center">
                        <Link href="/dashboard" className="text-xl font-bold hover:text-blue-200 transition-colors">
                            Todo System
                        </Link>
                    </div>

                    {/* Desktop Menu */}
                    <div className="hidden md:flex items-center space-x-8">
                        <NavigationMenu>
                            <NavigationMenuList>
                                {isAuthenticated && (
                                    <NavigationMenuItem>
                                        <NavigationMenuLink asChild>
                                            <Link href="/todos" className="text-white hover:text-blue-200 transition-colors px-3 py-2">
                                                Todos
                                            </Link>
                                        </NavigationMenuLink>
                                    </NavigationMenuItem>
                                )}
                            </NavigationMenuList>
                        </NavigationMenu>

                        {/* Authentication buttons */}
                        <div className="flex items-center space-x-4">
                            {isPending ? (
                                <div className="text-blue-200">Loading...</div>
                            ) : isAuthenticated ? (
                                <div className="flex items-center space-x-4">
                                    <span className="text-blue-200">
                                        Welcome, {user?.name || user?.email}
                                    </span>
                                    <Button
                                        onClick={handleLogout}
                                        variant="secondary"
                                        className="bg-blue-700 hover:bg-blue-800 text-white"
                                    >
                                        Logout
                                    </Button>
                                </div>
                            ) : (
                                <div className="flex items-center space-x-2">
                                    <Button asChild variant="secondary" className="bg-blue-700 hover:bg-blue-800 text-white">
                                        <Link href="/login">
                                            Login
                                        </Link>
                                    </Button>
                                    <Button asChild className="bg-green-600 hover:bg-green-700 text-white">
                                        <Link href="/signup">
                                            Sign Up
                                        </Link>
                                    </Button>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Mobile menu */}
                    <div className="md:hidden flex items-center">
                        <Sheet open={isOpen} onOpenChange={setIsOpen}>
                            <SheetTrigger asChild>
                                <Button variant="ghost" size="icon" className="text-white hover:text-blue-200 hover:bg-blue-700">
                                    <Menu className="h-6 w-6" />
                                    <span className="sr-only">Open menu</span>
                                </Button>
                            </SheetTrigger>
                            <SheetContent side="right" className="w-[300px] sm:w-[400px]">
                                <SheetHeader>
                                    <SheetTitle>Todo System</SheetTitle>
                                </SheetHeader>
                                <div className="mt-6 space-y-4">
                                    <div className="space-y-2">
                                        <Button asChild variant="ghost" className="w-full justify-start">
                                            <Link href="/" onClick={() => setIsOpen(false)}>
                                                Home
                                            </Link>
                                        </Button>
                                        {isAuthenticated && (
                                            <Button asChild variant="ghost" className="w-full justify-start">
                                                <Link href="/todos" onClick={() => setIsOpen(false)}>
                                                    All Todos
                                                </Link>
                                            </Button>
                                        )}
                                    </div>

                                    {/* Mobile Authentication Section */}
                                    <div className="border-t pt-4 mt-4">
                                        {isPending ? (
                                            <div className="text-muted-foreground">Loading...</div>
                                        ) : isAuthenticated ? (
                                            <div className="space-y-3">
                                                <div className="text-sm text-muted-foreground">
                                                    Welcome, {user?.name || user?.email}
                                                </div>
                                                <Button
                                                    onClick={() => {
                                                        handleLogout();
                                                        setIsOpen(false);
                                                    }}
                                                    variant="outline"
                                                    className="w-full"
                                                >
                                                    Logout
                                                </Button>
                                            </div>
                                        ) : (
                                            <div className="space-y-2">
                                                <Button asChild variant="outline" className="w-full">
                                                    <Link href="/login" onClick={() => setIsOpen(false)}>
                                                        Login
                                                    </Link>
                                                </Button>
                                                <Button asChild className="w-full">
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
