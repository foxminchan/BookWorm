"use client";

import { Card, CardContent } from "@workspace/ui/components/card";
import { calculateDiscount } from "@/lib/format";
import type { Book } from "@workspace/types/catalog/books";
import { useState } from "react";
import { Star } from "lucide-react";
import Image from "next/image";

type BookCardProps = {
  book: Book;
  onClick?: () => void;
};

export function BookCard({ book, onClick }: BookCardProps) {
  const [imgError, setImgError] = useState(false);

  return (
    <article
      className="border-none shadow-none group cursor-pointer bg-transparent"
      onClick={onClick}
      role="listitem"
      itemScope
      itemType="https://schema.org/Book"
      tabIndex={0}
      onKeyDown={(e) => {
        if ((e.key === "Enter" || e.key === " ") && onClick) {
          e.preventDefault();
          onClick();
        }
      }}
    >
      <Card className="border-none shadow-none group cursor-pointer bg-transparent">
        <CardContent className="p-0">
          <div className="aspect-3/4 overflow-hidden rounded-lg mb-4 bg-secondary relative">
            {book.priceSale && (
              <div className="absolute top-2 left-2 z-10 bg-destructive text-destructive-foreground text-[10px] font-bold px-2 py-1 rounded-full uppercase tracking-wider">
                -{calculateDiscount(book.price, book.priceSale)}% OFF
              </div>
            )}
            <Image
              src={
                imgError || !book.imageUrl
                  ? "/book-placeholder.svg"
                  : book.imageUrl
              }
              alt={book.name ?? "Book cover"}
              fill
              sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
              className="object-cover transition-transform duration-500 group-hover:scale-105"
              onError={() => setImgError(true)}
            />
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <p className="text-[10px] uppercase tracking-widest text-muted-foreground font-bold">
                {book.category?.name}
              </p>
              <div className="flex items-center text-xs gap-0.5">
                <Star className="size-3 fill-primary text-primary" />
                <span>{book.averageRating?.toFixed(1) ?? "0.0"}</span>
              </div>
            </div>
            <h3 className="font-serif font-medium text-lg leading-snug group-hover:underline">
              {book.name}
            </h3>
            <p className="text-sm text-muted-foreground">
              {book.authors && book.authors.length > 0
                ? book.authors.length > 3
                  ? `${book.authors
                      .slice(0, 3)
                      .map((a) => a.name)
                      .join(", ")} +${book.authors.length - 3} more`
                  : book.authors.map((a) => a.name).join(", ")
                : "Unknown Author"}
            </p>
            <div className="flex items-center gap-2 pt-1">
              {book.priceSale ? (
                <>
                  <span className="font-bold text-primary">
                    ${book.priceSale.toFixed(2)}
                  </span>
                  <span className="text-sm text-muted-foreground line-through decoration-muted-foreground/50">
                    ${book.price.toFixed(2)}
                  </span>
                </>
              ) : (
                <span className="font-bold">${book.price.toFixed(2)}</span>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </article>
  );
}
