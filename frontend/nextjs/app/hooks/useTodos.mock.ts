import { fn } from 'storybook/test';

import * as actual from './useTodos';
export * from './useTodos'
// Mock for useUserData hook
export const useTodos = fn(actual.useTodos).mockName('useTodos');