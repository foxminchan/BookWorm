"use client";

import { Filter, X } from "lucide-react";

import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";

const ITEMS_PER_PAGE = 8;

type ShopToolbarProps = {
  searchQuery: string;
  onClearSearch: () => void;
  totalCount: number;
  currentPage: number;
  sortBy: string;
  onSortChange: (value: string) => void;
  onOpenFilters: () => void;
};

export default function ShopToolbar({
  searchQuery,
  onClearSearch,
  totalCount,
  currentPage,
  sortBy,
  onSortChange,
  onOpenFilters,
}: ShopToolbarProps) {
  return (
    <div
      className="mb-8 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center"
      role="region"
      aria-labelledby="shop-title"
    >
      <div>
        <h1 id="shop-title" className="font-serif text-3xl font-medium">
          Shop All Books
        </h1>
        {searchQuery ? (
          <div className="mt-2 flex items-center gap-2">
            <p className="text-muted-foreground text-sm">Search results for</p>
            <Badge
              variant="secondary"
              className="flex items-center gap-1.5 font-normal"
            >
              "{searchQuery}"
              <Button
                variant="ghost"
                size="icon"
                onClick={onClearSearch}
                className="hover:bg-secondary-foreground/10 ml-1 h-5 w-5 rounded-full p-0"
                aria-label="Clear search"
              >
                <X className="size-3" />
              </Button>
            </Badge>
            <p className="text-muted-foreground text-sm">
              ({totalCount} {totalCount === 1 ? "result" : "results"})
            </p>
          </div>
        ) : (
          <p className="text-muted-foreground text-sm">
            Showing {(currentPage - 1) * ITEMS_PER_PAGE + 1}–
            {Math.min(currentPage * ITEMS_PER_PAGE, totalCount)} of {totalCount}{" "}
            results
          </p>
        )}
      </div>

      <div className="flex w-full items-center gap-4 sm:w-auto">
        <Button
          variant="outline"
          size="sm"
          className="flex items-center gap-2 bg-transparent md:hidden"
          onClick={onOpenFilters}
          aria-label="Open filters"
        >
          <Filter className="size-4" /> Filters
        </Button>
        <Select value={sortBy} onValueChange={onSortChange}>
          <SelectTrigger
            className="w-45 rounded-full"
            aria-label="Sort options"
          >
            <SelectValue placeholder="Sort by" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="name">Name (A-Z)</SelectItem>
            <SelectItem value="price-low">Price (Low to High)</SelectItem>
            <SelectItem value="price-high">Price (High to Low)</SelectItem>
            <SelectItem value="rating">Rating</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
