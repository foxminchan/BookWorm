import * as v from "valibot";

const MAX_AUTHOR_NAME_LENGTH = 100;

export const createAuthorSchema = v.object({
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_AUTHOR_NAME_LENGTH,
      `Name must be ${MAX_AUTHOR_NAME_LENGTH} characters or less`,
    ),
  ),
});

export const updateAuthorSchema = v.object({
  id: v.pipe(v.string(), v.uuid("Invalid ID format")),
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_AUTHOR_NAME_LENGTH,
      `Name must be ${MAX_AUTHOR_NAME_LENGTH} characters or less`,
    ),
  ),
});

export type CreateAuthorInput = v.InferOutput<typeof createAuthorSchema>;
export type UpdateAuthorInput = v.InferOutput<typeof updateAuthorSchema>;
