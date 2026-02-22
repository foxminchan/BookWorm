"use client";

import { useEffect, useRef } from "react";

import { useRouter } from "next/navigation";

import { Search } from "lucide-react";
import { useForm } from "react-hook-form";

import { Button } from "@workspace/ui/components/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
} from "@workspace/ui/components/form";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";

import { useDelayedToggle } from "@/hooks/useDelayedToggle";

type SearchFormValues = {
  search: string;
};

export function HeaderSearch() {
  const router = useRouter();
  const { isOpen, close, toggle, handleMouseEnter, handleMouseLeave } =
    useDelayedToggle({ closeDelay: 150 });
  const searchInputRef = useRef<HTMLInputElement>(null);
  const form = useForm<SearchFormValues>({
    defaultValues: { search: "" },
  });

  useEffect(() => {
    if (isOpen) {
      searchInputRef.current?.focus();
    }
  }, [isOpen]);

  const handleSearch = (data: SearchFormValues) => {
    if (data.search.trim()) {
      router.push(`/shop?search=${encodeURIComponent(data.search)}`);
      close();
      form.reset();
    }
  };

  return (
    <div
      className="relative"
      role="search"
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      <Button
        type="button"
        variant="ghost"
        size="icon"
        className="h-9 w-9 rounded-full"
        onClick={toggle}
        onKeyDown={(e) => {
          if (e.key === "Escape" && isOpen) {
            close();
          }
        }}
        aria-label="Search books"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <Search
          className="text-foreground/60 size-4 md:size-5"
          aria-hidden="true"
        />
      </Button>
      {isOpen && (
        <div className="bg-background border-foreground/10 animate-in fade-in slide-in-from-top-2 absolute top-full right-0 mt-2 w-48 rounded-lg border p-2 shadow-lg duration-200 md:w-64">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(handleSearch)}>
              <FormField
                control={form.control}
                name="search"
                render={({ field }) => {
                  const { ref: fieldRef, ...fieldRest } = field;
                  return (
                    <FormItem>
                      <FormControl>
                        <Label
                          htmlFor="header-search-input"
                          className="sr-only"
                        >
                          Search books
                        </Label>
                        <Input
                          id="header-search-input"
                          ref={(node) => {
                            fieldRef(node);
                            (
                              searchInputRef as React.MutableRefObject<HTMLInputElement | null>
                            ).current = node;
                          }}
                          placeholder="Search books..."
                          aria-label="Search books"
                          className="border-foreground/20 focus:border-primary placeholder:text-foreground/40 border-x-0 border-t-0 border-b bg-transparent px-3 py-2 text-sm"
                          {...fieldRest}
                        />
                      </FormControl>
                    </FormItem>
                  );
                }}
              />
            </form>
          </Form>
        </div>
      )}
    </div>
  );
}

export function MobileSearch() {
  const router = useRouter();
  const form = useForm<SearchFormValues>({
    defaultValues: { search: "" },
  });

  const handleSearch = (data: SearchFormValues) => {
    if (data.search.trim()) {
      router.push(`/shop?search=${encodeURIComponent(data.search)}`);
      form.reset();
    }
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(handleSearch)}
        className="hover:bg-secondary relative flex w-full items-center rounded-full p-2 transition-colors"
        role="search"
        aria-label="Search books"
      >
        <Label htmlFor="mobile-search-input" className="sr-only">
          Search books
        </Label>
        <Search
          className="text-foreground/60 pointer-events-none size-4 shrink-0"
          aria-hidden="true"
        />
        <FormField
          control={form.control}
          name="search"
          render={({ field }) => (
            <FormItem className="flex-1">
              <FormControl>
                <Input
                  id="mobile-search-input"
                  placeholder="Search books..."
                  className="w-full border-0 bg-transparent py-1 pl-2 text-sm outline-none focus-visible:ring-0"
                  {...field}
                />
              </FormControl>
            </FormItem>
          )}
        />
      </form>
    </Form>
  );
}
