import { toStandardJsonSchema } from "@valibot/to-json-schema";
import * as v from "valibot";

// ─── Valibot Schemas (shared between A2UI catalog & useComponent hooks) ─────

export const a2uiBookCardSchema = v.object({
  bookId: v.pipe(
    v.optional(v.string()),
    v.description("Unique identifier for the book"),
  ),
  title: v.pipe(v.string(), v.description("Title of the book")),
  authors: v.pipe(v.array(v.string()), v.description("List of author names")),
  price: v.pipe(v.number(), v.description("Original price")),
  priceSale: v.pipe(
    v.nullish(v.number()),
    v.description("Sale price if on discount"),
  ),
  imageUrl: v.pipe(v.nullish(v.string()), v.description("Cover image URL")),
  averageRating: v.pipe(
    v.optional(v.number()),
    v.description("Average star rating (0–5)"),
  ),
  status: v.pipe(
    v.picklist(["InStock", "OutOfStock"]),
    v.description("Availability status"),
  ),
  description: v.pipe(
    v.nullish(v.string()),
    v.description("Short description or blurb"),
  ),
});

export type A2UIBookCardProps = v.InferOutput<typeof a2uiBookCardSchema>;

export const bookCardSchema = v.object({
  id: v.pipe(v.string(), v.description("Unique identifier for the book")),
  title: v.pipe(v.string(), v.description("Title of the book")),
  authors: v.pipe(v.array(v.string()), v.description("List of author names")),
  price: v.pipe(v.number(), v.description("Original price")),
  priceSale: v.pipe(
    v.nullish(v.number()),
    v.description("Sale price if on discount"),
  ),
  imageUrl: v.pipe(v.nullish(v.string()), v.description("Cover image URL")),
  averageRating: v.pipe(
    v.optional(v.number()),
    v.description("Average star rating (0–5)"),
  ),
  status: v.pipe(
    v.picklist(["InStock", "OutOfStock"]),
    v.description("Availability status"),
  ),
  description: v.pipe(
    v.nullish(v.string()),
    v.description("Short description or blurb"),
  ),
});

export type BookCardProps = v.InferOutput<typeof bookCardSchema>;

export const bookListSchema = v.object({
  query: v.pipe(
    v.optional(v.string()),
    v.description("Search query that produced these results"),
  ),
  books: v.pipe(
    v.array(
      v.object({
        id: v.string(),
        title: v.string(),
        authors: v.array(v.string()),
        price: v.number(),
        priceSale: v.nullish(v.number()),
        averageRating: v.optional(v.number()),
        status: v.picklist(["InStock", "OutOfStock"]),
      }),
    ),
    v.description("List of books to display"),
  ),
});

export type BookListProps = v.InferOutput<typeof bookListSchema>;

export const basketSummarySchema = v.object({
  itemCount: v.pipe(
    v.number(),
    v.description("Total number of distinct items in basket"),
  ),
  totalPrice: v.pipe(v.number(), v.description("Aggregated total price")),
  items: v.pipe(
    v.array(
      v.object({
        id: v.string(),
        name: v.nullish(v.string()),
        quantity: v.number(),
        price: v.number(),
        priceSale: v.nullish(v.number()),
      }),
    ),
    v.description("Individual basket lines"),
  ),
});

export type BasketSummaryProps = v.InferOutput<typeof basketSummarySchema>;

// ─── Standard JSON Schemas ───────────────────────────────────────────────────

export const a2uiBookCardJsonSchema = toStandardJsonSchema(a2uiBookCardSchema);
export const bookCardJsonSchema = toStandardJsonSchema(bookCardSchema);
export const bookListJsonSchema = toStandardJsonSchema(bookListSchema);
export const basketSummaryJsonSchema =
  toStandardJsonSchema(basketSummarySchema);
