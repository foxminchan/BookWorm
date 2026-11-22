import * as v from "valibot";

const MAX_PUBLISHER_NAME_LENGTH = 50;

export const createPublisherSchema = v.object({
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_PUBLISHER_NAME_LENGTH,
      `Name must be ${MAX_PUBLISHER_NAME_LENGTH} characters or less`,
    ),
  ),
});

export const updatePublisherSchema = v.object({
  id: v.pipe(v.string(), v.uuid("Invalid ID format")),
  name: v.pipe(
    v.string(),
    v.minLength(1, "Name is required"),
    v.maxLength(
      MAX_PUBLISHER_NAME_LENGTH,
      `Name must be ${MAX_PUBLISHER_NAME_LENGTH} characters or less`,
    ),
  ),
});

export type CreatePublisherInput = v.InferOutput<typeof createPublisherSchema>;
export type UpdatePublisherInput = v.InferOutput<typeof updatePublisherSchema>;
