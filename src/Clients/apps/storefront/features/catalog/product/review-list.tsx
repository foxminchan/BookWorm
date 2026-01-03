"use client";

import cn from "classnames";
import { Star, User } from "lucide-react";

import { Separator } from "@workspace/ui/components/separator";

import { Pagination } from "@/components/pagination";

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

export default function ReviewList({
  reviews,
  currentPage,
  totalPages,
  onPageChange,
}: ReviewListProps) {
  return (
    <div className="space-y-8 lg:col-span-2">
      {reviews.map((feedback) => (
        <div key={feedback.id} className="group">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-secondary flex size-10 items-center justify-center rounded-full">
                <User className="text-muted-foreground size-5" />
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
          <p className="text-muted-foreground pl-13 leading-relaxed">
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
