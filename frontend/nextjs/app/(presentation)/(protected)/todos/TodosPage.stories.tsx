import type { Meta, StoryObj } from '@storybook/nextjs-vite';
import TodosPage from '@presentation/(protected)/todos/page';
import { fn, within, expect, userEvent } from "@storybook/test";

import { useTodos } from '#hooks/useTodos';

const mockPush = fn();
const mockRefresh = fn();
const mockDeleteMutate = fn();

const meta = {
    title: 'Pages/TodosPage',
    component: TodosPage,
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true,
            router: {
                pathname: '/todos',
                asPath: '/todos',
                push: mockPush,
                replace: fn(),
                refresh: mockRefresh,
            },
        },
    },
} satisfies Meta<typeof TodosPage>;

export default meta;

type Story = StoryObj<typeof meta>;

// Base story that reflects the default state
export const Default: Story = {
    beforeEach: async () => {
        // Reset mocks before each story run
        mockPush.mockReset();
        mockRefresh.mockReset();
        mockDeleteMutate.mockReset();

        // Mock the useTodos hook to return predefined todos
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify page renders the todo list', async () => {
            const todoList = canvas.getByText('TODO List');
            expect(todoList).toBeInTheDocument();

            const firstTodo = canvas.getByText(/First Todo/);
            expect(firstTodo).toBeInTheDocument();

            const secondTodo = canvas.getByText(/Second Todo/);
            expect(secondTodo).toBeInTheDocument();
        });
    },
    tags: ['autodocs'],
    render: () => <TodosPage />,
};

// Story that demonstrates loading state
export const LoadingState: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [],
            loading: true,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify loading state is displayed', async () => {
            const loadingText = canvas.getByText('Loading...');
            expect(loadingText).toBeInTheDocument();
        });
    },
    render: () => <TodosPage />,
};

// Story that demonstrates navigation to todo detail
export const NavigateToTodoDetail: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Navigate to todo detail when clicking on todo', async () => {
            const firstTodo = canvas.getByText(/First Todo/);
            await userEvent.click(firstTodo);

            expect(mockPush).toHaveBeenCalledWith('/todos/1');
        });
    },
    render: () => <TodosPage />,
};

// Story that demonstrates navigation to edit page
export const NavigateToEditPage: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Navigate to edit page when clicking on Edit button', async () => {
            const editButtons = canvas.getAllByText('Edit');
            await userEvent.click(editButtons[0]);

            expect(mockPush).toHaveBeenCalledWith('/todos/1?edit=1');
        });
    },
    render: () => <TodosPage />,
};

// Story that demonstrates deleting a todo
export const DeleteTodo: Story = {
    beforeEach: async () => {
        mockDeleteMutate.mockImplementation((id, options) => {
            if (options && options.onSuccess) {
                options.onSuccess();
            }
        });

        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Delete todo when clicking on Delete button', async () => {
            const deleteButtons = canvas.getAllByText('Delete');
            await userEvent.click(deleteButtons[0]);

            expect(mockDeleteMutate).toHaveBeenCalled();
            expect(mockRefresh).toHaveBeenCalled();
        });
    },
    render: () => <TodosPage />,
};

// Story that demonstrates navigation to add todo page
export const NavigateToAddPage: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: null,
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Navigate to add page when clicking on Add TODO button', async () => {
            const addButton = canvas.getByText('Add TODO');
            await userEvent.click(addButton);

            expect(mockPush).toHaveBeenCalledWith('/todos/new?edit=1');
        });
    },
    render: () => <TodosPage />,
};

// Story that demonstrates error state
export const ErrorState: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [],
            loading: false,
            deleteTodo: { mutate: mockDeleteMutate },
            fetchError: new Error("Failed to fetch todos"),
            createTodo: { mutate: fn() },
            updateTodo: { mutate: fn() },
        });
    },
    parameters: {
        // Disable throwing errors in Storybook so we can see the UI
        // In the real app, the error would be caught by Next.js error boundary
        chromatic: { disableSnapshot: true },
    },
    render: () => <TodosPage />,
};