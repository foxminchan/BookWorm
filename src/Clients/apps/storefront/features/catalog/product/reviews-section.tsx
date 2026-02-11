"use client";

import { ChevronDown, Loader2, MessageSquare, Sparkles } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@workspace/ui/components/dropdown-menu";

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
}: Readonly<ReviewsSectionProps>) {
  return (
    <>
      <div className="mb-12 flex flex-col items-start justify-between gap-6 md:flex-row md:items-center">
        <header>
          <h2 className="mb-2 font-serif text-3xl font-medium">
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
                className="gap-2 rounded-full bg-transparent px-6"
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
            className="border-primary/20 text-primary hover:bg-primary/5 gap-2 rounded-full bg-transparent px-6 transition-all"
            onClick={onSummarize}
            disabled={isSummarizing}
            aria-label="Summarize reviews"
          >
            {isSummarizing ? (
              <Loader2 className="size-4 animate-spin" aria-hidden="true" />
            ) : (
              <Sparkles className="text-primary size-4" aria-hidden="true" />
            )}
            {isSummarizing ? "Summarizing..." : "Summarize Reviews"}
          </Button>
          <Button
            variant={showReviewForm ? "ghost" : "outline"}
            className="gap-2 rounded-full bg-transparent px-6"
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
        <aside className="bg-primary/5 border-primary/10 animate-in fade-in slide-in-from-top-4 mb-12 rounded-2xl border p-8 duration-500">
          <div className="text-primary mb-4 flex items-center gap-2">
            <Sparkles className="size-5" aria-hidden="true" />
            <h3 className="font-serif font-bold tracking-tight">
              AI Review Summary
            </h3>
          </div>
          <p className="text-foreground/80 font-serif text-lg leading-relaxed italic">
            "{summary}"
          </p>
        </aside>
      )}
    </>
  );
}
