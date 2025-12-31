"use client";

import { Star } from "lucide-react";
import cn from "classnames";

type ReviewSummaryCardProps = {
  averageRating: number;
  totalReviews: number;
};

export function ReviewSummaryCard({
  averageRating,
  totalReviews,
}: ReviewSummaryCardProps) {
  return (
    <div className="bg-secondary/30 p-8 rounded-2xl text-center">
      <p className="text-5xl font-serif font-bold mb-2">
        {averageRating.toFixed(1)}
      </p>
      <div className="flex justify-center gap-1 mb-2">
        {[...Array(5)].map((_, i) => (
          <Star
            key={i}
            className={cn(
              "size-4",
              i < Math.floor(averageRating)
                ? "fill-primary text-primary"
                : "text-muted-foreground/30",
            )}
          />
        ))}
      </div>
      <p className="text-sm text-muted-foreground">
        Based on {totalReviews} reviews
      </p>
    </div>
  );
}
