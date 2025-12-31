"use client";

import type React from "react";
import { Plus, Minus } from "lucide-react";
import { Button } from "@workspace/ui/components/button";

type QuantityControlProps = {
  quantity: number;
  onIncrease: () => void;
  onDecrease: () => void;
  size?: "sm" | "md" | "lg";
  showBorder?: boolean;
  className?: string;
  variant?: "simple" | "input";
  onQuantityChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
};

export function QuantityControl({
  quantity,
  onIncrease,
  onDecrease,
  size = "md",
  showBorder = true,
  className = "",
  variant = "simple",
  onQuantityChange,
}: QuantityControlProps) {
  const sizeClasses = {
    sm: "h-8 px-2",
    md: "h-10 px-2",
    lg: "h-12 px-3",
  };

  const iconSizes = {
    sm: "size-3",
    md: "size-4",
    lg: "size-5",
  };

  if (variant === "input") {
    return (
      <div
        className={`flex items-center bg-secondary/50 dark:bg-gray-800/50 rounded-full ${sizeClasses[size]} p-1 w-fit shadow-inner ${className}`}
      >
        <Button
          variant="ghost"
          size="icon"
          className="rounded-full size-10 hover:bg-background dark:hover:bg-gray-700 shadow-sm transition-all"
          onClick={onDecrease}
          aria-label="Decrease quantity"
        >
          <Minus className={iconSizes[size]} />
        </Button>
        <input
          type="text"
          inputMode="numeric"
          min="1"
          max="99"
          value={quantity}
          onChange={onQuantityChange}
          className="w-12 bg-transparent text-center text-lg font-serif font-bold focus:outline-none border-none [&::-webkit-outer-spin-button]:hidden [&::-webkit-inner-spin-button]:hidden"
          aria-label="Book quantity"
        />
        <Button
          variant="ghost"
          size="icon"
          className="rounded-full size-10 hover:bg-background dark:hover:bg-gray-700 shadow-sm transition-all"
          onClick={onIncrease}
          aria-label="Increase quantity"
        >
          <Plus className={iconSizes[size]} />
        </Button>
      </div>
    );
  }

  return (
    <div
      className={`flex items-center ${showBorder ? "border rounded-full bg-white dark:bg-gray-900 dark:border-gray-700" : ""} ${sizeClasses[size]} ${className}`}
    >
      <button
        onClick={onDecrease}
        className="p-1 hover:text-primary transition-colors disabled:opacity-50"
        aria-label="Decrease quantity"
      >
        <Minus className={iconSizes[size]} />
      </button>
      <span
        className={`w-8 text-center font-medium ${quantity <= 0 ? "text-destructive" : ""}`}
        aria-live="polite"
        aria-atomic="true"
      >
        {quantity}
      </span>
      <button
        onClick={onIncrease}
        className="p-1 hover:text-primary transition-colors"
        aria-label="Increase quantity"
      >
        <Plus className={iconSizes[size]} />
      </button>
    </div>
  );
}
