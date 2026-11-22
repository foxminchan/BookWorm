"use client";

import { useComponent } from "@copilotkit/react-core/v2";
import { toStandardJsonSchema } from "@valibot/to-json-schema";
import { ShoppingCart, Star } from "lucide-react";
import * as v from "valibot";

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

const bookCardSchema = v.object({
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

type BookCardProps = v.InferOutput<typeof bookCardSchema>;

const bookListSchema = v.object({
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

type BookListProps = v.InferOutput<typeof bookListSchema>;

const basketSummarySchema = v.object({
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

type BasketSummaryProps = v.InferOutput<typeof basketSummarySchema>;

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
      parameters: toStandardJsonSchema(bookCardSchema),
      render: BookCardComponent,
    },
    [],
  );

  useComponent(
    {
      name: "showBookList",
      description:
        "Render a compact list of books in chat, e.g. for search results or recommendations. Each row shows title, authors, and price.",
      parameters: toStandardJsonSchema(bookListSchema),
      render: BookListComponent,
    },
    [],
  );

  useComponent(
    {
      name: "showBasketSummary",
      description:
        "Render an inline basket summary in chat showing all current basket items, quantities, and the total price.",
      parameters: toStandardJsonSchema(basketSummarySchema),
      render: BasketSummaryComponent,
    },
    [],
  );
}
