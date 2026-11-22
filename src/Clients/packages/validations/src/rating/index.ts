import * as v from "valibot";

const MAX_COMMENT_LENGTH = 1000;

export const createFeedbackSchema = v.object({
  bookId: v.pipe(v.string(), v.minLength(1, "Book ID is required")),
  firstName: v.nullish(
    v.pipe(v.string(), v.minLength(1, "First name is required")),
  ),
  lastName: v.nullish(
    v.pipe(v.string(), v.minLength(1, "Last name is required")),
  ),
  comment: v.nullish(
    v.pipe(
      v.string(),
      v.maxLength(
        MAX_COMMENT_LENGTH,
        `Comment must be ${MAX_COMMENT_LENGTH} characters or less`,
      ),
    ),
  ),
  rating: v.pipe(
    v.number(),
    v.integer("Rating must be an integer"),
    v.minValue(0, "Rating must be between 0 and 5"),
    v.maxValue(5, "Rating must be between 0 and 5"),
  ),
});

export const listFeedbacksSchema = v.object({
  bookId: v.pipe(v.string(), v.minLength(1, "Book ID is required")),
  pageIndex: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 1),
  pageSize: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 10),
  orderBy: v.optional(v.string(), "rating"),
  isDescending: v.optional(v.boolean(), false),
});

export type CreateFeedbackInput = v.InferOutput<typeof createFeedbackSchema>;
export type ListFeedbacksInput = v.InferOutput<typeof listFeedbacksSchema>;
