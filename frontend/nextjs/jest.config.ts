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
}

// createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
export default createJestConfig(config)