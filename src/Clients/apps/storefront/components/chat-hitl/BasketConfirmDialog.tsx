"use client";

import { useCallback, useEffect, useState } from "react";

import { Minus, Plus, ShoppingCart, X } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";
import { formatPrice } from "@workspace/utils/format";

export type BasketConfirmDialogProps = {
  /** Book UUID */
  bookId: string;
  /** Display title of the book */
  bookTitle?: string;
  /** Unit price in numeric form */
  unitPrice?: number;
  /** Initial quantity suggested by the agent (default 1) */
  initialQuantity?: number;
  /** Maximum quantity allowed (stock limit, default 99) */
  maxQuantity?: number;
  /** Called when the user confirms with the chosen quantity */
  onConfirm: (quantity: number) => void;
  /** Called when the user dismisses without confirming */
  onDismiss: () => void;
};

/**
 * Inline HITL confirmation dialog rendered inside the chat panel.
 * Allows the customer to adjust quantity (stepper + direct input) and
 * shows a live subtotal before confirming the basket add.
 */
export function BasketConfirmDialog({
  bookId,
  bookTitle,
  unitPrice,
  initialQuantity = 1,
  maxQuantity = 99,
  onConfirm,
  onDismiss,
}: Readonly<BasketConfirmDialogProps>) {
  const [quantity, setQuantity] = useState(
    Math.max(1, Math.min(initialQuantity, maxQuantity)),
  );
  const [inputValue, setInputValue] = useState(String(quantity));
  const [inputError, setInputError] = useState<string | null>(null);

  // Keep input field in sync with stepper changes
  useEffect(() => {
    setInputValue(String(quantity));
    setInputError(null);
  }, [quantity]);

  const decrease = useCallback(() => {
    setQuantity((q) => Math.max(1, q - 1));
  }, []);

  const increase = useCallback(() => {
    setQuantity((q) => Math.min(maxQuantity, q + 1));
  }, [maxQuantity]);

  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const raw = e.target.value;
      setInputValue(raw);

      const parsed = parseInt(raw, 10);
      if (isNaN(parsed) || parsed < 1) {
        setInputError("Quantity must be at least 1.");
        return;
      }
      if (parsed > maxQuantity) {
        setInputError(`Maximum quantity is ${maxQuantity}.`);
        return;
      }
      setInputError(null);
      setQuantity(parsed);
    },
    [maxQuantity],
  );

  const handleConfirm = useCallback(() => {
    if (inputError) return;
    onConfirm(quantity);
  }, [inputError, onConfirm, quantity]);

  const subtotal = unitPrice !== undefined ? unitPrice * quantity : undefined;

  const isAtMax = quantity >= maxQuantity;

  return (
    <div
      role="dialog"
      aria-modal="false"
      aria-labelledby="basket-confirm-title"
      className="mt-2 rounded-lg border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-950"
    >
      {/* Header */}
      <div className="mb-3 flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <ShoppingCart
            className="h-5 w-5 shrink-0 text-blue-600 dark:text-blue-400"
            aria-hidden="true"
          />
          <h3
            id="basket-confirm-title"
            className="text-sm font-semibold text-blue-900 dark:text-blue-100"
          >
            Add to Basket
          </h3>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="h-6 w-6 text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-200"
          onClick={onDismiss}
          aria-label="Dismiss basket confirmation"
        >
          <X className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>

      {/* Book name */}
      {bookTitle && (
        <p className="mb-3 text-sm font-medium text-blue-900 dark:text-blue-100">
          {bookTitle}
        </p>
      )}

      {/* Quantity controls */}
      <div className="mb-3">
        <label
          htmlFor="basket-quantity-input"
          className="mb-1 block text-xs font-medium text-blue-700 dark:text-blue-300"
        >
          Quantity
          {isAtMax && (
            <span
              className="ml-2 text-xs text-amber-600 dark:text-amber-400"
              role="status"
              aria-live="polite"
            >
              (max {maxQuantity})
            </span>
          )}
        </label>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8 shrink-0 border-blue-300 bg-white dark:border-blue-700 dark:bg-blue-900"
            onClick={decrease}
            disabled={quantity <= 1}
            aria-label="Decrease quantity"
          >
            <Minus className="h-3 w-3" aria-hidden="true" />
          </Button>
          <Input
            id="basket-quantity-input"
            type="text"
            inputMode="numeric"
            value={inputValue}
            onChange={handleInputChange}
            className="h-8 w-16 border-blue-300 bg-white text-center text-sm dark:border-blue-700 dark:bg-blue-900"
            aria-label="Quantity"
            aria-describedby={inputError ? "basket-qty-error" : undefined}
          />
          <Button
            variant="outline"
            size="icon"
            className="h-8 w-8 shrink-0 border-blue-300 bg-white dark:border-blue-700 dark:bg-blue-900"
            onClick={increase}
            disabled={isAtMax}
            aria-label="Increase quantity"
          >
            <Plus className="h-3 w-3" aria-hidden="true" />
          </Button>
        </div>

        {inputError && (
          <p
            id="basket-qty-error"
            role="alert"
            className="mt-1 text-xs text-red-600 dark:text-red-400"
          >
            {inputError}
          </p>
        )}
      </div>

      {/* Subtotal */}
      {subtotal !== undefined && (
        <div
          className="mb-3 flex justify-between text-sm"
          aria-live="polite"
          aria-atomic="true"
        >
          <span className="text-blue-700 dark:text-blue-300">Subtotal</span>
          <span className="font-semibold text-blue-900 dark:text-blue-100">
            {formatPrice(subtotal)}
          </span>
        </div>
      )}

      {/* Action buttons */}
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          className="flex-1 border-blue-300 text-blue-700 hover:bg-blue-100 dark:border-blue-700 dark:text-blue-300 dark:hover:bg-blue-900"
          onClick={onDismiss}
        >
          Cancel
        </Button>
        <Button
          size="sm"
          className="flex-1 bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
          onClick={handleConfirm}
          disabled={!!inputError}
        >
          Add to Basket
        </Button>
      </div>
    </div>
  );
}
