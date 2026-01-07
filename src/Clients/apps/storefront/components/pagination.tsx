"use client";

import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from "lucide-react";

import { Button } from "@workspace/ui/components/button";

type PaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const getPageNumbers = () => {
    const pages: number[] = [];
    const visiblePages = 5;

    if (totalPages <= visiblePages) {
      // Show all pages if total is small
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Show 5 consecutive pages
      let start: number;

      if (currentPage <= 3) {
        // Near the beginning, show 1-5
        start = 1;
      } else if (currentPage >= totalPages - 2) {
        // Near the end, show last 5 pages
        start = totalPages - 4;
      } else {
        // In the middle, show currentPage-2 to currentPage+2
        start = currentPage - 2;
      }

      for (let i = start; i < start + visiblePages; i++) {
        pages.push(i);
      }
    }

    return pages;
  };

  return (
    <nav
      className="flex items-center justify-center gap-3"
      role="navigation"
      aria-label="Pagination navigation"
    >
      {/* First Page */}
      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(1)}
        disabled={currentPage === 1}
        className="rounded-full"
        aria-label="Go to first page"
      >
        <ChevronsLeft className="size-4" aria-hidden="true" />
      </Button>

      {/* Previous Page */}
      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(Math.max(1, currentPage - 1))}
        disabled={currentPage === 1}
        className="rounded-full"
        aria-label="Go to previous page"
      >
        <ChevronLeft className="size-4" aria-hidden="true" />
      </Button>

      {/* Page Numbers */}
      <div className="flex items-center gap-2">
        {getPageNumbers().map((page) => (
          <Button
            key={page}
            variant={currentPage === page ? "default" : "outline"}
            className="size-11 rounded-full p-0"
            onClick={() => onPageChange(page)}
            aria-label={`${currentPage === page ? "Current page," : "Go to"} page ${page} of ${totalPages}`}
            aria-current={currentPage === page ? "page" : undefined}
          >
            {page}
          </Button>
        ))}
      </div>

      {/* Next Page */}
      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
        disabled={currentPage === totalPages}
        className="rounded-full"
        aria-label="Go to next page"
      >
        <ChevronRight className="size-4" aria-hidden="true" />
      </Button>

      {/* Last Page */}
      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(totalPages)}
        disabled={currentPage === totalPages}
        className="rounded-full"
        aria-label={`Go to last page, page ${totalPages}`}
      >
        <ChevronsRight className="size-4" aria-hidden="true" />
      </Button>
    </nav>
  );
}
