import { match } from "ts-pattern";

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
