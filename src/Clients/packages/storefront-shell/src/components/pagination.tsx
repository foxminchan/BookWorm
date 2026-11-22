"use client";

import { useMemo } from "react";

import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from "lucide-react";

import { Button } from "@workspace/ui/components/button";

const VISIBLE_PAGES = 5;

type PaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
}: Readonly<PaginationProps>) {
  const pageNumbers = useMemo(() => {
    const count = Math.min(VISIBLE_PAGES, totalPages);
    const half = Math.floor(VISIBLE_PAGES / 2);
    const start = Math.min(
      Math.max(1, currentPage - half),
      Math.max(1, totalPages - VISIBLE_PAGES + 1),
    );

    return Array.from({ length: count }, (_, i) => start + i);
  }, [currentPage, totalPages]);

  if (totalPages <= 1) return null;

  return (
    <nav
      className="flex items-center justify-center gap-3"
      aria-label="Pagination navigation"
    >
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

      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="rounded-full"
        aria-label="Go to previous page"
      >
        <ChevronLeft className="size-4" aria-hidden="true" />
      </Button>

      <div className="flex items-center gap-2">
        {pageNumbers.map((page) => (
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

      <Button
        variant="outline"
        size="icon"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="rounded-full"
        aria-label="Go to next page"
      >
        <ChevronRight className="size-4" aria-hidden="true" />
      </Button>

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
