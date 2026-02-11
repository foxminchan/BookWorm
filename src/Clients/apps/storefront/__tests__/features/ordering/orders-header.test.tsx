import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import OrdersHeader from "@/features/ordering/orders/orders-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrdersHeader", () => {
  const mockOnStatusChange = vi.fn();

  const defaultProps = {
    selectedStatus: "All" as const,
    onStatusChange: mockOnStatusChange,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display page title", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    expect(screen.getByText("Order History")).toBeInTheDocument();
  });

  it("should display page description", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    expect(
      screen.getByText("Track and manage all your purchases"),
    ).toBeInTheDocument();
  });

  it("should display back button", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    expect(
      screen.getByRole("link", { name: /back to account/i }),
    ).toBeInTheDocument();
  });

  it("should have link to account", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    const link = screen.getByRole("link", { name: /back to account/i });
    expect(link).toHaveAttribute("href", "/account");
  });

  it("should display filter label", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    expect(screen.getByText("Filter Orders")).toBeInTheDocument();
  });

  it("should display status select", () => {
    renderWithProviders(<OrdersHeader {...defaultProps} />);

    expect(screen.getByRole("combobox")).toBeInTheDocument();
  });

  it("should show selected status", () => {
    renderWithProviders(
      <OrdersHeader {...defaultProps} selectedStatus="Completed" />,
    );

    expect(screen.getByText("Completed")).toBeInTheDocument();
  });
});
