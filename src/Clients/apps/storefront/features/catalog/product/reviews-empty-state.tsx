"use client";

import { MessageSquare } from "lucide-react";
import { Button } from "@workspace/ui/components/button";

type ReviewsEmptyStateProps = {
  onWriteReview: () => void;
};

export default function ReviewsEmptyState({
  onWriteReview,
}: ReviewsEmptyStateProps) {
  return (
    <div className="text-center py-16 space-y-4">
      <MessageSquare className="size-12 text-muted-foreground/50 mx-auto" />
      <div>
        <h3 className="text-lg font-medium mb-2">No reviews yet</h3>
        <p className="text-muted-foreground mb-6">
          Be the first to share your thoughts about this book
        </p>
        <Button className="rounded-full" onClick={onWriteReview}>
          <MessageSquare className="size-4 mr-2" /> Write First Review
        </Button>
      </div>
    </div>
  );
}
