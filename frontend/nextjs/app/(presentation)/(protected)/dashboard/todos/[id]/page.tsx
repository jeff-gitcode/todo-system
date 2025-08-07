'use client';

import { useEffect, useState } from 'react';
import { useTodos } from '#hooks/useTodos';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useParams, useSearchParams, useRouter } from 'next/navigation';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle } from 'lucide-react';
import { toast } from 'sonner';

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
    const [titleError, setTitleError] = useState('');
    const [isEdit, setIsEdit] = useState(isNew || searchParams.get('edit') === '1');

    // Load todo if not new
    useEffect(() => {
        if (!isNew && !loading) {
            const found = todos.find(t => t.id === id);
            setTodo(found ?? null);
            setEditTitle(found?.title ?? '');
        }
    }, [id, isNew, loading, todos]);

    // Validate title
    const validateTitle = (value: string): boolean => {
        if (!value.trim()) {
            setTitleError('Title is required');
            return false;
        }
        setTitleError('');
        return true;
    };

    // Add TODO
    const handleAdd = () => {
        if (validateTitle(title)) {
            // Show loading toast
            toast.loading("Creating todo...", {
                description: "Please wait while we add your new todo",
                id: "create-todo"
            });
            
            createTodo.mutate(title, {
                onSuccess: () => {
                    // Dismiss loading toast
                    toast.dismiss("create-todo");
                    
                    // Show success toast
                    toast.success("Todo created", {
                        description: "Your todo has been successfully created"
                    });
                    
                    setTitle('');
                    router.push(`/dashboard/todos`);
                },
                onError: (error) => {
                    // Dismiss loading toast
                    toast.dismiss("create-todo");
                    
                    // Show error toast
                    toast.error("Failed to create todo", {
                        description: error.message || "An unexpected error occurred"
                    });
                }
            });
        } else {
            // Show validation error toast
            toast.error("Validation error", {
                description: "Please enter a title for your todo"
            });
        }
    };

    // Update TODO
    const handleUpdate = () => {
        if (todo && validateTitle(editTitle)) {
            // Show loading toast
            toast.loading("Updating todo...", {
                description: "Please wait while we update your todo",
                id: `update-${todo.id}`
            });
            
            updateTodo.mutate({ id: todo.id, title: editTitle }, {
                onSuccess: () => {
                    // Dismiss loading toast
                    toast.dismiss(`update-${todo.id}`);
                    
                    // Show success toast
                    toast.success("Todo updated", {
                        description: "Your todo has been successfully updated"
                    });
                    
                    setIsEdit(false);
                    router.push(`/dashboard/todos`);
                },
                onError: (error) => {
                    // Dismiss loading toast
                    toast.dismiss(`update-${todo.id}`);
                    
                    // Show error toast
                    toast.error("Failed to update todo", {
                        description: error.message || "An unexpected error occurred"
                    });
                }
            });
        } else {
            // Show validation error toast if there's no todo or invalid title
            toast.error("Validation error", {
                description: "Please enter a valid title for your todo"
            });
        }
    };

    // Handle navigation with toast feedback
    const handleBackToList = () => {
        toast.info("Returning to list", {
            description: "Navigating back to your todo list"
        });
        router.push('/dashboard/todos');
    };

    // Handle edit mode switch
    const handleEditModeToggle = () => {
        if (!isEdit) {
            toast.info("Editing todo", {
                description: "You can now edit this todo"
            });
            setIsEdit(true);
        } else {
            setIsEdit(false);
            setTitleError('');
            setEditTitle(todo?.title || '');
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
                <div className="mb-4">
                    <label htmlFor="title" className="block text-sm font-medium mb-1">
                        Title <span className="text-red-500">*</span>
                    </label>
                    <input
                        id="title"
                        type="text"
                        className={`border rounded px-2 py-1 w-full ${titleError ? 'border-red-500' : ''}`}
                        placeholder="Enter TODO title"
                        value={title}
                        onChange={e => {
                            setTitle(e.target.value);
                            if (titleError) validateTitle(e.target.value);
                        }}
                        required
                        aria-required="true"
                    />
                    {titleError && (
                        <Alert variant="destructive" className="mt-2 py-2">
                            <AlertCircle className="h-4 w-4" />
                            <AlertDescription>{titleError}</AlertDescription>
                        </Alert>
                    )}
                </div>
                <Button
                    className="w-full mb-2"
                    onClick={handleAdd}
                    disabled={createTodo.isPending}
                >
                    {createTodo.isPending ? 'Adding...' : 'Add'}
                </Button>
                <Button
                    variant="outline"
                    onClick={handleBackToList}
                >
                    Back to List
                </Button>
            </Card>
        );
    }

    if (!todo) {
        return (
            <Card className="p-6 text-center">
                <div className="mb-4">TODO not found.</div>
                <Button 
                    variant="outline" 
                    onClick={handleBackToList}
                >
                    Back to List
                </Button>
            </Card>
        );
    }

    return (
        <Card className="p-6 max-w-md mx-auto mt-10">
            <h1 className="text-2xl font-bold mb-4">TODO Detail</h1>
            <div className="mb-2"><strong>ID:</strong> {todo.id}</div>
            {isEdit ? (
                <div className="mb-4">
                    <label htmlFor="editTitle" className="block text-sm font-medium mb-1">
                        Title <span className="text-red-500">*</span>
                    </label>
                    <div className="flex gap-2">
                        <input
                            id="editTitle"
                            type="text"
                            className={`border rounded px-2 py-1 flex-1 ${titleError ? 'border-red-500' : ''}`}
                            value={editTitle}
                            onChange={e => {
                                setEditTitle(e.target.value);
                                if (titleError) validateTitle(e.target.value);
                            }}
                            required
                            aria-required="true"
                        />
                        <Button
                            size="sm"
                            onClick={handleUpdate}
                            disabled={updateTodo.isPending}
                        >
                            {updateTodo.isPending ? 'Saving...' : 'Save'}
                        </Button>
                        <Button
                            size="sm"
                            variant="outline"
                            onClick={() => {
                                setIsEdit(false);
                                setTitleError('');
                                setEditTitle(todo.title);
                                toast.info("Edit cancelled", {
                                    description: "Your changes have been discarded"
                                });
                            }}
                        >
                            Cancel
                        </Button>
                    </div>
                    {titleError && (
                        <Alert variant="destructive" className="mt-2 py-2">
                            <AlertCircle className="h-4 w-4" />
                            <AlertDescription>{titleError}</AlertDescription>
                        </Alert>
                    )}
                </div>
            ) : (
                <div className="mb-4"><strong>Title:</strong> {todo.title}</div>
            )}
            <Button
                variant="outline"
                onClick={handleBackToList}
            >
                Back to List
            </Button>
            {!isEdit && (
                <Button
                    className="ml-2"
                    variant="outline"
                    onClick={handleEditModeToggle}
                >
                    Edit
                </Button>
            )}
        </Card>
    );
}