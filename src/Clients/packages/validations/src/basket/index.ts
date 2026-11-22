import * as v from "valibot";

export const basketItemRequestSchema = v.object({
  id: v.pipe(v.string(), v.minLength(1, "Item ID is required")),
  quantity: v.pipe(
    v.number(),
    v.integer(),
    v.gtValue(0, "Quantity must be greater than 0"),
  ),
});

export const createBasketSchema = v.object({
  items: v.pipe(
    v.array(basketItemRequestSchema),
    v.minLength(1, "At least one item is required"),
  ),
});

export const updateBasketSchema = v.object({
  items: v.pipe(
    v.array(basketItemRequestSchema),
    v.minLength(1, "At least one item is required"),
  ),
});

export type CreateBasketInput = v.InferOutput<typeof createBasketSchema>;
export type UpdateBasketInput = v.InferOutput<typeof updateBasketSchema>;
