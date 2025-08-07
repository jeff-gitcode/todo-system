import { fn } from 'storybook/test';

import * as actual from './useSignUp';
export * from './useSignUp';
// Mock for useUserData hook
export const useSignUp = fn(actual.useSignUp).mockName('useSignUp');