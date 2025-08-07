'use client';

import { useRouter } from 'next/navigation';
import { useTodos } from '#hooks/useTodos';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
export default function TodosPage() {
    const { todos, loading, deleteTodo, fetchError } = useTodos();
    console.log('TodosPage rendered with todos:', todos);
    console.log('Loading state:', loading);
    const router = useRouter();

    const handleDelete = (id: string) => {
        // Show loading toast when delete operation starts
        toast.loading("Deleting todo...", {
            description: "Please wait while we delete this todo",
            id: `delete-${id}`,
        });

        deleteTodo.mutate(id, {
            onSuccess: () => {
                // Dismiss the loading toast
                toast.dismiss(`delete-${id}`);
                
                // Show success toast
                toast.success("Todo deleted", {
                    description: "The todo has been successfully deleted",
                });
                
                // Refresh the page to update the todos list
                router.refresh();
            },
            onError: (error) => {
                // Dismiss the loading toast
                toast.dismiss(`delete-${id}`);
                
                // Show error toast
                toast.error("Failed to delete todo", {
                    description: error.message || "An unexpected error occurred",
                });
            }
        });
    };

    if (loading) return (
        <Card className="p-6 text-center">
            Loading...
        </Card>
    );

    // Handle any fetch error
    if (fetchError) {
        throw fetchError
    }

    return (
        <Card className="p-6 max-w-xl mx-auto">
            <h1 className="text-2xl font-bold mb-4">TODO List</h1>
            <ul className="space-y-2 mb-6">
                {todos.map(todo => (
                    <li key={todo.id} className="flex items-center justify-between border-b pb-2">
                        <span
                            className="cursor-pointer hover:underline"
                            onClick={() => router.push(`/dashboard/todos/${todo.id}`)}
                        >
                            {todo.id}  {todo.title}
                        </span>
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => router.push(`/dashboard/todos/${todo.id}?edit=1`)}
                            >
                                Edit
                            </Button>
                            <Button
                                variant="destructive"
                                size="sm"
                                onClick={() => handleDelete(todo.id)}
                            >
                                Delete
                            </Button>
                        </div>
                    </li>
                ))
                }
            </ul >
            <Button className="w-full" onClick={() => router.push(`/dashboard/todos/new?edit=1`)}>Add TODO</Button>
        </Card >
    );
}