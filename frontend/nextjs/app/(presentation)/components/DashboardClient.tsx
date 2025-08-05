// app/(presentation)/(protected)/dashboard/DashboardClient.tsx
"use client";

import { useEffect } from 'react';
import { toast } from 'sonner';
import TodosPage from '@presentation/(protected)/dashboard/todos/page';

interface DashboardClientProps {
    user: {
        id: string;
        name?: string;
        email: string;
    };
    initialTodos: {
        id: string;
        title: string;
        completed: boolean;
        // Add other fields as needed
    }[];
}

export function DashboardClient({ user, initialTodos }: DashboardClientProps) {
    useEffect(() => {
        // Show welcome toast when component mounts (client-side only)
        toast.success("Welcome back!", {
            description: `Good to see you again, ${user.name || user.email}`,
            duration: 3000,
        });
    }, [user]);

    return <TodosPage />;
    // return <TodosPageClient initialTodos={initialTodos} userId={user.id} />;
}