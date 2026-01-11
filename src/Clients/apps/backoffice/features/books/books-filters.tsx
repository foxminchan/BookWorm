"use client";

import { useCallback, useState } from "react";

import { ChevronDown, ChevronUp } from "lucide-react";
import { useDebounceCallback } from "usehooks-ts";

import type { ListBooksQuery } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { PRICE_RANGE } from "@/lib/constants";

import { AuthorsFilter } from "./filters/authors-filter";
import { CategorySelect } from "./filters/category-select";
import { PriceRangeFilter } from "./filters/price-range-filter";
import { PublisherSelect } from "./filters/publisher-select";
import { SearchInput } from "./filters/search-input";

type BooksFiltersProps = {
  onFiltersChange: (
    query: Omit<ListBooksQuery, "pageIndex" | "pageSize">,
  ) => void;
};

export function BooksFilters({ onFiltersChange }: BooksFiltersProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [priceRange, setPriceRange] = useState<[number, number]>([
    PRICE_RANGE.min,
    PRICE_RANGE.max,
  ]);
  const [debouncedPriceRange, setDebouncedPriceRange] = useState<
    [number, number]
  >([PRICE_RANGE.min, PRICE_RANGE.max]);
  const [orderBy, setOrderBy] = useState<string>("name");
  const [isDescending, setIsDescending] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(
    undefined,
  );
  const [selectedAuthors, setSelectedAuthors] = useState<string[]>([]);
  const [debouncedAuthors, setDebouncedAuthors] = useState<string[]>([]);
  const [selectedPublisher, setSelectedPublisher] = useState<
    string | undefined
  >(undefined);

  const updateQuery = useCallback(() => {
    const query: Omit<ListBooksQuery, "pageIndex" | "pageSize"> = {
      search: debouncedSearch || undefined,
      minPrice:
        debouncedPriceRange[0] !== PRICE_RANGE.min
          ? debouncedPriceRange[0]
          : undefined,
      maxPrice:
        debouncedPriceRange[1] !== PRICE_RANGE.max
          ? debouncedPriceRange[1]
          : undefined,
      orderBy,
      isDescending,
      categoryId: selectedCategory,
      authorId: debouncedAuthors.length > 0 ? debouncedAuthors[0] : undefined,
      publisherId: selectedPublisher || undefined,
    };
    onFiltersChange(query);
  }, [
    debouncedSearch,
    debouncedPriceRange,
    orderBy,
    isDescending,
    selectedCategory,
    debouncedAuthors,
    selectedPublisher,
    onFiltersChange,
  ]);

  const handleSearchChange = useDebounceCallback(
    (term: string) => {
      setDebouncedSearch(term);
      updateQuery();
    },
    500,
    { leading: false, trailing: true },
  );

  const handleDebouncedPriceChange = useDebounceCallback(
    (min: number, max: number) => {
      setDebouncedPriceRange([min, max]);
      updateQuery();
    },
    500,
    { leading: false, trailing: true },
  );

  const handleDebouncedAuthorChange = useDebounceCallback(
    (authors: string[]) => {
      setDebouncedAuthors(authors);
      updateQuery();
    },
    500,
    { leading: false, trailing: true },
  );

  const handleAuthorToggle = (authorId: string) => {
    const newAuthors = selectedAuthors.includes(authorId)
      ? selectedAuthors.filter((id) => id !== authorId)
      : [...selectedAuthors, authorId];
    setSelectedAuthors(newAuthors);
    handleDebouncedAuthorChange(newAuthors);
  };

  const handlePriceRangeChange = (newMin: number, newMax: number) => {
    setPriceRange([newMin, newMax]);
    handleDebouncedPriceChange(newMin, newMax);
  };

  const handleResetFilters = () => {
    setSearchTerm("");
    setDebouncedSearch("");
    setPriceRange([PRICE_RANGE.min, PRICE_RANGE.max]);
    setDebouncedPriceRange([PRICE_RANGE.min, PRICE_RANGE.max]);
    setOrderBy("name");
    setIsDescending(false);
    setSelectedCategory(undefined);
    setSelectedAuthors([]);
    setDebouncedAuthors([]);
    setSelectedPublisher("");
    updateQuery();
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Filter & Search</CardTitle>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsExpanded(!isExpanded)}
            className="h-8 w-8 p-0"
          >
            {isExpanded ? (
              <ChevronUp className="h-4 w-4" />
            ) : (
              <ChevronDown className="h-4 w-4" />
            )}
            <span className="sr-only">
              {isExpanded ? "Collapse" : "Expand"} filters
            </span>
          </Button>
        </div>
      </CardHeader>
      {isExpanded && (
        <CardContent>
          <div className="space-y-4">
            <SearchInput
              value={searchTerm}
              onChange={(value) => {
                setSearchTerm(value);
                handleSearchChange(value);
              }}
            />

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <CategorySelect
                value={selectedCategory}
                onChange={setSelectedCategory}
              />
              <PublisherSelect
                value={selectedPublisher}
                onChange={setSelectedPublisher}
              />
            </div>

            <AuthorsFilter
              selectedAuthors={selectedAuthors}
              onToggle={handleAuthorToggle}
              onClear={() => {
                setSelectedAuthors([]);
                setDebouncedAuthors([]);
                updateQuery();
              }}
            />

            <PriceRangeFilter
              min={PRICE_RANGE.min}
              max={PRICE_RANGE.max}
              minPrice={priceRange[0]}
              maxPrice={priceRange[1]}
              onChange={handlePriceRangeChange}
            />

            <Button
              variant="outline"
              size="sm"
              onClick={handleResetFilters}
              className="w-full bg-transparent"
            >
              Reset Filters
            </Button>
          </div>
        </CardContent>
      )}
    </Card>
  );
}
