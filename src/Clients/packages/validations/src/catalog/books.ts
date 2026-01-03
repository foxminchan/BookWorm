import { z } from "zod";

const MAX_BOOK_DESCRIPTION_LENGTH = 500;
const MAX_BOOK_NAME_LENGTH = 50;

export const createBookSchema = z.object({
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
  categoryId: z.uuid({ message: "Invalid category ID format" }),
  publisherId: z.uuid({ message: "Invalid publisher ID format" }),
  authorIds: z
    .array(z.uuid({ message: "Invalid author ID format" }))
    .min(1, "At least one author is required"),
});

export const updateBookSchema = z.object({
  id: z.uuid({ error: "Invalid ID format" }),
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
  categoryId: z.uuid({ error: "Invalid category ID format" }),
  publisherId: z.uuid({ error: "Invalid publisher ID format" }),
  authorIds: z
    .array(z.uuid({ error: "Invalid author ID format" }))
    .min(1, "At least one author is required"),
  isRemoveImage: z.boolean().optional(),
});

export type CreateBookInput = z.infer<typeof createBookSchema>;
export type UpdateBookInput = z.infer<typeof updateBookSchema>;
