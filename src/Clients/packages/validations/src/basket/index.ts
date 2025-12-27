import { z } from "zod";

export const basketItemRequestSchema = z.object({
  id: z.uuid({ message: "Invalid ID format" }),
  quantity: z
    .number()
    .int()
    .positive({ message: "Quantity must be greater than 0" }),
});

export const createBasketSchema = z.object({
  items: z
    .array(basketItemRequestSchema)
    .nonempty({ message: "At least one item is required" }),
});

export const updateBasketSchema = z.object({
  items: z
    .array(basketItemRequestSchema)
    .nonempty({ message: "At least one item is required" }),
});

export type CreateBasketInput = z.infer<typeof createBasketSchema>;
export type UpdateBasketInput = z.infer<typeof updateBasketSchema>;
