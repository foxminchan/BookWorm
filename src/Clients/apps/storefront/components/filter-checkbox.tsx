"use client";

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
  return (
    <label className="group flex cursor-pointer items-center gap-2">
      <div className="relative h-4 w-4">
        <input
          type="checkbox"
          checked={checked}
          onChange={onChange}
          className="border-border bg-card checked:bg-primary checked:border-primary h-4 w-4 cursor-pointer appearance-none rounded border transition"
          aria-label={label}
        />
        {checked && (
          <svg
            className="text-primary-foreground pointer-events-none absolute inset-0 h-4 w-4"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            aria-hidden="true"
          >
            <path d="M3 8l3 3 7-7" />
          </svg>
        )}
      </div>
      <span className="group-hover:text-primary text-sm transition-colors">
        {label}
      </span>
    </label>
  );
}
