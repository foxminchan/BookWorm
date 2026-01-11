"use client";

import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Skeleton } from "@workspace/ui/components/skeleton";

type CategorySelectProps = {
  value: string | undefined;
  onChange: (value: string | undefined) => void;
};

export function CategorySelect({ value, onChange }: CategorySelectProps) {
  const { data: categories, isLoading } = useCategories();

  if (isLoading) {
    return (
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-10 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <Label htmlFor="category-select" className="text-xs">
        Category
      </Label>
      <Select
        value={value ?? ""}
        onValueChange={(val) => onChange(val || undefined)}
      >
        <SelectTrigger id="category-select" className="w-full">
          <SelectValue placeholder="All Categories" />
        </SelectTrigger>
        <SelectContent>
          {categories?.map((cat) => (
            <SelectItem key={cat.id} value={cat.id}>
              {cat.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
