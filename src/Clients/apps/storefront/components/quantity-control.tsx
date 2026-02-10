"use client";

import type { ChangeEvent } from "react";

import { Minus, Plus } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";

const SIZE_CLASSES = {
  sm: "h-8 px-2",
  md: "h-10 px-2",
  lg: "h-12 px-3",
} as const;

const ICON_SIZES = {
  sm: "size-3",
  md: "size-4",
  lg: "size-5",
} as const;

type BaseProps = {
  quantity: number;
  onIncrease: () => void;
  onDecrease: () => void;
  size?: "sm" | "md" | "lg";
  className?: string;
};

type SimpleVariantProps = BaseProps & {
  variant?: "simple";
  showBorder?: boolean;
};

type InputVariantProps = BaseProps & {
  variant: "input";
  onQuantityChange: (e: ChangeEvent<HTMLInputElement>) => void;
};

type QuantityControlProps = SimpleVariantProps | InputVariantProps;

export function QuantityControl(props: Readonly<QuantityControlProps>) {
  const {
    quantity,
    onIncrease,
    onDecrease,
    size = "md",
    className,
  } = props;

  if (props.variant === "input") {
    return (
      <div
        className={`bg-secondary/50 flex w-fit items-center rounded-full p-1 shadow-inner dark:bg-gray-800/50 ${SIZE_CLASSES[size]} ${className ?? ""}`}
      >
        <Button
          variant="ghost"
          size="icon"
          className="hover:bg-background size-10 rounded-full shadow-sm transition-all dark:hover:bg-gray-700"
          onClick={onDecrease}
          aria-label="Decrease quantity"
        >
          <Minus className={ICON_SIZES[size]} aria-hidden="true" />
        </Button>
        <Input
          type="text"
          inputMode="numeric"
          value={quantity}
          onChange={props.onQuantityChange}
          className="h-auto w-12 border-0 bg-transparent text-center font-serif text-lg font-bold shadow-none focus-visible:ring-0"
          aria-label="Book quantity"
        />
        <Button
          variant="ghost"
          size="icon"
          className="hover:bg-background size-10 rounded-full shadow-sm transition-all dark:hover:bg-gray-700"
          onClick={onIncrease}
          aria-label="Increase quantity"
        >
          <Plus className={ICON_SIZES[size]} aria-hidden="true" />
        </Button>
      </div>
    );
  }

  const showBorder = props.showBorder ?? true;

  return (
    <div
      className={`flex items-center ${showBorder ? "rounded-full border bg-white dark:border-gray-700 dark:bg-gray-900" : ""} ${SIZE_CLASSES[size]} ${className ?? ""}`}
    >
      <Button
        variant="ghost"
        size="icon"
        onClick={onDecrease}
        className="hover:text-primary h-auto w-auto p-1 transition-colors disabled:opacity-50"
        aria-label="Decrease quantity"
        disabled={quantity <= 0}
      >
        <Minus className={ICON_SIZES[size]} aria-hidden="true" />
      </Button>
      <output
        className={`w-8 text-center font-medium ${quantity <= 0 ? "text-destructive" : ""}`}
        aria-live="polite"
        aria-atomic="true"
        aria-label={`Quantity: ${quantity}`}
      >
        {quantity}
      </output>
      <Button
        variant="ghost"
        size="icon"
        onClick={onIncrease}
        className="hover:text-primary h-auto w-auto p-1 transition-colors"
        aria-label="Increase quantity"
      >
        <Plus className={ICON_SIZES[size]} aria-hidden="true" />
      </Button>
    </div>
  );
}
