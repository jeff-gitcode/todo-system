

import type { Meta, StoryObj } from '@storybook/nextjs-vite';
import { fn, within, expect, userEvent } from "@storybook/test";
import { useTodos } from '#hooks/useTodos';
import TodoDetailPage from '@presentation/(protected)/dashboard/todos/[id]/page';

const defaultRouter = {
    // The locale should be configured globally: https://storybook.js.org/docs/essentials/toolbars-and-globals#globals
    locale: 'en', // or your desired default locale
    asPath: '/',
    basePath: '/',
    isFallback: false,
    isLocaleDomain: false,
    isReady: true,
    isPreview: false,
    route: '/',
    pathname: '/',
    query: {},
};

const mockPush = fn();
const mockCreateMutate = fn();
const mockUpdateMutate = fn();


const meta = {
    title: 'Pages/TodoDetailPage',
    component: TodoDetailPage,
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true,
            router: defaultRouter,
        },
    },
} satisfies Meta<typeof TodoDetailPage>;

export default meta;

type Story = StoryObj<typeof meta>;

// Base story for viewing a todo detail
export const ViewingTodo: Story = {
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true,
            navigation: {
                pathname: '/dashboard/todos/[id]',
                query: { id: '1' },
                segments: [
                    ['id', '1'],
                ]
            }
        },
    },
    beforeEach: async () => {
        // Reset mocks
        mockPush.mockReset();
        mockCreateMutate.mockReset();
        mockUpdateMutate.mockReset();

        // Mock useTodos hook
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify todo details are displayed', async () => {
            const detailTitle = canvas.getByText('TODO Detail');
            expect(detailTitle).toBeInTheDocument();

            const todoTitle = canvas.getByText(/First Todo/);
            expect(todoTitle).toBeInTheDocument();

            const editButton = canvas.getByText('Edit');
            expect(editButton).toBeInTheDocument();
        });
    },
    tags: ['autodocs'],
    render: () => <TodoDetailPage />,
};

// Story for loading state
export const Loading: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [],
            loading: true,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify loading state is displayed', async () => {
            const loadingText = canvas.getByText('Loading...');
            expect(loadingText).toBeInTheDocument();
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for adding a new todo
export const AddingNewTodo: Story = {
    parameters: {
        nextjs: {
            router: {
                pathname: '/dashboard/todos/new',
                asPath: '/dashboard/todos/new',
                push: mockPush,
            },
            navigation: {
                pathname: '/dashboard/todos/[id]',
                query: { id: 'new' },
                segments: [
                    ['id', 'new'],
                ]
            },
            searchParams: new URLSearchParams(),
        },
    },
    beforeEach: async () => {
        mockCreateMutate.mockImplementation((title, { onSuccess }) => onSuccess({ id: '3', title }));

        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Fill in the new todo form', async () => {
            const formTitle = canvas.getByText('Add TODO');
            expect(formTitle).toBeInTheDocument();

            const input = canvas.getByPlaceholderText('Enter TODO title');
            await userEvent.type(input, 'New Task');

            const addButton = canvas.getByText('Add');
            await userEvent.click(addButton);

            expect(mockCreateMutate).toHaveBeenCalledWith('New Task', expect.objectContaining({
                onSuccess: expect.any(Function)
            }));
            expect(mockPush).toHaveBeenCalledWith('/dashboard/todos');
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for todo not found
export const TodoNotFound: Story = {
    parameters: {
        nextjs: {
            router: {
                pathname: '/dashboard/todos/999',
                asPath: '/dashboard/todos/999',
                push: mockPush,
            },
        },
    },
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify not found message is displayed', async () => {
            const notFoundMessage = canvas.getByText('TODO not found.');
            expect(notFoundMessage).toBeInTheDocument();
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for editing a todo
export const EditingTodo: Story = {
    parameters: {
        nextjs: {
            router: {
                pathname: '/dashboard/todos/1',
                asPath: '/dashboard/todos/1?edit=1',
                push: mockPush,
            },
            navigation: {
                pathname: '/dashboard/todos/[id]',
                query: { id: '1' },
                segments: [
                    ['id', '1'],
                ]
            },
            searchParams: new URLSearchParams('edit=1'),
        },
    },
    beforeEach: async () => {
        mockUpdateMutate.mockImplementation((data, { onSuccess }) => onSuccess());

        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Edit and save the todo', async () => {
            const input = canvas.getByDisplayValue('First Todo');
            await userEvent.clear(input);
            await userEvent.type(input, 'Updated Title');

            const saveButton = canvas.getByText('Save');
            await userEvent.click(saveButton);

            expect(mockUpdateMutate).toHaveBeenCalledWith(
                { id: '1', title: 'Updated Title' },
                expect.objectContaining({ onSuccess: expect.any(Function) })
            );
            expect(mockPush).toHaveBeenCalledWith('/dashboard/todos');
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for canceling edit
export const CancelingEdit: Story = {
    parameters: {
        nextjs: {
            router: {
                pathname: '/dashboard/todos/1',
                asPath: '/dashboard/todos/1?edit=1',
                push: mockPush,
            },
            searchParams: new URLSearchParams('edit=1'),
        },
    },
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Cancel the todo edit', async () => {
            const cancelButton = canvas.getByText('Cancel');
            await userEvent.click(cancelButton);

            // After canceling, the Edit button should appear again
            const editButton = canvas.getByText('Edit');
            expect(editButton).toBeInTheDocument();
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for navigating back to list
export const NavigatingBack: Story = {
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Navigate back to todo list', async () => {
            const backButton = canvas.getByText('Back to List');
            await userEvent.click(backButton);

            expect(mockPush).toHaveBeenCalledWith('/dashboard/todos');
        });
    },
    render: () => <TodoDetailPage />,
};

// Story for empty form validation
export const EmptyFormValidation: Story = {
    parameters: {
        nextjs: {
            router: {
                pathname: '/dashboard/todos/1',
                asPath: '/dashboard/todos/1?edit=1',
                push: mockPush,
            },
            searchParams: new URLSearchParams('edit=1'),
        },
    },
    beforeEach: async () => {
        useTodos.mockReturnValue({
            todos: [
                { id: '1', title: 'First Todo' },
                { id: '2', title: 'Second Todo' },
            ],
            loading: false,
            createTodo: { mutate: mockCreateMutate },
            updateTodo: { mutate: mockUpdateMutate },
            fetchError: null,
            deleteTodo: { mutate: fn() },
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Try to save with empty title', async () => {
            const input = canvas.getByDisplayValue('First Todo');
            await userEvent.clear(input);

            const saveButton = canvas.getByText('Save');
            await userEvent.click(saveButton);

            // Verify the update mutation was not called
            expect(mockUpdateMutate).not.toHaveBeenCalled();
        });
    },
    render: () => <TodoDetailPage />,
};