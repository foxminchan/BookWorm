"use client";

import { Button } from "@workspace/ui/components/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@workspace/ui/components/dropdown-menu";
import { MessageSquare, Sparkles, Loader2, ChevronDown } from "lucide-react";

type ReviewsSectionProps = {
  sortBy: "newest" | "highest" | "lowest";
  onSortChange: (sort: "newest" | "highest" | "lowest") => void;
  isSummarizing: boolean;
  onSummarize: () => void;
  showReviewForm: boolean;
  onToggleReviewForm: () => void;
  summary?: string;
};

export default function ReviewsSection({
  sortBy,
  onSortChange,
  isSummarizing,
  onSummarize,
  showReviewForm,
  onToggleReviewForm,
  summary,
}: ReviewsSectionProps) {
  return (
    <>
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-12">
        <header>
          <h2 className="text-3xl font-serif font-medium mb-2">
            Customer Feedback
          </h2>
          <p className="text-muted-foreground">
            What our readers are saying about this book
          </p>
        </header>
        <div className="flex flex-wrap gap-3">
          {/* Sorting Dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="outline"
                className="rounded-full gap-2 px-6 bg-transparent"
                aria-label="Sort reviews by"
              >
                Sort by: {sortBy.charAt(0).toUpperCase() + sortBy.slice(1)}{" "}
                <ChevronDown className="size-4" aria-hidden="true" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="rounded-xl">
              <DropdownMenuItem onClick={() => onSortChange("newest")}>
                Newest
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => onSortChange("highest")}>
                Highest Rating
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => onSortChange("lowest")}>
                Lowest Rating
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          <Button
            variant="outline"
            className="rounded-full gap-2 px-6 border-primary/20 text-primary hover:bg-primary/5 transition-all bg-transparent"
            onClick={onSummarize}
            disabled={isSummarizing}
            aria-label="Summarize reviews"
          >
            {isSummarizing ? (
              <Loader2 className="size-4 animate-spin" aria-hidden="true" />
            ) : (
              <Sparkles className="size-4 text-primary" aria-hidden="true" />
            )}
            {isSummarizing ? "Summarizing..." : "Summarize Reviews"}
          </Button>
          <Button
            variant={showReviewForm ? "ghost" : "outline"}
            className="rounded-full gap-2 px-6 bg-transparent"
            onClick={onToggleReviewForm}
            aria-label={
              showReviewForm ? "Cancel writing a review" : "Write a review"
            }
          >
            <MessageSquare className="size-4" aria-hidden="true" />{" "}
            {showReviewForm ? "Cancel Review" : "Write a Review"}
          </Button>
        </div>
      </div>

      {summary && (
        <aside className="mb-12 bg-primary/5 border border-primary/10 rounded-2xl p-8 animate-in fade-in slide-in-from-top-4 duration-500">
          <div className="flex items-center gap-2 mb-4 text-primary">
            <Sparkles className="size-5" aria-hidden="true" />
            <h3 className="font-serif font-bold tracking-tight">
              AI Review Summary
            </h3>
          </div>
          <p className="text-lg leading-relaxed text-foreground/80 italic font-serif">
            "{summary}"
          </p>
        </aside>
      )}
    </>
  );
}
