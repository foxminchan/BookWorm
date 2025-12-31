"use client";

import { Star, User } from "lucide-react";
import { Separator } from "@workspace/ui/components/separator";
import { Pagination } from "@/components/pagination";
import cn from "classnames";

type Review = {
  id: string;
  firstName: string;
  lastName: string;
  rating: number;
  comment: string;
};

type ReviewListProps = {
  reviews: Review[];
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function ReviewList({
  reviews,
  currentPage,
  totalPages,
  onPageChange,
}: ReviewListProps) {
  return (
    <div className="lg:col-span-2 space-y-8">
      {reviews.map((feedback) => (
        <div key={feedback.id} className="group">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="size-10 rounded-full bg-secondary flex items-center justify-center">
                <User className="size-5 text-muted-foreground" />
              </div>
              <div>
                <p className="font-medium">
                  {feedback.firstName} {feedback.lastName}
                </p>
                <div className="flex gap-0.5">
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={cn(
                        "size-3",
                        i < feedback.rating
                          ? "fill-primary text-primary"
                          : "text-muted-foreground/30",
                      )}
                    />
                  ))}
                </div>
              </div>
            </div>
          </div>
          <p className="text-muted-foreground leading-relaxed pl-13">
            {feedback.comment}
          </p>
          <Separator className="mt-8 group-last:hidden" />
        </div>
      ))}

      {totalPages > 1 && (
        <div className="pt-8">
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={onPageChange}
          />
        </div>
      )}
    </div>
  );
}
