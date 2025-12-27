import { z } from "zod";

const MAX_CATEGORY_NAME_LENGTH = 50;

export const createCategorySchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_CATEGORY_NAME_LENGTH,
      `Name must be ${MAX_CATEGORY_NAME_LENGTH} characters or less`,
    ),
});

export const updateCategorySchema = z.object({
  id: z.uuid({ message: "Invalid ID format" }),
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_CATEGORY_NAME_LENGTH,
      `Name must be ${MAX_CATEGORY_NAME_LENGTH} characters or less`,
    ),
});

export type CreateCategoryInput = z.infer<typeof createCategorySchema>;
export type UpdateCategoryInput = z.infer<typeof updateCategorySchema>;
