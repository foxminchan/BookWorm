"use client";

import { useCallback } from "react";

import { Label } from "@workspace/ui/components/label";
import { Slider } from "@workspace/ui/components/slider";

type PriceRangeFilterProps = Readonly<{
  min: number;
  max: number;
  step?: number;
  minPrice: number;
  maxPrice: number;
  onChange: (min: number, max: number) => void;
}>;

export function PriceRangeFilter({
  min,
  max,
  step = 1,
  minPrice,
  maxPrice,
  onChange,
}: PriceRangeFilterProps) {
  const handleValueChange = useCallback(
    (values: number[]) => {
      const [newMin, newMax] = values;
      if (newMin !== undefined && newMax !== undefined) {
        onChange(newMin, newMax);
      }
    },
    [onChange],
  );

  return (
    <div className="space-y-2">
      <Label className="text-xs">
        Price Range: ${minPrice} - ${maxPrice}
      </Label>
      <div className="space-y-4">
        <Slider
          min={min}
          max={max}
          step={step}
          value={[minPrice, maxPrice]}
          onValueChange={handleValueChange}
          className="w-full"
        />
        <div className="text-muted-foreground flex justify-between text-sm">
          <span>${minPrice}</span>
          <span>${maxPrice}+</span>
        </div>
      </div>
    </div>
  );
}
