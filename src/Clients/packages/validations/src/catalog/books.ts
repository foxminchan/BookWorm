import { z } from "zod";

export const createBookSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  price: z.number().positive("Price must be greater than 0"),
  priceSale: z.number().nullable().optional(),
  categoryId: z
    .string()
    .uuid("Invalid category ID format")
    .min(1, "Category is required"),
  publisherId: z
    .string()
    .uuid("Invalid publisher ID format")
    .min(1, "Publisher is required"),
  authorIds: z
    .array(z.string().uuid("Invalid author ID format"))
    .min(1, "At least one author is required"),
});

export const updateBookSchema = z.object({
  id: z.string().uuid("Invalid ID format").min(1, "ID is required"),
  name: z.string().min(1, "Name is required"),
  description: z.string().min(1, "Description is required"),
  price: z.number().positive("Price must be greater than 0"),
  priceSale: z.number().nullable().optional(),
  categoryId: z
    .string()
    .uuid("Invalid category ID format")
    .min(1, "Category is required"),
  publisherId: z
    .string()
    .uuid("Invalid publisher ID format")
    .min(1, "Publisher is required"),
  authorIds: z
    .array(z.string().uuid("Invalid author ID format"))
    .min(1, "At least one author is required"),
  isRemoveImage: z.boolean().optional(),
});

export type CreateBookInput = z.infer<typeof createBookSchema>;
export type UpdateBookInput = z.infer<typeof updateBookSchema>;
