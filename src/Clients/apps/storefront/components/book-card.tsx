"use client";

import { Card, CardContent } from "@workspace/ui/components/card";
import { formatPrice, calculateDiscount } from "@/lib/format";
import type { Book } from "@workspace/types/catalog/books";

interface BookCardProps {
  book: Book;
  onClick?: () => void;
  showCategory?: boolean;
}

export function BookCard({
  book,
  onClick,
  showCategory = true,
}: BookCardProps) {
  const hasDiscount = book.priceSale && book.priceSale < book.price;

  return (
    <article
      className="border-none shadow-none group cursor-pointer bg-transparent"
      itemScope
      itemType="https://schema.org/Book"
      onClick={onClick}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if ((e.key === "Enter" || e.key === " ") && onClick) {
          e.preventDefault();
          onClick();
        }
      }}
    >
      <Card className="border-none shadow-none">
        <CardContent className="p-0">
          <div className="aspect-3/4 overflow-hidden rounded-lg mb-4 bg-secondary">
            <img
              src={book.imageUrl || "/placeholder.svg"}
              alt={book.name || "Book cover"}
              className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
              itemProp="image"
            />
          </div>
          <div className="space-y-1">
            {showCategory && book.category && (
              <p className="text-[10px] uppercase tracking-widest text-muted-foreground font-bold">
                {book.category.name}
              </p>
            )}
            <h3
              className="font-serif font-medium text-lg leading-snug group-hover:underline"
              itemProp="name"
            >
              {book.name}
            </h3>
            {book.authors && book.authors.length > 0 && (
              <p className="text-sm text-muted-foreground" itemProp="author">
                {book.authors.map((author) => author.name).join(", ")}
              </p>
            )}
            <div
              className="flex items-center gap-2 pt-2"
              itemProp="offers"
              itemScope
              itemType="https://schema.org/Offer"
            >
              {hasDiscount ? (
                <>
                  <span className="font-bold text-primary" itemProp="price">
                    {formatPrice(book.priceSale!)}
                  </span>
                  <span className="text-sm text-muted-foreground line-through decoration-muted-foreground/50">
                    {formatPrice(book.price)}
                  </span>
                  <span className="text-xs font-bold text-primary bg-primary/10 px-2 py-0.5 rounded-full">
                    -{calculateDiscount(book.price, book.priceSale!)}%
                  </span>
                </>
              ) : (
                <span className="font-bold" itemProp="price">
                  {formatPrice(book.price)}
                </span>
              )}
              <meta itemProp="priceCurrency" content="USD" />
            </div>
          </div>
        </CardContent>
      </Card>
    </article>
  );
}
