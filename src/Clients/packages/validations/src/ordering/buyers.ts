import { z } from "zod";

const MAX_LENGTH_MEDIUM = 50;

export const createBuyerSchema = z.object({
  street: z
    .string()
    .min(1, { message: "Street is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `Street must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  city: z
    .string()
    .min(1, { message: "City is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `City must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  province: z
    .string()
    .min(1, { message: "Province is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `Province must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
});

export const updateAddressSchema = z.object({
  street: z
    .string()
    .min(1, { message: "Street is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `Street must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  city: z
    .string()
    .min(1, { message: "City is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `City must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
  province: z
    .string()
    .min(1, { message: "Province is required" })
    .max(MAX_LENGTH_MEDIUM, {
      message: `Province must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    }),
});

export const listBuyersSchema = z.object({
  pageIndex: z.number().int().positive().default(1).optional(),
  pageSize: z.number().int().positive().default(10).optional(),
});

export type CreateBuyerInput = z.infer<typeof createBuyerSchema>;
export type UpdateAddressInput = z.infer<typeof updateAddressSchema>;
export type ListBuyersInput = z.infer<typeof listBuyersSchema>;
