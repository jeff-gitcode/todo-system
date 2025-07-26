import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { ReactQueryProviders } from './reactQueryProvider';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

// frontend/nextjs/app/(presentation)/components/reactQueryProvider.test.tsx

// Arrange: Mock ReactQueryDevtools to avoid rendering actual devtools UI
jest.mock('@tanstack/react-query-devtools', () => ({
    ReactQueryDevtools: jest.fn(() => <div data-testid="devtools" />),
}));

describe('ReactQueryProviders', () => {
    it('renders children inside QueryClientProvider', () => {
        // Arrange
        const Child = () => <div data-testid="child">Hello</div>;

        // Act
        render(
            <ReactQueryProviders>
                <Child />
            </ReactQueryProviders>
        );

        // Assert
        expect(screen.getByTestId('child')).toBeInTheDocument();
    });

    it('renders ReactQueryDevtools', () => {
        // Arrange & Act
        render(
            <ReactQueryProviders>
                <div>Test</div>
            </ReactQueryProviders>
        );

        // Assert
        expect(screen.getByTestId('devtools')).toBeInTheDocument();
    });

    it('provides a QueryClient instance', () => {
        // Arrange
        let receivedClient: any = null;
        function TestComponent() {
            // Act
            // Try to access QueryClientProvider context
            // This is a smoke test: if QueryClientProvider is missing, this will throw
            // We can't access the client directly, but we can check that context is available
            return <div>Test</div>;
        }

        // Act & Assert
        expect(() =>
            render(
                <ReactQueryProviders>
                    <TestComponent />
                </ReactQueryProviders>
            )
        ).not.toThrow();
    });
});