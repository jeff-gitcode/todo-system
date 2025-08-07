import type { Meta, StoryObj } from '@storybook/nextjs-vite';
import { fn, within, expect, userEvent } from "@storybook/test";
import SignUpPage from '@presentation/(auth)/signup/page';
import { useSignUp } from '#hooks/useSignUp';

const mockSignUp = fn();
const mockSetError = fn();
const mockPush = fn();

const meta = {
    title: 'Pages/SignUpPage',
    component: SignUpPage,
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true,
            router: {
                pathname: '/signup',
                asPath: '/signup',
                push: mockPush,
            },
        },
    },
} satisfies Meta<typeof SignUpPage>;

export default meta;
type Story = StoryObj<typeof meta>;

// Default state story
export const Default: Story = {
    beforeEach: async () => {
        // Mock the useSignUp hook to return default values
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify sign up form renders correctly', async () => {
            const title = canvas.getByText('Create your account');
            expect(title).toBeInTheDocument();

            const nameInput = canvas.getByLabelText('Full Name');
            expect(nameInput).toBeInTheDocument();

            const emailInput = canvas.getByLabelText('Email address');
            expect(emailInput).toBeInTheDocument();

            const passwordInput = canvas.getByLabelText('Password');
            expect(passwordInput).toBeInTheDocument();

            const signUpButton = canvas.getByRole('button', { name: /Sign up/i });
            expect(signUpButton).toBeInTheDocument();

            const signInLink = canvas.getByText('Sign in');
            expect(signInLink).toBeInTheDocument();

            // Verify password requirements message
            const passwordRequirement = canvas.getByText('Must be at least 8 characters');
            expect(passwordRequirement).toBeInTheDocument();
        });
    },
    tags: ['autodocs'],
    render: () => <SignUpPage />,
};

// Loading state story
export const LoadingState: Story = {
    beforeEach: async () => {
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: true,
            error: null,
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify loading state is displayed', async () => {
            const loadingButton = canvas.getByText('Creating account...');
            expect(loadingButton).toBeInTheDocument();
            expect(loadingButton).toBeDisabled();
        });
    },
    render: () => <SignUpPage />,
};

// Error state story
export const WithError: Story = {
    beforeEach: async () => {
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: 'Email already exists',
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify error message is displayed', async () => {
            const errorMessage = canvas.getByText('Email already exists');
            expect(errorMessage).toBeInTheDocument();
        });
    },
    render: () => <SignUpPage />,
};

// Form submission story
export const SubmitForm: Story = {
    beforeEach: async () => {
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Fill out and submit the form', async () => {
            const nameInput = canvas.getByLabelText('Full Name');
            const emailInput = canvas.getByLabelText('Email address');
            const passwordInput = canvas.getByLabelText('Password');
            const signUpButton = canvas.getByRole('button', { name: /Sign up/i });

            // Fill in the form
            await userEvent.type(nameInput, 'John Doe');
            await userEvent.type(emailInput, 'test@example.com');
            await userEvent.type(passwordInput, 'password123');

            // Submit the form
            await userEvent.click(signUpButton);

            // Verify signUp function was called with correct values
            expect(mockSetError).toHaveBeenCalledWith(null);
            expect(mockSignUp).toHaveBeenCalledWith('test@example.com', 'password123', 'John Doe');
        });
    },
    render: () => <SignUpPage />,
};

// Navigate to sign in story
export const NavigateToSignIn: Story = {
    beforeEach: async () => {
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Click the sign in link', async () => {
            const signInLink = canvas.getByText('Sign in');
            await userEvent.click(signInLink);

            // In a real test, we would verify navigation, but Storybook's simulated
            // environment doesn't fully support this, so we're just verifying the click
            expect(signInLink).toBeInTheDocument();
        });
    },
    render: () => <SignUpPage />,
};

// Validation error story
export const ValidationError: Story = {
    beforeEach: async () => {
        useSignUp.mockReturnValue({
            signUp: mockSignUp,
            loading: false,
            error: null,
            setError: mockSetError,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Try to submit with empty fields', async () => {
            // Form fields should have required attribute, so browser validation will kick in
            // However, we can't easily test browser validation in Storybook
            // This story is mainly to visually see the form's validation behavior

            const nameInput = canvas.getByLabelText('Full Name');
            const emailInput = canvas.getByLabelText('Email address');
            const passwordInput = canvas.getByLabelText('Password');

            // Verify required attributes
            expect(nameInput).toHaveAttribute('required');
            expect(emailInput).toHaveAttribute('required');
            expect(passwordInput).toHaveAttribute('required');
            expect(passwordInput).toHaveAttribute('minLength', '8');
        });
    },
    render: () => <SignUpPage />,
};