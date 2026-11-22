"use client";

import type { ComponentType } from "react";

import {
  type GenerativeUIAriaAttributes,
  useGenerativeUIAccessibility,
} from "@copilotkit/react-core/generative-ui";
import { ShoppingCart, Star } from "lucide-react";

import type { BasketItem } from "@workspace/types/basket";
import { Badge } from "@workspace/ui/components/badge";
import { Bubble, BubbleContent } from "@workspace/ui/components/bubble";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Message, MessageContent } from "@workspace/ui/components/message";
import { Separator } from "@workspace/ui/components/separator";
import { calculateDiscount, formatPrice } from "@workspace/utils/format";

import type {
  A2UIBookCardProps,
  BasketSummaryProps,
  BookCardProps,
  BookListProps,
} from "./definitions";

type AriaProps = {
  ariaProps?: GenerativeUIAriaAttributes;
};

export function BookCardView({
  title,
  authors,
  price,
  priceSale,
  averageRating,
  status,
  description,
  ariaProps,
}: (BookCardProps | A2UIBookCardProps) & AriaProps) {
  const discount = priceSale ? calculateDiscount(price, priceSale) : null;

  return (
    <Card className="w-full" {...ariaProps}>
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

export function BookListView({
  query,
  books,
  ariaProps,
}: BookListProps & AriaProps) {
  return (
    <Message {...ariaProps}>
      <MessageContent>
        <Card className="w-full">
          {query && (
            <CardHeader className="pb-2">
              <CardTitle className="text-muted-foreground text-xs font-medium">
                Results for &ldquo;{query}&rdquo; — {books.length} book
                {books.length === 1 ? "" : "s"}
              </CardTitle>
            </CardHeader>
          )}
          <CardContent className={query ? "space-y-2 pt-0" : "space-y-2 pt-4"}>
            {books.length === 0 ? (
              <Bubble variant="muted">
                <BubbleContent className="text-muted-foreground">
                  No books found.
                </BubbleContent>
              </Bubble>
            ) : (
              <div className="space-y-2">
                {books.map((book) => (
                  <div
                    key={book.id}
                    className="bg-card hover:bg-accent/40 flex items-center justify-between rounded-lg border p-2.5 transition-colors"
                  >
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium">
                        {book.title}
                      </p>
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
          </CardContent>
        </Card>
      </MessageContent>
    </Message>
  );
}

export function BasketSummaryView({
  itemCount,
  totalPrice,
  items,
  ariaProps,
}: BasketSummaryProps & AriaProps) {
  return (
    <Card className="w-full" {...ariaProps}>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center gap-2 text-sm font-semibold">
          <ShoppingCart className="size-4" />
          <span>
            Your Basket ({itemCount} item{itemCount === 1 ? "" : "s"})
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
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
                    {formatPrice(
                      (item.priceSale ?? item.price) * item.quantity,
                    )}
                  </span>
                </div>
              ))}
            </div>
            <Separator />
            <div className="flex items-center justify-between text-sm font-bold">
              <span>Total</span>
              <span>{formatPrice(totalPrice)}</span>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

export function withA2UIAccessibility<TProps extends object>(
  Component: ComponentType<TProps & AriaProps>,
) {
  return function A2UIAccessibleComponent(props: TProps) {
    const ariaProps = useGenerativeUIAccessibility();
    return <Component {...props} ariaProps={ariaProps} />;
  };
}
