import { betterAuth } from "better-auth";
import { nextCookies } from "better-auth/next-js";

// export const auth = betterAuth({
//   // ...你的配置，如数据库、邮件、OAuth等
//   // emailAndPassword: {
//   //   enabled: true,
//   //   autoSignIn: false //defaults to true
//   // },
//   plugins: [nextCookies()],

//   // secret: process.env.AUTH_SECRET,
//   // database: {
//   //   type: "postgres",
//   //   url: process.env.DATABASE_URL
//   // },
//   // session: {
//   //   // Add supported session options here if needed
//   // }
// });

export const auth = betterAuth({
  secret: process.env.AUTH_SECRET,
  database: {
    type: "postgres",
    url: process.env.DATABASE_URL
  },
  session: {
    fields: {
      expiresAt: "expires", // Map your existing `expires` field to Better Auth's `expiresAt`
      token: "sessionToken" // Map your existing `sessionToken` field to Better Auth's `token`
    }
  },
})