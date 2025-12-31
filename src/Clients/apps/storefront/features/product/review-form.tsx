"use client";

import { Star, Send, Loader2 } from "lucide-react";
import { Button } from "@workspace/ui/components/button";
import cn from "classnames";

type ReviewFormProps = {
  firstName: string;
  lastName: string;
  comment: string;
  rating: number;
  isSubmitting: boolean;
  onChange: (field: string, value: string | number) => void;
  onSubmit: () => void;
};

export function ReviewForm({
  firstName,
  lastName,
  comment,
  rating,
  isSubmitting,
  onChange,
  onSubmit,
}: ReviewFormProps) {
  return (
    <div className="bg-background border border-border p-6 rounded-2xl shadow-sm space-y-4">
      <h3 className="font-serif font-medium text-lg">Your Review</h3>
      <div className="space-y-3">
        <div className="flex gap-1">
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onClick={() => onChange("rating", star)}
              className="focus:outline-none transition-transform active:scale-90"
            >
              <Star
                className={cn(
                  "size-6",
                  star <= rating
                    ? "fill-primary text-primary"
                    : "text-muted-foreground/30",
                )}
              />
            </button>
          ))}
        </div>
        <div className="grid grid-cols-2 gap-3">
          <input
            type="text"
            placeholder="First Name"
            className="bg-secondary/50 border-none rounded-lg px-4 py-2 text-sm focus:ring-1 focus:ring-primary outline-none"
            value={firstName}
            onChange={(e) => onChange("firstName", e.target.value)}
          />
          <input
            type="text"
            placeholder="Last Name"
            className="bg-secondary/50 border-none rounded-lg px-4 py-2 text-sm focus:ring-1 focus:ring-primary outline-none"
            value={lastName}
            onChange={(e) => onChange("lastName", e.target.value)}
          />
        </div>
        <textarea
          placeholder="Share your thoughts..."
          rows={4}
          className="w-full bg-secondary/50 border-none rounded-lg px-4 py-3 text-sm focus:ring-1 focus:ring-primary outline-none resize-none"
          value={comment}
          onChange={(e) => onChange("comment", e.target.value)}
        />
        <Button
          className="w-full rounded-full gap-2"
          onClick={onSubmit}
          disabled={isSubmitting || rating === 0}
        >
          {isSubmitting ? (
            <>
              <Loader2 className="size-4 animate-spin" />
              Submitting...
            </>
          ) : (
            <>
              <Send className="size-4" /> Submit Review
            </>
          )}
        </Button>
      </div>
    </div>
  );
}
