"use client";

import type React from "react";
import { Button } from "@workspace/ui/components/button";
import { ShoppingBasket, Loader2 } from "lucide-react";
import { QuantityControl } from "@/components/quantity-control";

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
        <QuantityControl
          quantity={quantity}
          onDecrease={onDecrease}
          onIncrease={onIncrease}
          onQuantityChange={onQuantityChange}
          variant="input"
          size="lg"
          showBorder={false}
        />
      )}
    </div>
  );
}
