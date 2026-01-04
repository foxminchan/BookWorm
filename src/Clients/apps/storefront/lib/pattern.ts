import { match } from "ts-pattern";

import type { OrderStatus } from "@workspace/types/ordering/orders";

/**
 * Sort parameter configuration
 */
type SortParams = {
  orderBy: string;
  isDescending: boolean;
};

/**
 * Maps shop sort options to API sort parameters using ts-pattern
 * @param sortBy - Sort option: 'price-low', 'price-high', 'rating', 'name'
 * @returns API sort parameters
 */
export function getShopSortParams(sortBy: string): SortParams {
  return match(sortBy)
    .with("price-low", () => ({ orderBy: "price", isDescending: false }))
    .with("price-high", () => ({ orderBy: "price", isDescending: true }))
    .with("rating", () => ({ orderBy: "averageRating", isDescending: true }))
    .with("name", () => ({ orderBy: "name", isDescending: false }))
    .otherwise(() => ({ orderBy: "name", isDescending: false }));
}

/**
 * Maps review sort options to API sort parameters using ts-pattern
 * @param sortBy - Sort option: 'newest', 'highest', 'lowest'
 * @returns API sort parameters
 */
export function getReviewSortParams(
  sortBy: "newest" | "highest" | "lowest",
): SortParams {
  return match(sortBy)
    .with("highest", () => ({ orderBy: "rating", isDescending: true }))
    .with("lowest", () => ({ orderBy: "rating", isDescending: false }))
    .with("newest", () => ({ orderBy: "createdAt", isDescending: true }))
    .exhaustive();
}

/**
 * Maps order status to Tailwind CSS classes using ts-pattern
 * Provides consistent styling across order views
 * @param status - Order status
 * @returns Tailwind CSS class string
 */
export function getOrderStatusColor(status: OrderStatus): string {
  return match(status)
    .with(
      "Completed",
      () =>
        "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
    )
    .with(
      "Cancelled",
      () => "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
    )
    .with(
      "New",
      () => "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
    )
    .otherwise(() => "bg-gray-100 text-gray-800");
}

/**
 * Maps order status to Tailwind CSS classes for order list view
 * Uses bordered variant for better visual separation
 * @param status - Order status
 * @returns Tailwind CSS class string with borders
 */
export function getOrderStatusColorBordered(status: OrderStatus): string {
  return match(status)
    .with(
      "Completed",
      () =>
        "bg-green-50 text-green-900 border border-green-200 dark:bg-green-950/20 dark:text-green-300 dark:border-green-800",
    )
    .with(
      "Cancelled",
      () =>
        "bg-red-50 text-red-900 border border-red-200 dark:bg-red-950/20 dark:text-red-300 dark:border-red-800",
    )
    .with(
      "New",
      () =>
        "bg-blue-50 text-blue-900 border border-blue-200 dark:bg-blue-950/20 dark:text-blue-300 dark:border-blue-800",
    )
    .otherwise(() => "bg-gray-100 text-gray-800");
}
