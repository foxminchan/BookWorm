import * as v from "valibot";

const MAX_LENGTH_MEDIUM = 50;

const addressSchema = v.object({
  street: v.pipe(
    v.string(),
    v.minLength(1, "Street is required"),
    v.maxLength(
      MAX_LENGTH_MEDIUM,
      `Street must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    ),
  ),
  city: v.pipe(
    v.string(),
    v.minLength(1, "City is required"),
    v.maxLength(
      MAX_LENGTH_MEDIUM,
      `City must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    ),
  ),
  province: v.pipe(
    v.string(),
    v.minLength(1, "Province is required"),
    v.maxLength(
      MAX_LENGTH_MEDIUM,
      `Province must not exceed ${MAX_LENGTH_MEDIUM} characters`,
    ),
  ),
});

export const createBuyerSchema = addressSchema;
export const updateAddressSchema = addressSchema;

export const listBuyersSchema = v.object({
  pageIndex: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 1),
  pageSize: v.optional(v.pipe(v.number(), v.integer(), v.gtValue(0)), 10),
});

export type CreateBuyerInput = v.InferOutput<typeof createBuyerSchema>;
export type UpdateAddressInput = v.InferOutput<typeof updateAddressSchema>;
export type ListBuyersInput = v.InferOutput<typeof listBuyersSchema>;
