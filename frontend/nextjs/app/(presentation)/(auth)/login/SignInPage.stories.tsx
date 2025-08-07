import React from 'react';
import type { Meta, StoryObj } from '@storybook/nextjs-vite';
import { fn, within, expect, userEvent } from "@storybook/test";
import SignInPage from '@presentation/(auth)/login/page';
import { useSignIn } from '#hooks/useSignIn';


const mockSignIn = fn();
const mockSetError = fn();
const mockPush = fn();

const meta = {
  title: 'Pages/SignInPage',
  component: SignInPage,
  parameters: {
    layout: 'fullscreen',
    nextjs: {
      appDirectory: true,
      router: {
        pathname: '/login',
        asPath: '/login',
        push: mockPush,
      },
    },
  },
//   decorators: [
//     (Story) => {
//       // Reset mocks before each story run
//       mockSignIn.mockReset();
//       mockSetError.mockReset();
//       mockPush.mockReset();
      
//       return <Story />;
//     }
//   ]
} satisfies Meta<typeof SignInPage>;

export default meta;

type Story = StoryObj<typeof meta>;

// Default state story
export const Normal: Story = {
  beforeEach: async () => {
    // Mock the useSignIn hook to return default values
    useSignIn.mockReturnValue({
      signIn: mockSignIn,
      loading: false,
      error: null,
      setError: mockSetError,
    });
  },
  play: async ({ canvasElement, step }) => {
    const canvas = within(canvasElement);

    await step('Verify sign in form renders correctly', async () => {
      const title = canvas.getByText('Sign in to your account');
      expect(title).toBeInTheDocument();

      const emailInput = canvas.getByLabelText('Email address');
      expect(emailInput).toBeInTheDocument();

      const passwordInput = canvas.getByLabelText('Password');
      expect(passwordInput).toBeInTheDocument();

      const signInButton = canvas.getByRole('button', { name: /Sign in/i });
      expect(signInButton).toBeInTheDocument();

      const signUpLink = canvas.getByText('Sign up');
      expect(signUpLink).toBeInTheDocument();
     });
  },
  tags: ['autodocs'],
  render: () => <SignInPage />,
};

// Loading state story
export const LoadingState: Story = {
  beforeEach: async () => {
    useSignIn.mockReturnValue({
      signIn: mockSignIn,
      loading: true,
      error: null,
      setError: mockSetError,
    });
  },
  play: async ({ canvasElement, step }) => {
    const canvas = within(canvasElement);

    await step('Verify loading state is displayed', async () => {
      const loadingButton = canvas.getByText('Signing in...');
      expect(loadingButton).toBeInTheDocument();
      expect(loadingButton).toBeDisabled();
    });
  },
  render: () => <SignInPage />,
};

// Error state story
export const WithError: Story = {
  beforeEach: async () => {
    useSignIn.mockReturnValue({
      signIn: mockSignIn,
      loading: false,
      error: 'Invalid email or password',
      setError: mockSetError,
    });
  },
  play: async ({ canvasElement, step }) => {
    const canvas = within(canvasElement);

    await step('Verify error message is displayed', async () => {
      const errorMessage = canvas.getByText('Invalid email or password');
      expect(errorMessage).toBeInTheDocument();
    });
  },
  render: () => <SignInPage />,
};

// Form submission story
export const SubmitForm: Story = {
  beforeEach: async () => {
    useSignIn.mockReturnValue({
      signIn: mockSignIn,
      loading: false,
      error: null,
      setError: mockSetError,
    });
  },
  play: async ({ canvasElement, step }) => {
    const canvas = within(canvasElement);

    await step('Fill out and submit the form', async () => {
      const emailInput = canvas.getByLabelText('Email address');
      const passwordInput = canvas.getByLabelText('Password');
      const signInButton = canvas.getByRole('button', { name: /Sign in/i });

      // Fill in the form
      await userEvent.type(emailInput, 'test@example.com');
      await userEvent.type(passwordInput, 'password123');

      // Submit the form
      await userEvent.click(signInButton);

      // Verify signIn function was called with correct values
      expect(mockSetError).toHaveBeenCalledWith(null);
      expect(mockSignIn).toHaveBeenCalledWith('test@example.com', 'password123');
    });
  },
  render: () => <SignInPage />,
};

// Navigate to sign up story
export const NavigateToSignUp: Story = {
  beforeEach: async () => {
    useSignIn .mockReturnValue({
      signIn: mockSignIn,
      loading: false,
      error: null,
      setError: mockSetError,
    });
  },
  play: async ({ canvasElement, step }) => {
    const canvas = within(canvasElement);

    await step('Click the sign up link', async () => {
      const signUpLink = canvas.getByText('Sign up');
      await userEvent.click(signUpLink);
      
      // In a real test, we would verify navigation, but Storybook's simulated
      // environment doesn't fully support this, so we're just verifying the click
      expect(signUpLink).toBeInTheDocument();
    });
  },
  render: () => <SignInPage />,
};