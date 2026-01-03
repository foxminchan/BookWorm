"use client";

import { useState } from "react";

import Image from "next/image";

import { Star } from "lucide-react";

import type { Book } from "@workspace/types/catalog/books";
import { Card, CardContent } from "@workspace/ui/components/card";

import { DEFAULT_BOOK_IMAGE } from "@/lib/constants";
import { calculateDiscount } from "@/lib/format";

type BookCardProps = {
  book: Book;
  onClick?: () => void;
};

export function BookCard({ book, onClick }: BookCardProps) {
  const [imgError, setImgError] = useState(false);

  return (
    <article
      className="group cursor-pointer border-none bg-transparent shadow-none"
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
      <Card className="group cursor-pointer border-none bg-transparent shadow-none">
        <CardContent className="p-0">
          <div className="bg-secondary relative mb-4 aspect-3/4 overflow-hidden rounded-lg">
            {book.priceSale && (
              <div className="bg-destructive text-destructive-foreground absolute top-2 left-2 z-10 rounded-full px-2 py-1 text-[10px] font-bold tracking-wider uppercase">
                -{calculateDiscount(book.price, book.priceSale)}% OFF
              </div>
            )}
            <Image
              src={
                imgError || !book.imageUrl ? DEFAULT_BOOK_IMAGE : book.imageUrl
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
              <p className="text-muted-foreground text-[10px] font-bold tracking-widest uppercase">
                {book.category?.name}
              </p>
              <div className="flex items-center gap-0.5 text-xs">
                <Star className="fill-primary text-primary size-3" />
                <span>{book.averageRating?.toFixed(1) ?? "0.0"}</span>
              </div>
            </div>
            <h3 className="font-serif text-lg leading-snug font-medium group-hover:underline">
              {book.name}
            </h3>
            <p className="text-muted-foreground text-sm">
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
                  <span className="text-primary font-bold">
                    ${book.priceSale.toFixed(2)}
                  </span>
                  <span className="text-muted-foreground decoration-muted-foreground/50 text-sm line-through">
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
