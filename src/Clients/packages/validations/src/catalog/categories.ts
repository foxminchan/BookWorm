import { z } from "zod";

export const createCategorySchema = z.object({
  name: z.string().min(1, "Name is required"),
});

export const updateCategorySchema = z.object({
  id: z.string().uuid("Invalid ID format").min(1, "ID is required"),
  name: z.string().min(1, "Name is required"),
});

export type CreateCategoryInput = z.infer<typeof createCategorySchema>;
export type UpdateCategoryInput = z.infer<typeof updateCategorySchema>;
