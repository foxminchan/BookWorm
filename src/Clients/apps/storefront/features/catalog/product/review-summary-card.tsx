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
}: Readonly<ReviewSummaryCardProps>) {
  return (
    <div className="bg-secondary/30 rounded-2xl p-8 text-center">
      <p className="mb-2 font-serif text-5xl font-bold">
        {averageRating.toFixed(1)}
      </p>
      <div className="mb-2 flex justify-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <Star
            key={star}
            className={cn(
              "size-4",
              star <= Math.floor(averageRating)
                ? "fill-primary text-primary"
                : "text-muted-foreground/30",
            )}
            aria-hidden="true"
          />
        ))}
      </div>
      <p className="text-muted-foreground text-sm">
        Based on {totalReviews} reviews
      </p>
    </div>
  );
}
