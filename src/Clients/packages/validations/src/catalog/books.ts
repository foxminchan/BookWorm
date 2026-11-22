import * as v from "valibot";

const MAX_BOOK_DESCRIPTION_LENGTH = 500;
const MAX_BOOK_NAME_LENGTH = 50;

const baseBookEntries = {
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_BOOK_NAME_LENGTH,
      `Name must be ${MAX_BOOK_NAME_LENGTH} characters or less`,
    ),
  ),
  description: v.pipe(
    v.string(),
    v.minLength(1, "Description is required"),
    v.maxLength(
      MAX_BOOK_DESCRIPTION_LENGTH,
      `Description must be ${MAX_BOOK_DESCRIPTION_LENGTH} characters or less`,
    ),
  ),
  price: v.pipe(
    v.number("Price must be a number"),
    v.gtValue(0, "Price must be greater than 0"),
  ),
  priceSale: v.nullish(v.number()),
  categoryId: v.pipe(v.string(), v.minLength(1, "Category is required")),
  publisherId: v.pipe(v.string(), v.minLength(1, "Publisher is required")),
};

export const createBookSchema = v.object({
  ...baseBookEntries,
  authorIds: v.pipe(
    v.array(v.pipe(v.string(), v.minLength(1, "Invalid author ID format"))),
    v.minLength(1, "At least one author is required"),
  ),
});

export const updateBookSchema = v.object({
  ...baseBookEntries,
  id: v.pipe(v.string(), v.uuid("Invalid ID format")),
  authorIds: v.pipe(
    v.array(v.pipe(v.string(), v.uuid("Invalid author ID format"))),
    v.minLength(1, "At least one author is required"),
  ),
  isRemoveImage: v.optional(v.boolean()),
});

export type CreateBookInput = v.InferOutput<typeof createBookSchema>;
export type UpdateBookInput = v.InferOutput<typeof updateBookSchema>;
