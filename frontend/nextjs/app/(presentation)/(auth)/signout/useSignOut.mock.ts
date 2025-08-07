import { fn } from 'storybook/test';

import * as actual from './useSignOut';
export * from './useSignOut';
// Mock for useUserData hook
export const useSignOut = fn(actual.useSignOut).mockName('useSignOut');