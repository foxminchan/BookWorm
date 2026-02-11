"use client";

import { MessageSquare } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

type ReviewsEmptyStateProps = {
  onWriteReview: () => void;
};

export default function ReviewsEmptyState({
  onWriteReview,
}: Readonly<ReviewsEmptyStateProps>) {
  return (
    <div className="space-y-4 py-16 text-center">
      <MessageSquare className="text-muted-foreground/50 mx-auto size-12" />
      <div>
        <h3 className="mb-2 text-lg font-medium">No reviews yet</h3>
        <p className="text-muted-foreground mb-6">
          Be the first to share your thoughts about this book
        </p>
        <Button
          type="button"
          className="rounded-full"
          onClick={onWriteReview}
          aria-label="Write the first review for this book"
        >
          <MessageSquare className="mr-2 size-4" aria-hidden="true" /> Write
          First Review
        </Button>
      </div>
    </div>
  );
}
