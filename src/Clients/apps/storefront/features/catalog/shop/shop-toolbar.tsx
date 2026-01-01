"use client";

import { Button } from "@workspace/ui/components/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Filter, X } from "lucide-react";
import { Badge } from "@workspace/ui/components/badge";

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
      className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8"
      role="region"
      aria-labelledby="shop-title"
    >
      <div>
        <h1 id="shop-title" className="text-3xl font-serif font-medium">
          Shop All Books
        </h1>
        {searchQuery ? (
          <div className="flex items-center gap-2 mt-2">
            <p className="text-sm text-muted-foreground">Search results for</p>
            <Badge
              variant="secondary"
              className="font-normal flex items-center gap-1.5"
            >
              "{searchQuery}"
              <button
                onClick={onClearSearch}
                className="ml-1 hover:bg-secondary-foreground/10 rounded-full p-0.5 transition-colors"
                aria-label="Clear search"
              >
                <X className="size-3" />
              </button>
            </Badge>
            <p className="text-sm text-muted-foreground">
              ({totalCount} {totalCount === 1 ? "result" : "results"})
            </p>
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            Showing {(currentPage - 1) * ITEMS_PER_PAGE + 1}–
            {Math.min(currentPage * ITEMS_PER_PAGE, totalCount)} of {totalCount}{" "}
            results
          </p>
        )}
      </div>

      <div className="flex items-center gap-4 w-full sm:w-auto">
        <Button
          variant="outline"
          size="sm"
          className="md:hidden flex items-center gap-2 bg-transparent"
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
