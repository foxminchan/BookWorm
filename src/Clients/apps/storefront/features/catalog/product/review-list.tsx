"use client";

import cn from "classnames";
import { Star, User } from "lucide-react";

import type { Feedback } from "@workspace/types/rating";
import { Separator } from "@workspace/ui/components/separator";

import { Pagination } from "@/components/pagination";

type ReviewListProps = {
  reviews: Feedback[];
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export default function ReviewList({
  reviews,
  currentPage,
  totalPages,
  onPageChange,
}: Readonly<ReviewListProps>) {
  return (
    <div className="space-y-8 lg:col-span-2">
      {reviews.map((feedback) => (
        <div key={feedback.id} className="group">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-secondary flex size-10 items-center justify-center rounded-full">
                <User
                  className="text-muted-foreground size-5"
                  aria-hidden="true"
                />
              </div>
              <div>
                <p className="font-medium">
                  {feedback.firstName ?? ""} {feedback.lastName ?? ""}
                </p>
                <div className="flex gap-0.5">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <Star
                      key={star}
                      className={cn(
                        "size-3",
                        star <= feedback.rating
                          ? "fill-primary text-primary"
                          : "text-muted-foreground/30",
                      )}
                      aria-hidden="true"
                    />
                  ))}
                </div>
              </div>
            </div>
          </div>
          <p className="text-muted-foreground pl-13 leading-relaxed">
            {feedback.comment}
          </p>
          <Separator className="mt-8 group-last:hidden" aria-hidden="true" />
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
