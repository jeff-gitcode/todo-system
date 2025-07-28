import { betterAuth } from "better-auth";
import { nextCookies } from "better-auth/next-js";

export const auth = betterAuth({
  // ...你的配置，如数据库、邮件、OAuth等
  plugins: [nextCookies()],
});