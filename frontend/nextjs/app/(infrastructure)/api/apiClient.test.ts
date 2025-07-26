import { api, localApi, setAuthToken } from './apiClient';

describe('apiClient', () => {
    describe('api instance', () => {
        it('should have correct baseURL and headers', () => {
            // Arrange
            // (No setup needed for this test)

            // Act
            const baseURL = api.defaults.baseURL;
            const contentType = api.defaults.headers['Content-Type'];

            // Assert
            expect(baseURL).toBe('http://localhost:3001');
            expect(contentType).toBe('application/json');
        });
    });

    describe('localApi instance', () => {
        it('should have correct baseURL and headers', () => {
            // Arrange
            // (No setup needed for this test)

            // Act
            const baseURL = localApi.defaults.baseURL;
            const contentType = localApi.defaults.headers['Content-Type'];

            // Assert
            expect(baseURL).toBe('/api');
            expect(contentType).toBe('application/json');
        });
    });

    describe('setAuthToken', () => {
        afterEach(() => {
            // Arrange
            // Clean up after each test
            delete api.defaults.headers.common['Authorization'];
        });

        it('should set Authorization header when token is provided', () => {
            // Arrange
            const token = 'test-token';

            // Act
            setAuthToken(token);

            // Assert
            expect(api.defaults.headers.common['Authorization']).toBe('Bearer test-token');
        });

        it('should remove Authorization header when token is not provided', () => {
            // Arrange
            setAuthToken('test-token');

            // Act
            setAuthToken('');

            // Assert
            expect(api.defaults.headers.common['Authorization']).toBeUndefined();
        });
    });
});