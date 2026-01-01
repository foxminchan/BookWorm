"use client";

import ReviewForm from "./review-form";
import ReviewList from "./review-list";
import ReviewSummaryCard from "./review-summary-card";
import ReviewsEmptyState from "./reviews-empty-state";
import ReviewsSection from "./reviews-section";
import { ReviewsLoadingSkeleton } from "@/components/loading-skeleton";

type Review = {
  id: string;
  firstName: string;
  lastName: string;
  rating: number;
  comment: string;
};

type ReviewsContainerProps = {
  isLoading: boolean;
  reviews: Review[];
  averageRating: number;
  totalReviews: number;
  sortBy: "newest" | "highest" | "lowest";
  onSortChange: (sort: "newest" | "highest" | "lowest") => void;
  isSummarizing: boolean;
  onSummarize: () => void;
  showReviewForm: boolean;
  onToggleReviewForm: () => void;
  summary?: string;
  reviewForm: {
    firstName: string;
    lastName: string;
    comment: string;
    rating: number;
    isSubmitting: boolean;
    onChange: (field: string, value: string | number) => void;
    onSubmit: () => void;
  };
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export default function ReviewsContainer({
  isLoading,
  reviews,
  averageRating,
  totalReviews,
  sortBy,
  onSortChange,
  isSummarizing,
  onSummarize,
  showReviewForm,
  onToggleReviewForm,
  summary,
  reviewForm,
  currentPage,
  totalPages,
  onPageChange,
}: ReviewsContainerProps) {
  if (isLoading) {
    return <ReviewsLoadingSkeleton />;
  }

  if (reviews.length === 0) {
    return <ReviewsEmptyState onWriteReview={onToggleReviewForm} />;
  }

  return (
    <>
      <ReviewsSection
        sortBy={sortBy}
        onSortChange={onSortChange}
        isSummarizing={isSummarizing}
        onSummarize={onSummarize}
        showReviewForm={showReviewForm}
        onToggleReviewForm={onToggleReviewForm}
        summary={summary}
      />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">
        <div className="lg:col-span-1 space-y-8">
          <ReviewSummaryCard
            averageRating={averageRating}
            totalReviews={totalReviews}
          />

          {showReviewForm && (
            <ReviewForm
              firstName={reviewForm.firstName}
              lastName={reviewForm.lastName}
              comment={reviewForm.comment}
              rating={reviewForm.rating}
              isSubmitting={reviewForm.isSubmitting}
              onChange={reviewForm.onChange}
              onSubmit={reviewForm.onSubmit}
            />
          )}
        </div>

        <ReviewList
          reviews={reviews}
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={onPageChange}
        />
      </div>
    </>
  );
}
