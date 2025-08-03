import { betterAuth } from 'better-auth'
import { prismaAdapter } from 'better-auth/adapters/prisma'
import prisma from '@/lib/prisma'
import { redirect } from 'next/navigation'

export const auth = betterAuth({
  database: prismaAdapter(prisma, {
    provider: 'postgresql',
  }),
  emailAndPassword: {
    enabled: true,
  },
  // signUp: {
  //   onSuccess: async () => {
  //     redirect('/dashboard')
  //   },
  // },
  // signIn: {
  //   onSuccess: async () => {
  //     redirect('/dashboard')
  //   },
  // },
  // trustedOrigins: ['http://localhost:3001'],
})