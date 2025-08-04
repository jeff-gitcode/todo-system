'use client';

import Link from 'next/link';
import { useState } from 'react';

export default function Navigation() {
    const [isOpen, setIsOpen] = useState(false);

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
                        <Link href="/todos" className="hover:text-blue-200 transition-colors">
                            All Todos
                        </Link>
                        <Link href="/todos/add" className="hover:text-blue-200 transition-colors">
                            Add Todo
                        </Link>
                        <Link href="/completed" className="hover:text-blue-200 transition-colors">
                            Completed
                        </Link>
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
                            <Link href="/todos" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                All Todos
                            </Link>
                            <Link href="/todos/add" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                Add Todo
                            </Link>
                            <Link href="/completed" className="block px-3 py-2 hover:bg-blue-700 rounded-md">
                                Completed
                            </Link>
                        </div>
                    </div>
                )}
            </div>
        </nav>
    );
}
