import { PrismaClient } from "@prisma/client";

const prisma = new PrismaClient();

export const db = {
    user: prisma.user,
    // Add other models as needed
};