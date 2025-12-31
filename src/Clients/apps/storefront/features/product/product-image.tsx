"use client";

import { useState } from "react";
import Image from "next/image";
import { Badge } from "@workspace/ui/components/badge";

type ProductImageProps = {
  imageUrl?: string;
  name: string;
  hasSale?: boolean;
};

export function ProductImage({ imageUrl, name, hasSale }: ProductImageProps) {
  const [imgError, setImgError] = useState(false);

  return (
    <figure className="aspect-3/4 bg-secondary rounded-2xl overflow-hidden shadow-sm relative group">
      {hasSale && (
        <Badge className="absolute top-6 left-6 z-10 bg-destructive text-destructive-foreground font-bold px-3 py-1">
          SALE
        </Badge>
      )}
      <Image
        src={imgError || !imageUrl ? "/book-placeholder.svg" : imageUrl}
        alt={`${name} book cover`}
        fill
        className="object-cover transition-transform duration-700 group-hover:scale-105"
        itemProp="image"
        onError={() => setImgError(true)}
        sizes="(max-width: 768px) 100vw, 50vw"
        priority
      />
    </figure>
  );
}
