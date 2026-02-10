"use client";

import type { Author } from "@workspace/types/catalog/authors";
import type { Category } from "@workspace/types/catalog/categories";
import type { Publisher } from "@workspace/types/catalog/publishers";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@workspace/ui/components/sheet";
import { Slider } from "@workspace/ui/components/slider";

import { FilterSection } from "@/components/filter-section";
import { currencyFormatter } from "@/lib/constants";

type FilterItem = { id: string; name: string };

type ShopFiltersProps = {
  priceRange: number[];
  setPriceRange: (range: number[]) => void;
  categories: Category[] | undefined;
  selectedCategories: string[];
  onToggleCategory: (id: string) => void;
  publishers: Publisher[] | undefined;
  selectedPublishers: string[];
  onTogglePublisher: (id: string) => void;
  authors: Author[] | undefined;
  selectedAuthors: string[];
  onToggleAuthor: (id: string) => void;
  isFilterOpen: boolean;
  setIsFilterOpen: (value: boolean) => void;
};

function toFilterItems(
  items: Array<{ id: string; name: string | null }> | undefined,
): FilterItem[] {
  return (items ?? []).map((item) => ({
    id: item.id,
    name: item.name ?? "",
  }));
}

function PriceRangeFilter({
  priceRange,
  setPriceRange,
}: Readonly<{
  priceRange: number[];
  setPriceRange: (range: number[]) => void;
}>) {
  return (
    <div>
      <h3 className="mb-2 font-serif font-semibold">Price Range</h3>
      <Slider
        value={priceRange}
        onValueChange={(value) =>
          setPriceRange([value[0] ?? 0, value[1] ?? 100])
        }
        max={100}
        step={1}
        className="mb-2"
      />
      <div className="text-muted-foreground flex items-center justify-between text-sm">
        <span>{currencyFormatter.format(priceRange[0] ?? 0)}</span>
        <span>{currencyFormatter.format(priceRange[1] ?? 100)}+</span>
      </div>
    </div>
  );
}

export default function ShopFilters({
  priceRange,
  setPriceRange,
  categories,
  selectedCategories,
  onToggleCategory,
  publishers,
  selectedPublishers,
  onTogglePublisher,
  authors,
  selectedAuthors,
  onToggleAuthor,
  isFilterOpen,
  setIsFilterOpen,
}: Readonly<ShopFiltersProps>) {
  const categoryItems = toFilterItems(categories);
  const publisherItems = toFilterItems(publishers);
  const authorItems = toFilterItems(authors);

  return (
    <>
      {/* Desktop Sidebar Filters */}
      <aside
        className="hidden w-64 shrink-0 space-y-6 md:block"
        aria-label="Filters"
      >
        <PriceRangeFilter
          priceRange={priceRange}
          setPriceRange={setPriceRange}
        />

        <FilterSection
          title="Category"
          items={categoryItems}
          selectedItems={selectedCategories}
          onToggle={onToggleCategory}
        />

        <FilterSection
          title="Publisher"
          items={publisherItems}
          selectedItems={selectedPublishers}
          onToggle={onTogglePublisher}
        />

        <FilterSection
          title="Author"
          items={authorItems}
          selectedItems={selectedAuthors}
          onToggle={onToggleAuthor}
        />
      </aside>

      {/* Mobile Filter Sheet */}
      <Sheet open={isFilterOpen} onOpenChange={setIsFilterOpen}>
        <SheetContent side="left" className="overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Filters</SheetTitle>
          </SheetHeader>

          <div className="mt-8 space-y-6 px-4">
            <PriceRangeFilter
              priceRange={priceRange}
              setPriceRange={setPriceRange}
            />

            <FilterSection
              title="Category"
              items={categoryItems}
              selectedItems={selectedCategories}
              onToggle={onToggleCategory}
            />

            <FilterSection
              title="Publisher"
              items={publisherItems}
              selectedItems={selectedPublishers}
              onToggle={onTogglePublisher}
            />

            <FilterSection
              title="Author"
              items={authorItems}
              selectedItems={selectedAuthors}
              onToggle={onToggleAuthor}
            />
          </div>
        </SheetContent>
      </Sheet>
    </>
  );
}
