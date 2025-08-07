import type { Config } from 'jest'
import nextJest from 'next/jest.js'

const createJestConfig = nextJest({
    // Provide the path to your Next.js app to load next.config.js and .env files in your test environment
    dir: './',
})

// Add any custom config to be passed to Jest
const config: Config = {
    coverageProvider: 'v8',
    testEnvironment: 'jsdom',
    // Add more setup options before each test is run
    setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
    moduleNameMapper: {
        '^@/(.*)$': '<rootDir>/$1',
        '^@infrastructure/(.*)$': '<rootDir>/app/(infrastructure)/$1',
        '^@presentation/(.*)$': '<rootDir>/app/(presentation)/$1',
        '^@application/(.*)$': '<rootDir>/app/application/$1',
        '^@components/(.*)$': '<rootDir>/app/components/$1',
        '^@tests/(.*)$': '<rootDir>/app/tests/$1',
        '^@public/(.*)$': '<rootDir>/public/$1',
        '^@styles/(.*)$': '<rootDir>/app/styles/$1',
        '^@utils/(.*)$': '<rootDir>/app/utils/$1',
        '^@types/(.*)$': '<rootDir>/app/types/$1',
        '^@hooks/(.*)$': '<rootDir>/app/hooks/$1',
        '^@config/(.*)$': '<rootDir>/app/config/$1',
        '^@assets/(.*)$': '<rootDir>/app/assets/$1',
    },
    // Fix the testPathIgnorePatterns to use proper regex format
    testPathIgnorePatterns: [
        '/node_modules/',
        '/.next/',
        '.*\\.spec\\.ts$'  // Properly escaped regex for .spec.ts files
    ],
    // You can also use testMatch to specifically include only certain files
    testMatch: [
        '**/*.test.ts',
        '**/*.test.tsx',
        '**/__tests__/**/*.ts',
        '**/__tests__/**/*.tsx'
    ],
    // Add coverage configuration
    collectCoverage: true,
    coverageDirectory: 'coverage',
    collectCoverageFrom: [
        'app/**/*.{js,jsx,ts,tsx}',
        '!app/**/*.d.ts',
        '!app/**/*.stories.{js,jsx,ts,tsx}',
        '!app/**/_*.{js,jsx,ts,tsx}',
        '!app/**/page.tsx', // Exclude Next.js page files if needed
        '!app/**/layout.tsx', // Exclude layout files if needed
        '!app/(presentation)/api/route.{js,ts}', // Exclude API route handlers if needed
        '!**/node_modules/**',
    ],
    coverageThreshold: {
        global: {
            branches: 70,
            functions: 70,
            lines: 70,
            statements: 70,
        },
    },
    coverageReporters: ['json', 'lcov', 'text', 'clover', 'html'],
}

// createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
export default createJestConfig(config)