import { z } from "zod";

const MAX_COMMENT_LENGTH = 1000;

export const createFeedbackSchema = z.object({
  bookId: z.uuid({ error: "Invalid book ID format" }),
  firstName: z.string().min(1, "First name is required").optional().nullable(),
  lastName: z.string().min(1, "Last name is required").optional().nullable(),
  comment: z
    .string()
    .max(
      MAX_COMMENT_LENGTH,
      `Comment must be ${MAX_COMMENT_LENGTH} characters or less`,
    )
    .optional()
    .nullable(),
  rating: z
    .number()
    .int("Rating must be an integer")
    .min(0, "Rating must be between 0 and 5")
    .max(5, "Rating must be between 0 and 5"),
});

export const listFeedbacksSchema = z.object({
  bookId: z.uuid({ error: "Invalid book ID format" }),
  pageIndex: z.number().int().positive().default(1).optional(),
  pageSize: z.number().int().positive().default(10).optional(),
  orderBy: z.string().default("rating").optional(),
  isDescending: z.boolean().default(false).optional(),
});

export type CreateFeedbackInput = z.infer<typeof createFeedbackSchema>;
export type ListFeedbacksInput = z.infer<typeof listFeedbacksSchema>;
