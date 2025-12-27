import { z } from "zod";

const MAX_PUBLISHER_NAME_LENGTH = 50;

export const createPublisherSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_PUBLISHER_NAME_LENGTH,
      `Name must be ${MAX_PUBLISHER_NAME_LENGTH} characters or less`,
    ),
});

export const updatePublisherSchema = z.object({
  id: z.uuid({ message: "Invalid ID format" }),
  name: z
    .string()
    .min(1, "Name is required")
    .max(
      MAX_PUBLISHER_NAME_LENGTH,
      `Name must be ${MAX_PUBLISHER_NAME_LENGTH} characters or less`,
    ),
});

export type CreatePublisherInput = z.infer<typeof createPublisherSchema>;
export type UpdatePublisherInput = z.infer<typeof updatePublisherSchema>;
