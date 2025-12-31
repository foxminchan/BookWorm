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
    <label className="flex items-center gap-2 cursor-pointer group">
      <div className="relative w-4 h-4">
        <input
          type="checkbox"
          checked={checked}
          onChange={onChange}
          className="appearance-none w-4 h-4 border border-border rounded bg-card checked:bg-primary checked:border-primary transition cursor-pointer"
          aria-label={label}
        />
        {checked && (
          <svg
            className="absolute inset-0 w-4 h-4 text-primary-foreground pointer-events-none"
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
      <span className="text-sm group-hover:text-primary transition-colors">
        {label}
      </span>
    </label>
  );
}
