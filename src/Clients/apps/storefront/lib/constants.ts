import { env } from "@/env.mjs";

export const APP_CONFIG = {
  name: "BookWorm",
  email: {
    support: "support@bookworm.com",
  },
  social: {
    twitter: "https://twitter.com/bookworm",
    facebook: "https://facebook.com/bookworm",
  },
} as const;

export const FILTER_SECTIONS = {
  category: "category",
  publisher: "publisher",
  author: "author",
} as const;

export const ITEMS_PER_PAGE = 12;

export const PRICE_RANGE = {
  min: 0,
  max: 100,
  step: 1,
} as const;

export const SORT_OPTIONS = [
  { value: "default", label: "Featured" },
  { value: "price-low", label: "Price: Low to High" },
  { value: "price-high", label: "Price: High to Low" },
  { value: "title", label: "Title: A-Z" },
] as const;

export const DEBOUNCE_DELAY = {
  search: 300,
  quantity: 300,
  priceRange: 500,
} as const;

export const DEFAULT_BOOK_IMAGE = "/book-placeholder.svg";
