"use client";

import { Checkbox } from "@workspace/ui/components/checkbox";
import { Label } from "@workspace/ui/components/label";

type FilterCheckboxProps = {
  label: string;
  checked: boolean;
  onChange: () => void;
};

export function FilterCheckbox({
  label,
  checked,
  onChange,
}: FilterCheckboxProps) {
  const id = `filter-${label.toLowerCase().replace(/\s+/g, "-")}`;

  return (
    <div className="flex items-center gap-2">
      <Checkbox
        id={id}
        checked={checked}
        onCheckedChange={onChange}
        aria-checked={checked}
      />
      <Label
        htmlFor={id}
        className="group-hover:text-primary cursor-pointer transition-colors"
      >
        {label}
      </Label>
    </div>
  );
}
