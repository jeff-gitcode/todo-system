import type { Meta, StoryObj } from '@storybook/nextjs-vite';
import { fn, within, expect, userEvent } from "@storybook/test";
import SignOutPage from '@presentation/(auth)/signout/page';
import { useSignOut } from '#hooks/useSignOut';

const mockSignOut = fn();
const mockBack = fn();

const meta = {
    title: 'Pages/SignOutPage',
    component: SignOutPage,
    parameters: {
        layout: 'fullscreen',
        nextjs: {
            appDirectory: true,
            router: {
                pathname: '/signout',
                asPath: '/signout',
                push: fn(),
                back: mockBack,
            },
        },
    },
} satisfies Meta<typeof SignOutPage>;

export default meta;
type Story = StoryObj<typeof meta>;

// Default state - showing the confirmation dialog
export const Default: Story = {
    beforeEach: async () => {
        // Reset mocks
        mockSignOut.mockReset();
        mockBack.mockReset();

        // Mock the useSignOut hook
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: false,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify sign out confirmation page renders correctly', async () => {
            const title = canvas.getByText('Sign out');
            expect(title).toBeInTheDocument();

            const confirmationText = canvas.getByText('Are you sure you want to sign out of your account?');
            expect(confirmationText).toBeInTheDocument();

            const signOutButton = canvas.getByRole('button', { name: /^Sign out$/i });
            expect(signOutButton).toBeInTheDocument();

            const cancelButton = canvas.getByRole('button', { name: /Cancel/i });
            expect(cancelButton).toBeInTheDocument();
        });
    },
    tags: ['autodocs'],
    render: () => <SignOutPage />,
};

// Loading state - when the user has clicked sign out
export const LoadingState: Story = {
    beforeEach: async () => {
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: true,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify loading state is displayed', async () => {
            const loadingButton = canvas.getByText('Signing out...');
            expect(loadingButton).toBeInTheDocument();
            expect(loadingButton).toBeDisabled();

            const cancelButton = canvas.getByRole('button', { name: /Cancel/i });
            expect(cancelButton).toBeDisabled();
        });
    },
    render: () => <SignOutPage />,
};

// Sign out flow - clicking the sign out button
export const SignOutFlow: Story = {
    beforeEach: async () => {
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: false,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Click the sign out button', async () => {
            const signOutButton = canvas.getByRole('button', { name: /^Sign out$/i });
            await userEvent.click(signOutButton);

            expect(mockSignOut).toHaveBeenCalled();
        });
    },
    render: () => <SignOutPage />,
};

// Cancel flow - clicking the cancel button
export const CancelFlow: Story = {
    beforeEach: async () => {
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: false,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Click the cancel button', async () => {
            const cancelButton = canvas.getByRole('button', { name: /Cancel/i });
            await userEvent.click(cancelButton);

            expect(mockBack).toHaveBeenCalled();
        });
    },
    render: () => <SignOutPage />,
};

// Visual verification of button styling
export const ButtonStyling: Story = {
    beforeEach: async () => {
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: false,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify button styling', async () => {
            const signOutButton = canvas.getByRole('button', { name: /^Sign out$/i });
            const cancelButton = canvas.getByRole('button', { name: /Cancel/i });

            expect(signOutButton).toHaveClass('w-full');
            expect(cancelButton).toHaveClass('w-full');

            // Verify the destructive variant is applied to the sign out button
            expect(signOutButton).toHaveClass('destructive');
        });
    },
    render: () => <SignOutPage />,
};

// Disabled UI state - when loading
export const DisabledState: Story = {
    beforeEach: async () => {
        useSignOut.mockReturnValue({
            signOut: mockSignOut,
            loading: true,
        });
    },
    play: async ({ canvasElement, step }) => {
        const canvas = within(canvasElement);

        await step('Verify buttons are disabled during loading', async () => {
            const signOutButton = canvas.getByRole('button', { name: /Signing out.../i });
            const cancelButton = canvas.getByRole('button', { name: /Cancel/i });

            expect(signOutButton).toBeDisabled();
            expect(cancelButton).toBeDisabled();

            // Try clicking the disabled buttons (should do nothing)
            await userEvent.click(signOutButton);
            await userEvent.click(cancelButton);

            expect(mockSignOut).not.toHaveBeenCalled();
            expect(mockBack).not.toHaveBeenCalled();
        });
    },
    render: () => <SignOutPage />,
};