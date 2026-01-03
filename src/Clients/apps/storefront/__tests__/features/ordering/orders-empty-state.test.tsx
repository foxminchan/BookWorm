import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import OrdersEmptyState from "@/features/ordering/orders/orders-empty-state";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrdersEmptyState", () => {
  const mockOnClearFilter = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display empty state message", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    expect(screen.getByText("No Orders Found")).toBeInTheDocument();
  });

  it("should display description text", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    expect(
      screen.getByText(/we couldn't find any orders matching/i),
    ).toBeInTheDocument();
  });

  it("should render package icon", () => {
    const { container } = renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should not show clear filter button when status is All", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    expect(
      screen.queryByRole("button", { name: /clear filter/i }),
    ).not.toBeInTheDocument();
  });

  it("should show clear filter button when status is not All", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="Completed"
        onClearFilter={mockOnClearFilter}
      />,
    );

    expect(
      screen.getByRole("button", { name: /clear filter/i }),
    ).toBeInTheDocument();
  });

  it("should call onClearFilter when button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="New"
        onClearFilter={mockOnClearFilter}
      />,
    );

    const button = screen.getByRole("button", { name: /clear filter/i });
    await user.click(button);

    expect(mockOnClearFilter).toHaveBeenCalledTimes(1);
  });

  it("should have serif font for heading", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    const heading = screen.getByText("No Orders Found");
    expect(heading).toHaveClass("font-serif");
  });

  it("should have rounded icon container", () => {
    const { container } = renderWithProviders(
      <OrdersEmptyState
        selectedStatus="All"
        onClearFilter={mockOnClearFilter}
      />,
    );

    const iconContainer = container.querySelector(".rounded-full");
    expect(iconContainer).toBeInTheDocument();
  });

  it("should display for Cancelled status", () => {
    renderWithProviders(
      <OrdersEmptyState
        selectedStatus="Cancelled"
        onClearFilter={mockOnClearFilter}
      />,
    );

    expect(
      screen.getByRole("button", { name: /clear filter/i }),
    ).toBeInTheDocument();
  });
});
