import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { todoService } from '@application/todoService';
import { Todo } from '@domain/models';

export function useTodos() {
    const queryClient = useQueryClient();

    // 查询所有 TODO
    const { data: todos = [], isLoading: loading, error: fetchError } = useQuery<Todo[]>({
        queryKey: ['todos'],
        queryFn: () => todoService.getAll(),
    });

    // 新建 TODO
    const createTodo = useMutation({
        mutationFn: (title: string) => todoService.create(title),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['todos'] }),
        onError: (error: unknown) => {
            // Optionally log or handle create error here
            console.error('Create todo error:', error);
        },
    });

    // 更新 TODO
    const updateTodo = useMutation({
        mutationFn: ({ id, title }: { id: string; title: string }) => todoService.update(id, title),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['todos'] }),
        onError: (error: unknown) => {
            // Optionally log or handle update error here
            console.error('Update todo error:', error);
        },
    });

    // 删除 TODO
    const deleteTodo = useMutation({
        mutationFn: (id: string) => todoService.delete(id),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['todos'] }),
        onError: (error: unknown) => {
            // Optionally log or handle delete error here
            console.error('Delete todo error:', error);
        },
    });

    return {
        todos,
        loading,
        fetchError,
        createTodo,
        updateTodo,
        deleteTodo,
    };
}