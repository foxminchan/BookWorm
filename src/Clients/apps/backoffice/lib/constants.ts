// API
export const API = {
  DEFAULT_RETRY: 3,
  DEFAULT_TIMEOUT: 30000,
} as const;

// Pagination
export const PAGE_SIZES = [10, 20, 50] as const;
export const DEFAULT_PAGE_SIZE = 10;
export const DEFAULT_BOOKS_PAGE_SIZE = 100;

// Chart colors
export const CHART_COLORS = [
  "#10b981",
  "#06b6d4",
  "#f59e0b",
  "#8b5cf6",
  "#ec4899",
] as const;

// Currency
export const currencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
});

// Chart theme
export const CHART_THEME = {
  tooltip: {
    backgroundColor: "#171717",
    border: "1px solid #262626",
  },
  grid: {
    stroke: "#262626",
    strokeDasharray: "3 3",
  },
  axis: {
    stroke: "#a1a1a1",
  },
} as const;
