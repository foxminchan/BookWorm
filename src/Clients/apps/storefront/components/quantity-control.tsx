"use client";

import { Plus, Minus } from "lucide-react";

type QuantityControlProps = {
  quantity: number;
  onIncrease: () => void;
  onDecrease: () => void;
  size?: "sm" | "md" | "lg";
  showBorder?: boolean;
  className?: string;
};

export function QuantityControl({
  quantity,
  onIncrease,
  onDecrease,
  size = "md",
  showBorder = true,
  className = "",
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

  return (
    <div
      className={`flex items-center ${showBorder ? "border rounded-full bg-white" : ""} ${sizeClasses[size]} ${className}`}
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
