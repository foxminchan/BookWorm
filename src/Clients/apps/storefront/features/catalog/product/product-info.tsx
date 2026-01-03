"use client";

import cn from "classnames";
import { Star } from "lucide-react";

import { Separator } from "@workspace/ui/components/separator";

type ProductInfoProps = {
  category?: string;
  name: string;
  authors: Array<{ name: string }>;
  averageRating: number;
  totalReviews: number;
  price: number;
  priceSale?: number;
  status: string;
  description?: string;
  publisher?: string;
};

export default function ProductInfo({
  category,
  name,
  authors,
  averageRating,
  totalReviews,
  price,
  priceSale,
  status,
  description,
  publisher,
}: ProductInfoProps) {
  return (
    <div className="flex flex-col">
      <div className="mb-6">
        <p className="text-primary mb-2 text-sm font-bold tracking-widest uppercase">
          {category}
        </p>
        <h1
          className="mb-4 font-serif text-4xl leading-tight font-medium md:text-5xl"
          itemProp="name"
        >
          {name}
        </h1>
        <p className="text-muted-foreground text-xl">
          by{" "}
          <span itemProp="author">{authors.map((a) => a.name).join(", ")}</span>
        </p>
      </div>

      <div className="mb-8 flex items-center gap-4">
        <div className="flex items-center gap-1">
          {[...Array(5)].map((_, i) => (
            <Star
              key={i}
              className={cn(
                "size-5",
                i < Math.floor(averageRating)
                  ? "fill-primary text-primary"
                  : "text-muted-foreground/30",
              )}
              aria-hidden="true"
            />
          ))}
          <span
            className="ml-2 font-medium"
            itemProp="aggregateRating"
            itemScope
            itemType="https://schema.org/AggregateRating"
          >
            <meta itemProp="ratingValue" content={String(averageRating)} />
            <meta itemProp="reviewCount" content={String(totalReviews)} />
            {averageRating.toFixed(1)}
          </span>
        </div>
        <Separator orientation="vertical" className="h-4" aria-hidden="true" />
        <span className="text-muted-foreground text-sm">
          {totalReviews} Reviews
        </span>
      </div>

      <div className="mb-8">
        <div
          className="mb-2 flex items-baseline gap-3"
          itemProp="offers"
          itemScope
          itemType="https://schema.org/Offer"
        >
          {priceSale ? (
            <>
              <span
                className="text-primary text-3xl font-bold"
                itemProp="price"
              >
                ${priceSale.toFixed(2)}
              </span>
              <span className="text-muted-foreground text-xl line-through">
                ${price.toFixed(2)}
              </span>
            </>
          ) : (
            <span className="text-3xl font-bold" itemProp="price">
              ${price.toFixed(2)}
            </span>
          )}
          <meta itemProp="priceCurrency" content="USD" />
          <meta
            itemProp="availability"
            content={status === "InStock" ? "InStock" : "OutOfStock"}
          />
        </div>
        <p
          className={cn(
            "text-sm font-medium",
            status === "InStock" ? "text-emerald-600" : "text-destructive",
          )}
        >
          {status === "InStock" ? "Available in Store" : "Out of Stock"}
        </p>
      </div>

      <div className="mb-10 space-y-6">
        <p
          className="text-muted-foreground text-lg leading-relaxed"
          itemProp="description"
        >
          {description}
        </p>

        <div className="grid grid-cols-2 gap-y-4 text-sm">
          <div>
            <p className="text-muted-foreground text-[10px] font-bold tracking-wider uppercase">
              Publisher
            </p>
            <p className="font-medium" itemProp="publisher">
              {publisher}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground text-[10px] font-bold tracking-wider uppercase">
              Category
            </p>
            <p className="font-medium" itemProp="bookEdition">
              {category}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
