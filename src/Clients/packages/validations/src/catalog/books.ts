import { z } from "zod";

const MAX_BOOK_DESCRIPTION_LENGTH = 500;
const MAX_BOOK_NAME_LENGTH = 50;

const baseBookSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_BOOK_NAME_LENGTH,
      `Name must be ${MAX_BOOK_NAME_LENGTH} characters or less`,
    ),
  description: z
    .string()
    .min(1, "Description is required")
    .max(
      MAX_BOOK_DESCRIPTION_LENGTH,
      `Description must be ${MAX_BOOK_DESCRIPTION_LENGTH} characters or less`,
    ),
  price: z.number().positive("Price must be greater than 0"),
  priceSale: z.number().nullable().optional(),
  categoryId: z.string().min(1, "Category is required"),
  publisherId: z.string().min(1, "Publisher is required"),
});

export const createBookSchema = baseBookSchema.extend({
  authorIds: z
    .array(z.string().min(1, "Invalid author ID format"))
    .min(1, "At least one author is required"),
});

export const updateBookSchema = baseBookSchema.extend({
  id: z.uuid({ error: "Invalid ID format" }),
  authorIds: z
    .array(z.uuid({ error: "Invalid author ID format" }))
    .min(1, "At least one author is required"),
  isRemoveImage: z.boolean().optional(),
});

export type CreateBookInput = z.infer<typeof createBookSchema>;
export type UpdateBookInput = z.infer<typeof updateBookSchema>;
