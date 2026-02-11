"use client";

import { useCallback } from "react";

import { Slider } from "@workspace/ui/components/slider";

type PriceRangeSliderProps = Readonly<{
  min: number;
  max: number;
  step?: number;
  minPrice: number;
  maxPrice: number;
  onMinChange: (value: number) => void;
  onMaxChange: (value: number) => void;
}>;

export function PriceRangeSlider({
  min,
  max,
  step = 1,
  minPrice,
  maxPrice,
  onMinChange,
  onMaxChange,
}: PriceRangeSliderProps) {
  const handleValueChange = useCallback(
    (values: number[]) => {
      const [newMin, newMax] = values;
      if (newMin !== undefined && newMin !== minPrice) {
        onMinChange(newMin);
      }
      if (newMax !== undefined && newMax !== maxPrice) {
        onMaxChange(newMax);
      }
    },
    [minPrice, maxPrice, onMinChange, onMaxChange],
  );

  return (
    <div className="space-y-4">
      <Slider
        min={min}
        max={max}
        step={step}
        value={[minPrice, maxPrice]}
        onValueChange={handleValueChange}
        className="w-full"
      />

      {/* Labels */}
      <div className="text-muted-foreground flex justify-between text-sm">
        <span>${minPrice}</span>
        <span>${maxPrice}+</span>
      </div>
    </div>
  );
}
