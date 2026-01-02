"use client";

import cn from "classnames";
import { Star } from "lucide-react";

type ReviewSummaryCardProps = {
  averageRating: number;
  totalReviews: number;
};

export default function ReviewSummaryCard({
  averageRating,
  totalReviews,
}: ReviewSummaryCardProps) {
  return (
    <div className="bg-secondary/30 rounded-2xl p-8 text-center">
      <p className="mb-2 font-serif text-5xl font-bold">
        {averageRating.toFixed(1)}
      </p>
      <div className="mb-2 flex justify-center gap-1">
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
      <p className="text-muted-foreground text-sm">
        Based on {totalReviews} reviews
      </p>
    </div>
  );
}
