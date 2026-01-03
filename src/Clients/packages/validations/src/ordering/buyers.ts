import { z } from "zod";

const MAX_LENGTH_MEDIUM = 50;

export const createBuyerSchema = z.object({
  street: z
    .string()
    .min(1, { error: "Street is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `Street must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  city: z
    .string()
    .min(1, { error: "City is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `City must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  province: z
    .string()
    .min(1, { error: "Province is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `Province must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
});

export const updateAddressSchema = z.object({
  street: z
    .string()
    .min(1, { error: "Street is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `Street must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  city: z
    .string()
    .min(1, { error: "City is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `City must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  province: z
    .string()
    .min(1, { error: "Province is required" })
    .max(MAX_LENGTH_MEDIUM, {
      error: `Province must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
});

export const listBuyersSchema = z.object({
  pageIndex: z.number().int().positive().default(1).optional(),
  pageSize: z.number().int().positive().default(10).optional(),
});

export type CreateBuyerInput = z.infer<typeof createBuyerSchema>;
export type UpdateAddressInput = z.infer<typeof updateAddressSchema>;
export type ListBuyersInput = z.infer<typeof listBuyersSchema>;
