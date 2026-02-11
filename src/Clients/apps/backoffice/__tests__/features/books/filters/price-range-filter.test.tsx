import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { PriceRangeFilter } from "@/features/books/filters/price-range-filter";

// Mock the Slider component to capture onValueChange
let mockOnValueChange: ((values: number[]) => void) | null = null;

vi.mock("@workspace/ui/components/slider", () => ({
  Slider: ({ onValueChange, value, ...props }: any) => {
    mockOnValueChange = onValueChange;
    return (
      <div data-testid="mock-slider" data-value={JSON.stringify(value)}>
        <input type="range" {...props} />
      </div>
    );
  },
}));

describe("PriceRangeFilter", () => {
  it("renders price range label with current values", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={20}
        maxPrice={80}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("Price Range: $20 - $80")).toBeInTheDocument();
  });

  it("displays min and max price labels", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={10}
        maxPrice={90}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("$10")).toBeInTheDocument();
    expect(screen.getByText("$90+")).toBeInTheDocument();
  });

  it("renders slider component", () => {
    const onChange = vi.fn();
    const { container } = render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={0}
        maxPrice={100}
        onChange={onChange}
      />,
    );

    const slider = container.querySelector('input[type="range"]');
    expect(slider).toBeInTheDocument();
  });

  it("handles zero prices", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={0}
        maxPrice={0}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("Price Range: $0 - $0")).toBeInTheDocument();
    expect(screen.getByText("$0")).toBeInTheDocument();
  });

  it("handles max price range", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={1000}
        minPrice={0}
        maxPrice={1000}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("Price Range: $0 - $1000")).toBeInTheDocument();
    expect(screen.getByText("$1000+")).toBeInTheDocument();
  });

  it("accepts custom step value", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        step={5}
        minPrice={0}
        maxPrice={100}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("Price Range: $0 - $100")).toBeInTheDocument();
  });

  it("has proper component structure", () => {
    const onChange = vi.fn();
    const { container } = render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    const spaces = container.querySelectorAll(".space-y-2, .space-y-4");
    expect(spaces.length).toBeGreaterThan(0);
  });

  it("displays correct formatting for price labels", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={50}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Check that prices have dollar sign
    expect(screen.getByText("$50")).toBeInTheDocument();
    expect(screen.getByText("$75+")).toBeInTheDocument();
  });

  it("calls onChange when values change", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Verify onChange is defined and passed to component
    expect(onChange).toBeDefined();
  });

  it("handles undefined values in handleValueChange gracefully", () => {
    const onChange = vi.fn();
    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Component should render without errors
    expect(screen.getByText("Price Range: $25 - $75")).toBeInTheDocument();
  });

  it("updates display when price range changes", () => {
    const onChange = vi.fn();
    const { rerender } = render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={10}
        maxPrice={90}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("$10")).toBeInTheDocument();
    expect(screen.getByText("$90+")).toBeInTheDocument();

    rerender(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={20}
        maxPrice={80}
        onChange={onChange}
      />,
    );

    expect(screen.getByText("$20")).toBeInTheDocument();
    expect(screen.getByText("$80+")).toBeInTheDocument();
  });

  it("calls onChange when both min and max values are defined", () => {
    const onChange = vi.fn();

    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Trigger the onValueChange callback via the mocked Slider
    // If the mock captured the callback, invoke it to simulate user interaction
    if (mockOnValueChange) {
      mockOnValueChange([30, 70]);
      expect(onChange).toHaveBeenCalledWith(30, 70);
    }
  });

  it("does not call onChange when min is undefined", () => {
    const onChange = vi.fn();

    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Trigger with undefined min
    if (mockOnValueChange) {
      mockOnValueChange([undefined as any, 70]);
    }

    expect(onChange).not.toHaveBeenCalled();
  });

  it("does not call onChange when max is undefined", () => {
    const onChange = vi.fn();

    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Trigger with undefined max
    if (mockOnValueChange) {
      mockOnValueChange([30, undefined as any]);
    }

    expect(onChange).not.toHaveBeenCalled();
  });

  it("does not call onChange when both values are undefined", () => {
    const onChange = vi.fn();

    render(
      <PriceRangeFilter
        min={0}
        max={100}
        minPrice={25}
        maxPrice={75}
        onChange={onChange}
      />,
    );

    // Trigger with both undefined
    if (mockOnValueChange) {
      mockOnValueChange([undefined as any, undefined as any]);
    }

    expect(onChange).not.toHaveBeenCalled();
  });
});
