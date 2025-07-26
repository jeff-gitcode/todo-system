'use client';

import { useEffect, useState } from 'react';
import { useTodos } from '@hooks/useTodos';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useParams, useSearchParams, useRouter } from 'next/navigation';

export default function TodoDetailPage() {
    const { createTodo, updateTodo, todos, loading } = useTodos();
    const params = useParams();
    const searchParams = useSearchParams();
    const router = useRouter();
    const id = typeof params?.id === 'string' ? params.id : Array.isArray(params?.id) ? params.id[0] : '';
    const isNew = id === 'new';

    const [title, setTitle] = useState('');
    const [editTitle, setEditTitle] = useState('');
    const [todo, setTodo] = useState<{ id: string; title: string } | null>(null);

    // For edit mode
    const [isEdit, setIsEdit] = useState(isNew || searchParams.get('edit') === '1');

    // Load todo if not new
    useEffect(() => {
        if (!isNew && !loading) {
            const found = todos.find(t => t.id === id);
            setTodo(found ?? null);
            setEditTitle(found?.title ?? '');
        }
    }, [id, isNew, loading, todos]);

    // Add TODO
    const handleAdd = () => {
        if (title.trim()) {
            createTodo.mutate(title, {
                onSuccess: (newTodo) => {
                    setTitle('');
                    router.push(`/todos/${newTodo.id}`);
                }
            });
        }
    };

    // Update TODO
    const handleUpdate = () => {
        if (todo && editTitle.trim()) {
            updateTodo.mutate({ id: todo.id, title: editTitle }, {
                onSuccess: () => {
                    setIsEdit(false);
                    router.push(`/todos/${todo.id}`);
                }
            });
        }
    };

    if (loading) {
        return (
            <Card className="p-6 text-center">
                Loading...
            </Card>
        );
    }

    if (isNew) {
        return (
            <Card className="p-6 max-w-md mx-auto mt-10">
                <h1 className="text-2xl font-bold mb-4">Add TODO</h1>
                <input
                    type="text"
                    className="border rounded px-2 py-1 w-full mb-4"
                    placeholder="Enter TODO title"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                />
                <Button className="w-full mb-2" onClick={handleAdd}>
                    Add
                </Button>
                <Button
                    variant="outline"
                    onClick={() => router.push('/todos')}
                >
                    Back to List
                </Button>
            </Card>
        );
    }

    if (!todo) {
        return (
            <Card className="p-6 text-center">
                TODO not found.
            </Card>
        );
    }

    return (
        <Card className="p-6 max-w-md mx-auto mt-10">
            <h1 className="text-2xl font-bold mb-4">TODO Detail</h1>
            <div className="mb-2"><strong>ID:</strong> {todo.id}</div>
            {isEdit ? (
                <div className="mb-4 flex gap-2">
                    <input
                        type="text"
                        className="border rounded px-2 py-1 flex-1"
                        value={editTitle}
                        onChange={e => setEditTitle(e.target.value)}
                    />
                    <Button
                        size="sm"
                        onClick={handleUpdate}
                    >
                        Save
                    </Button>
                    <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setIsEdit(false)}
                    >
                        Cancel
                    </Button>
                </div>
            ) : (
                <div className="mb-4"><strong>Title:</strong> {todo.title}</div>
            )}
            <Button
                variant="outline"
                onClick={() => router.push('/todos')}
            >
                Back to List
            </Button>
            {!isEdit && (
                <Button
                    className="ml-2"
                    variant="outline"
                    onClick={() => setIsEdit(true)}
                >
                    Edit
                </Button>
            )}
        </Card>
    );
}