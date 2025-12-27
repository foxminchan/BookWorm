import { z } from "zod";

export const orderStatusSchema = z.enum(["New", "Cancelled", "Completed"]);

export const listOrdersSchema = z.object({
  pageIndex: z.number().int().positive().default(1).optional(),
  pageSize: z.number().int().positive().default(10).optional(),
  status: orderStatusSchema.optional(),
  buyerId: z.uuid({ message: "Invalid buyer ID format" }).optional(),
});

export type ListOrdersInput = z.infer<typeof listOrdersSchema>;
