'use client';

import { useRouter } from 'next/navigation';
import { useTodos } from '@hooks/useTodos';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

export default function TodosPage() {
    const { todos, loading, deleteTodo, fetchError } = useTodos();
    const router = useRouter();

    const handleDelete = (id: string) => {
        deleteTodo.mutate(id, {
            onSuccess: () => {
                // Refresh the page to update the todos list
                router.refresh();
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
                            onClick={() => router.push(`/todos/${todo.id}`)}
                        >
                            {todo.id}  {todo.title}
                        </span>
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => router.push(`/todos/${todo.id}?edit=1`)}
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
                ))}
            </ul>
            <Button className="w-full" onClick={() => router.push(`/todos/new?edit=1`)}>Add TODO</Button>
        </Card>
    );
}