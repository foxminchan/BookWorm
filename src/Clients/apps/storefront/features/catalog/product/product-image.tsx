"use client";

import { useState } from "react";

import Image from "next/image";

import { Badge } from "@workspace/ui/components/badge";

import { DEFAULT_BOOK_IMAGE } from "@/lib/constants";

type ProductImageProps = {
  imageUrl?: string;
  name: string;
  hasSale?: boolean;
};

export default function ProductImage({
  imageUrl,
  name,
  hasSale,
}: ProductImageProps) {
  const [imgError, setImgError] = useState(false);

  return (
    <figure className="bg-secondary group relative aspect-3/4 overflow-hidden rounded-2xl shadow-sm">
      {hasSale && (
        <Badge className="bg-destructive text-destructive-foreground absolute top-6 left-6 z-10 px-3 py-1 font-bold">
          SALE
        </Badge>
      )}
      <Image
        src={imgError || !imageUrl ? DEFAULT_BOOK_IMAGE : imageUrl}
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
