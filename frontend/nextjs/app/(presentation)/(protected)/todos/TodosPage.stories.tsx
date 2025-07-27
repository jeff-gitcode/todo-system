import type { Meta, StoryObj } from '@storybook/nextjs-vite'; // Updated import for nextjs-vite
import TodosPage from '@presentation/(protected)/todos/page';
import { fn, within } from "@storybook/test";

import { useTodos } from '#hooks/useTodos';

const meta = {
    title: 'Pages/TodosPage',
    component: TodosPage,
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true, // 👈 This is crucial for next/navigation
            router: {
                pathname: '/dashboard',
                asPath: '/dashboard',
                query: { user: 'Laptop' },
            },
        },
    },
} satisfies Meta<typeof TodosPage>;;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {
    beforeEach: async () => {
        // Mock the useTodos hook to return predefined todos
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: fn() },
            fetchError: null,
        });
    },
    play: async ({ args, canvasElement }) => {
        const canvas = within(canvasElement);
    },
    tags: ['autodocs'],
    render: () => {
        return (
            <TodosPage />
        );
    },
};