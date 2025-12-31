"use client";

import type React from "react";
import { Button } from "@workspace/ui/components/button";
import { ShoppingBasket, Loader2 } from "lucide-react";

type ProductActionsProps = {
  quantity: number;
  status: string;
  isAddingToBasket: boolean;
  onAddToBasket: () => void;
  onQuantityChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onDecrease: () => void;
  onIncrease: () => void;
};

export function ProductActions({
  quantity,
  status,
  isAddingToBasket,
  onAddToBasket,
  onQuantityChange,
  onDecrease,
  onIncrease,
}: ProductActionsProps) {
  return (
    <div className="flex flex-col sm:flex-row gap-4">
      {quantity === 0 ? (
        <Button
          size="lg"
          className="rounded-full h-14 text-lg gap-3 w-full sm:w-auto px-10 shadow-lg shadow-primary/20"
          onClick={onAddToBasket}
          disabled={status !== "InStock" || isAddingToBasket}
        >
          {isAddingToBasket ? (
            <>
              <Loader2 className="size-5 animate-spin" />
              Adding...
            </>
          ) : (
            <>
              <ShoppingBasket className="size-5" /> Add to Basket
            </>
          )}
        </Button>
      ) : (
        <div className="flex items-center bg-secondary/50 rounded-full h-12 p-1 w-fit shadow-inner">
          <Button
            variant="ghost"
            size="icon"
            className="rounded-full size-10 hover:bg-background shadow-sm transition-all"
            onClick={onDecrease}
            aria-label="Decrease quantity"
          >
            <span className="text-lg font-medium">-</span>
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
            className="rounded-full size-10 hover:bg-background shadow-sm transition-all"
            onClick={onIncrease}
            aria-label="Increase quantity"
          >
            <span className="text-lg font-medium">+</span>
          </Button>
        </div>
      )}
    </div>
  );
}
