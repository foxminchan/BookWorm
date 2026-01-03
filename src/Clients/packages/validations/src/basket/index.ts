import { z } from "zod";

export const basketItemRequestSchema = z.object({
  id: z.string().min(1, { error: "Item ID is required" }),
  quantity: z
    .number()
    .int()
    .positive({ error: "Quantity must be greater than 0" }),
});

export const createBasketSchema = z.object({
  items: z
    .array(basketItemRequestSchema)
    .min(1, { error: "At least one item is required" }),
});

export const updateBasketSchema = z.object({
  items: z
    .array(basketItemRequestSchema)
    .min(1, { error: "At least one item is required" }),
});

export type CreateBasketInput = z.infer<typeof createBasketSchema>;
export type UpdateBasketInput = z.infer<typeof updateBasketSchema>;
