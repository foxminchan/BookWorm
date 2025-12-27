import { z } from "zod";

const MAX_AUTHOR_NAME_LENGTH = 100;

export const createAuthorSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_AUTHOR_NAME_LENGTH,
      `Name must be ${MAX_AUTHOR_NAME_LENGTH} characters or less`,
    ),
});

export const updateAuthorSchema = z.object({
  id: z.uuid({ message: "Invalid ID format" }),
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_AUTHOR_NAME_LENGTH,
      `Name must be ${MAX_AUTHOR_NAME_LENGTH} characters or less`,
    ),
});

export type CreateAuthorInput = z.infer<typeof createAuthorSchema>;
export type UpdateAuthorInput = z.infer<typeof updateAuthorSchema>;
