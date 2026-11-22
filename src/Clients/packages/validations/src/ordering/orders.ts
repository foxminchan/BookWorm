import * as v from "valibot";

export const orderStatusSchema = v.picklist(["New", "Cancelled", "Completed"]);

export const listOrdersSchema = v.object({
  pageIndex: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 1),
  pageSize: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 10),
  status: v.optional(orderStatusSchema),
  buyerId: v.optional(v.pipe(v.string(), v.uuid("Invalid buyer ID format"))),
});

export type ListOrdersInput = v.InferOutput<typeof listOrdersSchema>;
