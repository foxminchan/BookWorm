import { z } from "zod";

export const createPublisherSchema = z.object({
  name: z.string().min(1, "Name is required"),
});

export const updatePublisherSchema = z.object({
  id: z.string().uuid("Invalid ID format").min(1, "ID is required"),
  name: z.string().min(1, "Name is required"),
});

export type CreatePublisherInput = z.infer<typeof createPublisherSchema>;
export type UpdatePublisherInput = z.infer<typeof updatePublisherSchema>;
