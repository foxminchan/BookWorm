"use client";

import type React from "react";
import { ProductImage } from "./product-image";
import { ProductInfo } from "./product-info";
import { ProductActions } from "./product-actions";

type ProductSectionProps = {
  book: {
    imageUrl?: string | null;
    name: string;
    priceSale?: number | null;
    category?: { name: string | null } | null;
    authors: Array<{ name: string | null }>;
    averageRating: number;
    totalReviews: number;
    price: number;
    status: string;
    description?: string | null;
    publisher?: { name: string | null } | null;
  };
  quantity: number;
  isAddingToBasket: boolean;
  onAddToBasket: () => void;
  onQuantityChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onDecrease: () => void;
  onIncrease: () => void;
};

export function ProductSection({
  book,
  quantity,
  isAddingToBasket,
  onAddToBasket,
  onQuantityChange,
  onDecrease,
  onIncrease,
}: ProductSectionProps) {
  return (
    <article
      className="grid grid-cols-1 md:grid-cols-2 gap-12 lg:gap-20 mb-16"
      itemScope
      itemType="https://schema.org/Book"
    >
      <ProductImage
        imageUrl={book.imageUrl ?? undefined}
        name={book.name}
        hasSale={!!book.priceSale}
      />

      <div className="flex flex-col">
        <ProductInfo
          category={book.category?.name ?? undefined}
          name={book.name}
          authors={book.authors.map((a) => ({ name: a.name || "" }))}
          averageRating={book.averageRating}
          totalReviews={book.totalReviews}
          price={book.price}
          priceSale={book.priceSale ?? undefined}
          status={book.status}
          description={book.description ?? undefined}
          publisher={book.publisher?.name ?? undefined}
        />

        <ProductActions
          quantity={quantity}
          status={book.status}
          isAddingToBasket={isAddingToBasket}
          onAddToBasket={onAddToBasket}
          onQuantityChange={onQuantityChange}
          onDecrease={onDecrease}
          onIncrease={onIncrease}
        />
      </div>
    </article>
  );
}
