"use client";

import { useCallback, useState } from "react";

import { Star, X } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Textarea } from "@workspace/ui/components/textarea";

const MAX_COMMENT_LENGTH = 2000;

export type ReviewFormWidgetProps = {
  /** Book UUID */
  bookId: string;
  /** Display title of the book (optional) */
  bookTitle?: string;
  /** Initial rating suggested by the agent (1-5) */
  initialRating?: number;
  /** Initial comment suggested by the agent */
  initialComment?: string;
  /** Called when the user confirms with the chosen rating and comment */
  onConfirm: (rating: number, comment: string) => void;
  /** Called when the user dismisses without confirming */
  onDismiss: () => void;
};

/**
 * Inline HITL review form rendered inside the chat panel.
 * Provides a star selector, comment textarea, character counter,
 * and confirm/cancel controls.
 */
export function ReviewFormWidget({
  bookId: _bookId,
  bookTitle,
  initialRating = 0,
  initialComment = "",
  onConfirm,
  onDismiss,
}: Readonly<ReviewFormWidgetProps>) {
  const [rating, setRating] = useState(Math.max(0, Math.min(5, initialRating)));
  const [hoveredStar, setHoveredStar] = useState(0);
  const [comment, setComment] = useState(initialComment);
  const [commentError, setCommentError] = useState<string | null>(null);

  const handleCommentChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      const value = e.target.value;
      setComment(value);
      if (value.length > MAX_COMMENT_LENGTH) {
        setCommentError(
          `Comment must not exceed ${MAX_COMMENT_LENGTH} characters.`,
        );
      } else {
        setCommentError(null);
      }
    },
    [],
  );

  const handleConfirm = useCallback(() => {
    if (rating < 1 || commentError) return;
    onConfirm(rating, comment.trim());
  }, [comment, commentError, onConfirm, rating]);

  const canConfirm = rating >= 1 && !commentError;
  const charsRemaining = MAX_COMMENT_LENGTH - comment.length;

  return (
    <div
      role="dialog"
      aria-modal="false"
      aria-labelledby="review-form-title"
      className="mt-2 rounded-lg border border-amber-200 bg-amber-50 p-4 dark:border-amber-800 dark:bg-amber-950"
    >
      {/* Header */}
      <div className="mb-3 flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <Star
            className="h-5 w-5 shrink-0 fill-amber-500 text-amber-500"
            aria-hidden="true"
          />
          <h3
            id="review-form-title"
            className="text-sm font-semibold text-amber-900 dark:text-amber-100"
          >
            Submit a Review
          </h3>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="h-6 w-6 text-amber-600 hover:text-amber-800 dark:text-amber-400 dark:hover:text-amber-200"
          onClick={onDismiss}
          aria-label="Dismiss review form"
        >
          <X className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>

      {/* Book title */}
      {bookTitle && (
        <p className="mb-3 text-sm font-medium text-amber-900 dark:text-amber-100">
          {bookTitle}
        </p>
      )}

      {/* Star rating */}
      <div className="mb-3">
        <span
          id="star-rating-label"
          className="mb-1 block text-xs font-medium text-amber-700 dark:text-amber-300"
        >
          Rating
          <span className="ml-1 text-red-500" aria-hidden="true">
            *
          </span>
        </span>
        <div
          role="radiogroup"
          aria-labelledby="star-rating-label"
          aria-required="true"
          className="flex gap-1"
        >
          {[1, 2, 3, 4, 5].map((star) => {
            const filled = star <= (hoveredStar || rating);
            return (
              <button
                key={star}
                type="button"
                role="radio"
                aria-checked={rating === star}
                aria-label={`${star} ${star === 1 ? "star" : "stars"}`}
                className="rounded p-0.5 focus-visible:ring-2 focus-visible:ring-amber-500 focus-visible:outline-none"
                onClick={() => setRating(star)}
                onMouseEnter={() => setHoveredStar(star)}
                onMouseLeave={() => setHoveredStar(0)}
              >
                <Star
                  className={`h-7 w-7 transition-colors ${
                    filled
                      ? "fill-amber-500 text-amber-500"
                      : "fill-transparent text-amber-300 dark:text-amber-700"
                  }`}
                  aria-hidden="true"
                />
              </button>
            );
          })}
        </div>
        {rating === 0 && (
          <p
            className="mt-1 text-xs text-amber-600 dark:text-amber-400"
            role="status"
            aria-live="polite"
          >
            Please select a star rating.
          </p>
        )}
      </div>

      {/* Comment textarea */}
      <div className="mb-3">
        <label
          htmlFor="review-comment-input"
          className="mb-1 block text-xs font-medium text-amber-700 dark:text-amber-300"
        >
          Comment{" "}
          <span className="font-normal text-amber-600 dark:text-amber-400">
            (optional)
          </span>
        </label>
        <Textarea
          id="review-comment-input"
          value={comment}
          onChange={handleCommentChange}
          placeholder="Share your thoughts about this book..."
          className="min-h-[80px] resize-none border-amber-300 bg-white text-sm dark:border-amber-700 dark:bg-amber-900"
          aria-describedby={
            commentError ? "review-comment-error" : "review-comment-counter"
          }
          maxLength={MAX_COMMENT_LENGTH + 1}
        />
        <div className="mt-1 flex justify-between">
          {commentError ? (
            <p
              id="review-comment-error"
              role="alert"
              className="text-xs text-red-600 dark:text-red-400"
            >
              {commentError}
            </p>
          ) : (
            <span />
          )}
          <span
            id="review-comment-counter"
            className={`text-xs ${charsRemaining < 100 ? "text-amber-600 dark:text-amber-400" : "text-amber-500 dark:text-amber-500"}`}
            aria-live="polite"
            aria-atomic="true"
          >
            {charsRemaining} chars remaining
          </span>
        </div>
      </div>

      {/* Preview */}
      {rating > 0 && (
        <div
          className="mb-3 rounded border border-amber-200 bg-white p-2 text-xs text-amber-800 dark:border-amber-700 dark:bg-amber-900 dark:text-amber-200"
          aria-label="Review preview"
        >
          <span className="font-semibold">Preview: </span>
          {rating}-star review
          {comment.trim() &&
            ` — "${comment.trim().slice(0, 60)}${comment.trim().length > 60 ? "…" : ""}"`}
        </div>
      )}

      {/* Action buttons */}
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          className="flex-1 border-amber-300 text-amber-700 hover:bg-amber-100 dark:border-amber-700 dark:text-amber-300 dark:hover:bg-amber-900"
          onClick={onDismiss}
        >
          Cancel
        </Button>
        <Button
          size="sm"
          className="flex-1 bg-amber-600 hover:bg-amber-700 dark:bg-amber-500 dark:hover:bg-amber-600"
          onClick={handleConfirm}
          disabled={!canConfirm}
        >
          Submit Review
        </Button>
      </div>
    </div>
  );
}
