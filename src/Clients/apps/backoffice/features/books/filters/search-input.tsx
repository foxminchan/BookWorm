"use client";

import { Search } from "lucide-react";

import { Input } from "@workspace/ui/components/input";

type SearchInputProps = Readonly<{
  value: string;
  onChange: (value: string) => void;
}>;

export function SearchInput({ value, onChange }: SearchInputProps) {
  return (
    <div className="relative">
      <label htmlFor="book-search" className="sr-only">
        Search books by title or author
      </label>
      <Search
        className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2"
        aria-hidden="true"
      />
      <Input
        id="book-search"
        type="text"
        placeholder="Search by title or author..."
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="pl-9"
      />
    </div>
  );
}
