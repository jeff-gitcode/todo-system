import { fn } from 'storybook/test';

import * as actual from './useSignIn';
export * from './useSignIn';
// Mock for useUserData hook
export const useSignIn = fn(actual.useSignIn).mockName('useSignIn');