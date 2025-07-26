'use client';

import { useTodos } from '../../hooks/useTodos';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

export default function TodosPage() {
    const { todos, loading } = useTodos();

    if (loading) return (
        <Card className="p-6 text-center">
            加载中...
        </Card>
    );

    return (
        <Card className="p-6 max-w-xl mx-auto">
            <h1 className="text-2xl font-bold mb-4">TODO 列表</h1>
            <ul className="space-y-2 mb-6">
                {todos.map(todo => (
                    <li key={todo.id} className="flex items-center justify-between border-b pb-2">
                        <span>{todo.title}</span>
                        <Button variant="outline" size="sm">编辑</Button>
                    </li>
                ))}
            </ul>
            <Button className="w-full">新建 TODO</Button>
        </Card>
    );
}