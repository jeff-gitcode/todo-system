'use client';

import Link from 'next/link';
import { useState } from 'react';
import { authClient } from '@/lib/auth-client';

interface User {
    id: string;
    email: string;
    name?: string;
}

export default function Navigation() {
    const [isOpen, setIsOpen] = useState(false);

    // Use the auth client to get session data
    const { data: session, isPending } = authClient.useSession();
    const isAuthenticated = !!session?.user;
    const user = session?.user;

    return (
        <nav className="bg-blue-600 text-white shadow-lg">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-16">
                    <div className="flex items-center">
                        <Link href="/" className="text-xl font-bold">
                            Todo System
                        </Link>
                    </div>

                    {/* Desktop Menu */}
                    <div className="hidden md:flex items-center space-x-8">
                        <Link href="/" className="hover:text-blue-200 transition-colors">
                            Home
                        </Link>
                        {isAuthenticated && (
                            <>
                                <Link href="/todos" className="hover:text-blue-200 transition-colors">
                                    All Todos
                                </Link>
                                <Link href="/todos/add" className="hover:text-blue-200 transition-colors">
                                    Add Todo
                                </Link>
                                <Link href="/completed" className="hover:text-blue-200 transition-colors">
                                    Completed
                                </Link>
                            </>
                        )}

                        {/* Authentication buttons */}
                        <div className="flex items-center space-x-4">
                            {isPending ? (
                                <div className="text-blue-200">Loading...</div>
                            ) : isAuthenticated ? (
                                <div className="flex items-center space-x-4">
                                    <span className="text-blue-200">
                                        Welcome, {user?.name || user?.email}
                                    </span>
                                    <Link
                                        href="/signout"
                                        className="bg-blue-700 hover:bg-blue-800 px-4 py-2 rounded-md transition-colors"
                                    >
                                        Logout
                                    </Link>
                                </div>
                            ) : (
                                <div className="flex items-center space-x-2">
                                    <Link
                                        href="/login"
                                        className="bg-blue-700 hover:bg-blue-800 px-4 py-2 rounded-md transition-colors"
                                    >
                                        Login
                                    </Link>
                                    <Link
                                        href="/signup"
                                        className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded-md transition-colors"
                                    >
                                        Sign Up
                                    </Link>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Mobile menu button */}
                    <div className="md:hidden flex items-center">
                        <button
                            onClick={() => setIsOpen(!isOpen)}
                            className="text-white hover:text-blue-200 focus:outline-none"
                        >
                            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                {isOpen ? (
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                ) : (
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                                )}
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Mobile Menu */}
                {isOpen && (
                    <div className="md:hidden">
                        <div className="px-2 pt-2 pb-3 space-y-1 sm:px-3">
                            <Link href="/" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                Home
                            </Link>
                            {isAuthenticated && (
                                <>
                                    <Link href="/todos" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                        All Todos
                                    </Link>
                                    <Link href="/todos/add" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                        Add Todo
                                    </Link>
                                    <Link href="/completed" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                        Completed
                                    </Link>
                                </>
                            )}

                            {/* Mobile Authentication Section */}
                            <div className="border-t border-blue-500 pt-2 mt-2">
                                {isPending ? (
                                    <div className="px-3 py-2 text-blue-200">Loading...</div>
                                ) : isAuthenticated ? (
                                    <div className="space-y-2">
                                        <div className="px-3 py-2 text-blue-200">
                                            Welcome, {user?.name || user?.email}
                                        </div>
                                        <Link
                                            href="/signout"
                                            className="block w-full text-left px-3 py-2 hover:bg-blue-700 rounded-md"
                                            onClick={() => setIsOpen(false)}
                                        >
                                            Logout
                                        </Link>
                                    </div>
                                ) : (
                                    <div className="space-y-2">
                                        <Link
                                            href="/login"
                                            className="block w-full text-left px-3 py-2 hover:bg-blue-700 rounded-md"
                                            onClick={() => setIsOpen(false)}
                                        >
                                            Login
                                        </Link>
                                        <Link
                                            href="/signup"
                                            className="block w-full text-left px-3 py-2 hover:bg-green-700 rounded-md bg-green-600"
                                            onClick={() => setIsOpen(false)}
                                        >
                                            Sign Up
                                        </Link>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </nav>
    );
}
