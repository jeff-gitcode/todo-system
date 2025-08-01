import type { Preview } from '@storybook/nextjs-vite'
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from 'react';

import '../app/globals.css'; // Ensure global styles are applied

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },

    a11y: {
      // 'todo' - show a11y violations in the test UI only
      // 'error' - fail CI on a11y violations
      // 'off' - skip a11y checks entirely
      test: 'todo'
    }
  },
  decorators: [
    (Story) => {
      const [queryClient] = useState(() => new QueryClient());

      return (
        <QueryClientProvider client={queryClient}>
          <Story />
        </QueryClientProvider>
      );
    }
  ]
};

export default preview;