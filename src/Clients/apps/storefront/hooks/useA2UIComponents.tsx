"use client";

import { useComponent } from "@copilotkit/react-core/v2";
import { ShoppingCart, Star } from "lucide-react";
import { z } from "zod";

import type { BasketItem } from "@workspace/types/basket";
import { Badge } from "@workspace/ui/components/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { calculateDiscount, formatPrice } from "@workspace/utils/format";

// ─── Schemas ────────────────────────────────────────────────────────────────

const bookCardSchema = z.object({
  id: z.string().describe("Unique identifier for the book"),
  title: z.string().describe("Title of the book"),
  authors: z.array(z.string()).describe("List of author names"),
  price: z.number().describe("Original price"),
  priceSale: z
    .number()
    .nullable()
    .optional()
    .describe("Sale price if on discount"),
  imageUrl: z.string().nullable().optional().describe("Cover image URL"),
  averageRating: z.number().optional().describe("Average star rating (0–5)"),
  status: z.enum(["InStock", "OutOfStock"]).describe("Availability status"),
  description: z
    .string()
    .nullable()
    .optional()
    .describe("Short description or blurb"),
});

type BookCardProps = z.infer<typeof bookCardSchema>;

const bookListSchema = z.object({
  query: z
    .string()
    .optional()
    .describe("Search query that produced these results"),
  books: z
    .array(
      z.object({
        id: z.string(),
        title: z.string(),
        authors: z.array(z.string()),
        price: z.number(),
        priceSale: z.number().nullable().optional(),
        averageRating: z.number().optional(),
        status: z.enum(["InStock", "OutOfStock"]),
      }),
    )
    .describe("List of books to display"),
});

type BookListProps = z.infer<typeof bookListSchema>;

const basketSummarySchema = z.object({
  itemCount: z.number().describe("Total number of distinct items in basket"),
  totalPrice: z.number().describe("Aggregated total price"),
  items: z
    .array(
      z.object({
        id: z.string(),
        name: z.string().nullable().optional(),
        quantity: z.number(),
        price: z.number(),
        priceSale: z.number().nullable().optional(),
      }),
    )
    .describe("Individual basket lines"),
});

type BasketSummaryProps = z.infer<typeof basketSummarySchema>;

// ─── Components ─────────────────────────────────────────────────────────────

function BookCardComponent({
  title,
  authors,
  price,
  priceSale,
  averageRating,
  status,
  description,
}: BookCardProps) {
  const discount = priceSale ? calculateDiscount(price, priceSale) : null;

  return (
    <Card className="w-full">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-2">
          <CardTitle className="text-base leading-tight">{title}</CardTitle>
          <Badge variant={status === "InStock" ? "default" : "secondary"}>
            {status === "InStock" ? "In Stock" : "Out of Stock"}
          </Badge>
        </div>
        {authors.length > 0 && (
          <p className="text-muted-foreground text-xs">{authors.join(", ")}</p>
        )}
      </CardHeader>
      <CardContent className="space-y-2">
        {description && (
          <p className="text-muted-foreground line-clamp-2 text-xs">
            {description}
          </p>
        )}
        <div className="flex items-center gap-3">
          <div className="flex flex-col">
            {priceSale ? (
              <>
                <span className="font-semibold text-green-700 dark:text-green-400">
                  {formatPrice(priceSale)}
                </span>
                <span className="text-muted-foreground text-xs line-through">
                  {formatPrice(price)}
                </span>
              </>
            ) : (
              <span className="font-semibold">{formatPrice(price)}</span>
            )}
          </div>
          {discount !== null && (
            <Badge variant="destructive" className="text-xs">
              -{discount}%
            </Badge>
          )}
          {averageRating !== undefined && (
            <div className="ml-auto flex items-center gap-1 text-xs">
              <Star className="size-3 fill-yellow-400 text-yellow-400" />
              <span>{averageRating.toFixed(1)}</span>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

function BookListComponent({ query, books }: BookListProps) {
  return (
    <div className="space-y-2">
      {query && (
        <p className="text-muted-foreground text-xs">
          Results for &ldquo;{query}&rdquo; — {books.length} book
          {books.length === 1 ? "" : "s"}
        </p>
      )}
      {books.length === 0 ? (
        <p className="text-muted-foreground text-sm">No books found.</p>
      ) : (
        <div className="space-y-2">
          {books.map((book) => (
            <div
              key={book.id}
              className="flex items-center justify-between rounded-lg border p-2"
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{book.title}</p>
                {book.authors.length > 0 && (
                  <p className="text-muted-foreground truncate text-xs">
                    {book.authors.join(", ")}
                  </p>
                )}
              </div>
              <div className="ml-2 flex shrink-0 flex-col items-end gap-0.5">
                {book.priceSale ? (
                  <>
                    <span className="text-sm font-semibold text-green-700 dark:text-green-400">
                      {formatPrice(book.priceSale)}
                    </span>
                    <span className="text-muted-foreground text-xs line-through">
                      {formatPrice(book.price)}
                    </span>
                  </>
                ) : (
                  <span className="text-sm font-semibold">
                    {formatPrice(book.price)}
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function BasketSummaryComponent({
  itemCount,
  totalPrice,
  items,
}: BasketSummaryProps) {
  return (
    <div className="space-y-3 rounded-lg border p-3">
      <div className="flex items-center gap-2">
        <ShoppingCart className="size-4" />
        <span className="text-sm font-semibold">
          Your Basket ({itemCount} item{itemCount === 1 ? "" : "s"})
        </span>
      </div>
      {items.length === 0 ? (
        <p className="text-muted-foreground text-sm">Your basket is empty.</p>
      ) : (
        <>
          <div className="space-y-1.5">
            {(items as BasketItem[]).map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between text-sm"
              >
                <span className="flex-1 truncate">
                  {item.name ?? "Unknown"} × {item.quantity}
                </span>
                <span className="ml-2 shrink-0 font-medium">
                  {formatPrice((item.priceSale ?? item.price) * item.quantity)}
                </span>
              </div>
            ))}
          </div>
          <div className="flex items-center justify-between border-t pt-2 text-sm font-bold">
            <span>Total</span>
            <span>{formatPrice(totalPrice)}</span>
          </div>
        </>
      )}
    </div>
  );
}

// ─── Hook ────────────────────────────────────────────────────────────────────

/**
 * Registers A2UI components that the backend agent can invoke to render
 * rich book-store UI cards directly inside the chat conversation.
 *
 * Registered components:
 * - `showBookCard`    — single book detail card
 * - `showBookList`    — compact list of books (search results / recommendations)
 * - `showBasketSummary` — live basket snapshot
 */
export function useA2UIComponents() {
  useComponent(
    {
      name: "showBookCard",
      description:
        "Render a rich book detail card in chat showing title, authors, price, rating, availability, and an optional description.",
      parameters: bookCardSchema,
      render: BookCardComponent,
    },
    [],
  );

  useComponent(
    {
      name: "showBookList",
      description:
        "Render a compact list of books in chat, e.g. for search results or recommendations. Each row shows title, authors, and price.",
      parameters: bookListSchema,
      render: BookListComponent,
    },
    [],
  );

  useComponent(
    {
      name: "showBasketSummary",
      description:
        "Render an inline basket summary in chat showing all current basket items, quantities, and the total price.",
      parameters: basketSummarySchema,
      render: BasketSummaryComponent,
    },
    [],
  );
}
