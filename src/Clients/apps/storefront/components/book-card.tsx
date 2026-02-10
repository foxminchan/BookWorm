"use client";

import { useState } from "react";

import Image from "next/image";

import { Star } from "lucide-react";

import type { Book } from "@workspace/types/catalog/books";
import { Badge } from "@workspace/ui/components/badge";
import { Card, CardContent } from "@workspace/ui/components/card";

import { DEFAULT_BOOK_IMAGE, currencyFormatter } from "@/lib/constants";
import { calculateDiscount, formatPrice } from "@/lib/format";

type BookCardProps = {
  book: Book;
  onClick?: () => void;
};

const MAX_VISIBLE_AUTHORS = 3;

function formatAuthorList(authors: Book["authors"]): string {
  if (!authors || authors.length === 0) return "Unknown Author";

  const names = authors.map((a) => a.name);

  if (names.length <= MAX_VISIBLE_AUTHORS) {
    return names.join(", ");
  }

  return `${names.slice(0, MAX_VISIBLE_AUTHORS).join(", ")} +${names.length - MAX_VISIBLE_AUTHORS} more`;
}

export function BookCard({ book, onClick }: Readonly<BookCardProps>) {
  const [imgError, setImgError] = useState(false);

  const authorNames =
    book.authors && book.authors.length > 0
      ? book.authors.map((a) => a.name)
      : [];
  const fullAuthorText =
    authorNames.length > 0 ? authorNames.join(", ") : "Unknown Author";
  const displayAuthorText = formatAuthorList(book.authors);
  const ratingText = book.averageRating?.toFixed(1) ?? "0.0";
  const discount = book.priceSale
    ? calculateDiscount(book.price, book.priceSale)
    : null;
  const altText =
    fullAuthorText === "Unknown Author"
      ? `Cover of ${book.name}`
      : `Cover of ${book.name} by ${fullAuthorText}`;
  const priceText = book.priceSale
    ? `Sale price ${formatPrice(book.priceSale)}, original price ${formatPrice(book.price)}`
    : formatPrice(book.price);

  const interactiveProps = onClick
    ? {
        role: "button" as const,
        tabIndex: 0,
        onClick,
        onKeyDown: (e: React.KeyboardEvent) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            onClick();
          }
        },
      }
    : {};

  return (
    <article
      className="group focus-visible:ring-ring focus-within:ring-ring cursor-pointer rounded-lg border-none bg-transparent shadow-none focus-within:ring-2 focus-within:ring-offset-2 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none"
      itemScope
      itemType="https://schema.org/Book"
      aria-label={`${book.name} by ${fullAuthorText}. ${priceText}. Rating ${ratingText} stars`}
      {...interactiveProps}
    >
      <Card className="border-none bg-transparent shadow-none">
        <CardContent className="p-0">
          <div className="bg-secondary relative mb-4 aspect-3/4 overflow-hidden rounded-lg">
            {discount !== null && (
              <Badge
                variant="destructive"
                className="absolute top-2 left-2 z-10 rounded-full px-2 py-1 text-[10px] font-bold tracking-wider uppercase"
                aria-label={`On sale: ${discount}% off`}
              >
                -{discount}% OFF
              </Badge>
            )}
            <Image
              src={
                imgError || !book.imageUrl ? DEFAULT_BOOK_IMAGE : book.imageUrl
              }
              alt={altText}
              fill
              sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
              className="object-cover transition-transform duration-500 group-hover:scale-105"
              onError={() => setImgError(true)}
            />
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <p className="text-muted-foreground text-[10px] font-bold tracking-widest uppercase">
                {book.category?.name}
              </p>
              <div className="flex items-center gap-0.5 text-xs">
                <Star className="fill-primary text-primary size-3" />
                <span>{ratingText}</span>
              </div>
            </div>
            <h3 className="font-serif text-lg leading-snug font-medium group-hover:underline">
              {book.name}
            </h3>
            <p className="text-muted-foreground text-sm">{displayAuthorText}</p>
            <div className="flex items-center gap-2 pt-1">
              {book.priceSale ? (
                <>
                  <span className="text-primary font-bold">
                    {currencyFormatter.format(book.priceSale)}
                  </span>
                  <span className="text-muted-foreground decoration-muted-foreground/50 text-sm line-through">
                    {currencyFormatter.format(book.price)}
                  </span>
                </>
              ) : (
                <span className="font-bold">
                  {currencyFormatter.format(book.price)}
                </span>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </article>
  );
}
