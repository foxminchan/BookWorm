import { z } from "zod";

export const createAuthorSchema = z.object({
  name: z.string().min(1, "Name is required"),
});

export const updateAuthorSchema = z.object({
  id: z.string().uuid("Invalid ID format").min(1, "ID is required"),
  name: z.string().min(1, "Name is required"),
});

export type CreateAuthorInput = z.infer<typeof createAuthorSchema>;
export type UpdateAuthorInput = z.infer<typeof updateAuthorSchema>;
