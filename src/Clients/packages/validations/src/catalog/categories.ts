import * as v from "valibot";

const MAX_CATEGORY_NAME_LENGTH = 50;

export const createCategorySchema = v.object({
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_CATEGORY_NAME_LENGTH,
      `Name must be ${MAX_CATEGORY_NAME_LENGTH} characters or less`,
    ),
  ),
});

export const updateCategorySchema = v.object({
  id: v.pipe(v.string(), v.uuid("Invalid ID format")),
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_CATEGORY_NAME_LENGTH,
      `Name must be ${MAX_CATEGORY_NAME_LENGTH} characters or less`,
    ),
  ),
});

export type CreateCategoryInput = v.InferOutput<typeof createCategorySchema>;
export type UpdateCategoryInput = v.InferOutput<typeof updateCategorySchema>;
