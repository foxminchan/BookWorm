"use client";

import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@workspace/ui/components/sheet";
import { Slider } from "@workspace/ui/components/slider";

import { FilterSection } from "@/components/filter-section";

type ShopFiltersProps = {
  priceRange: number[];
  setPriceRange: (range: number[]) => void;
  categories: any[] | undefined;
  selectedCategories: string[];
  onToggleCategory: (id: string) => void;
  publishers: any[] | undefined;
  selectedPublishers: string[];
  onTogglePublisher: (id: string) => void;
  authors: any[] | undefined;
  selectedAuthors: string[];
  onToggleAuthor: (id: string) => void;
  isFilterOpen: boolean;
  setIsFilterOpen: (value: boolean) => void;
};

function PriceRangeFilter({
  priceRange,
  setPriceRange,
}: {
  priceRange: number[];
  setPriceRange: (range: number[]) => void;
}) {
  return (
    <div>
      <h3 className="mb-4 font-serif font-medium">Price Range</h3>
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
        <span>${priceRange[0]}</span>
        <span>${priceRange[1]}+</span>
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
}: ShopFiltersProps) {
  return (
    <>
      {/* Desktop Sidebar Filters */}
      <aside
        className="hidden w-64 shrink-0 space-y-8 md:block"
        aria-label="Filters"
      >
        <PriceRangeFilter
          priceRange={priceRange}
          setPriceRange={setPriceRange}
        />

        <FilterSection
          title="Category"
          items={(categories || []).map((c: any) => ({
            id: c.id,
            name: c.name,
          }))}
          selectedItems={selectedCategories}
          onToggle={onToggleCategory}
        />

        <FilterSection
          title="Publisher"
          items={(publishers || []).map((p: any) => ({
            id: p.id,
            name: p.name,
          }))}
          selectedItems={selectedPublishers}
          onToggle={onTogglePublisher}
        />

        <FilterSection
          title="Author"
          items={(authors || []).map((a: any) => ({ id: a.id, name: a.name }))}
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

          <div className="mt-8 space-y-8 px-4">
            <PriceRangeFilter
              priceRange={priceRange}
              setPriceRange={setPriceRange}
            />

            <FilterSection
              title="Category"
              items={(categories || []).map((c: any) => ({
                id: c.id,
                name: c.name,
              }))}
              selectedItems={selectedCategories}
              onToggle={onToggleCategory}
            />

            <FilterSection
              title="Publisher"
              items={(publishers || []).map((p: any) => ({
                id: p.id,
                name: p.name,
              }))}
              selectedItems={selectedPublishers}
              onToggle={onTogglePublisher}
            />

            <FilterSection
              title="Author"
              items={(authors || []).map((a: any) => ({
                id: a.id,
                name: a.name,
              }))}
              selectedItems={selectedAuthors}
              onToggle={onToggleAuthor}
            />
          </div>
        </SheetContent>
      </Sheet>
    </>
  );
}
