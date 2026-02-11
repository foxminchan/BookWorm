/**
 * Shared constants across BookWorm frontend applications
 * @module constants
 */

/**
 * Default placeholder image for books without cover images
 */
export const DEFAULT_BOOK_IMAGE = "/book-placeholder.svg";

/**
 * Price range configuration for book filtering
 */
export const PRICE_RANGE = {
  min: 0,
  max: 100,
  step: 1,
} as const;

/**
 * Currency formatter for displaying prices in USD
 */
export const currencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
});
