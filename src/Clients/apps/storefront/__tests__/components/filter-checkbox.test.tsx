import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { FilterCheckbox } from "@/components/filter-checkbox";

import { renderWithProviders } from "../utils/test-utils";

describe("FilterCheckbox", () => {
  it("should render with label", () => {
    renderWithProviders(
      <FilterCheckbox label="Fiction" checked={false} onChange={vi.fn()} />,
    );

    expect(screen.getByText("Fiction")).toBeInTheDocument();
  });

  it("should render checked state correctly", () => {
    renderWithProviders(
      <FilterCheckbox label="Fiction" checked onChange={vi.fn()} />,
    );

    const checkbox = screen.getByRole("checkbox", { name: "Fiction" });
    expect(checkbox).toBeChecked();
  });

  it("should render unchecked state correctly", () => {
    renderWithProviders(
      <FilterCheckbox label="Fiction" checked={false} onChange={vi.fn()} />,
    );

    const checkbox = screen.getByRole("checkbox", { name: "Fiction" });
    expect(checkbox).not.toBeChecked();
  });

  it("should call onChange when clicked", async () => {
    const user = userEvent.setup();
    const mockOnChange = vi.fn();

    renderWithProviders(
      <FilterCheckbox
        label="Fiction"
        checked={false}
        onChange={mockOnChange}
      />,
    );

    const checkbox = screen.getByRole("checkbox", { name: "Fiction" });
    await user.click(checkbox);

    expect(mockOnChange).toHaveBeenCalledTimes(1);
  });

  it("should call onChange when label is clicked", async () => {
    const user = userEvent.setup();
    const mockOnChange = vi.fn();

    renderWithProviders(
      <FilterCheckbox
        label="Fiction"
        checked={false}
        onChange={mockOnChange}
      />,
    );

    const label = screen.getByText("Fiction");
    await user.click(label);

    expect(mockOnChange).toHaveBeenCalledTimes(1);
  });

  it("should toggle between checked and unchecked states", async () => {
    const user = userEvent.setup();
    const mockOnChange = vi.fn();

    const { rerender } = renderWithProviders(
      <FilterCheckbox
        label="Fiction"
        checked={false}
        onChange={mockOnChange}
      />,
    );

    const checkbox = screen.getByRole("checkbox", { name: "Fiction" });
    expect(checkbox).not.toBeChecked();

    await user.click(checkbox);
    expect(mockOnChange).toHaveBeenCalled();

    // Simulate parent component updating checked state
    rerender(
      <FilterCheckbox label="Fiction" checked onChange={mockOnChange} />,
    );

    expect(screen.getByRole("checkbox", { name: "Fiction" })).toBeChecked();
  });

  it("should show checkmark icon when checked", () => {
    const { container } = renderWithProviders(
      <FilterCheckbox label="Fiction" checked onChange={vi.fn()} />,
    );

    const svg = container.querySelector("svg");
    expect(svg).toBeInTheDocument();
  });

  it("should not show checkmark icon when unchecked", () => {
    const { container } = renderWithProviders(
      <FilterCheckbox label="Fiction" checked={false} onChange={vi.fn()} />,
    );

    const svg = container.querySelector("svg");
    expect(svg).not.toBeInTheDocument();
  });

  it("should have accessible label", () => {
    renderWithProviders(
      <FilterCheckbox
        label="Science Fiction"
        checked={false}
        onChange={vi.fn()}
      />,
    );

    const checkbox = screen.getByRole("checkbox", { name: "Science Fiction" });
    expect(checkbox).toBeInTheDocument();
  });

  it("should handle long labels", () => {
    const longLabel =
      "Very Long Category Name That Might Wrap To Multiple Lines";

    renderWithProviders(
      <FilterCheckbox label={longLabel} checked={false} onChange={vi.fn()} />,
    );

    expect(screen.getByText(longLabel)).toBeInTheDocument();
  });

  it("should have cursor pointer styling", () => {
    const { container } = renderWithProviders(
      <FilterCheckbox label="Fiction" checked={false} onChange={vi.fn()} />,
    );

    const label = container.querySelector("label");
    expect(label).toHaveClass("cursor-pointer");
  });
});
