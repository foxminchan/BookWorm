import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import OrderDetailsSection from "@/features/ordering/checkout/order-details-section";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrderDetailsSection", () => {
  const defaultProps = {
    status: "Completed" as const,
    total: 59.98,
    buyerName: "John Doe",
    buyerAddress: "123 Main St, New York, NY 10001",
  };

  it("should display shipping address heading", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Shipping Address")).toBeInTheDocument();
  });

  it("should display buyer name", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("John Doe")).toBeInTheDocument();
  });

  it("should display buyer address", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(
      screen.getByText("123 Main St, New York, NY 10001"),
    ).toBeInTheDocument();
  });

  it("should display 'No address set' when address is missing", () => {
    renderWithProviders(
      <OrderDetailsSection
        {...defaultProps}
        buyerName={undefined}
        buyerAddress={undefined}
      />,
    );

    expect(screen.getByText("No address set")).toBeInTheDocument();
  });

  it("should display order status heading", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Order Status")).toBeInTheDocument();
  });

  it("should display status badge with status text", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("should display different status values", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} status="New" />);

    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("should display order total heading", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Order Total")).toBeInTheDocument();
  });

  it("should display formatted total price", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("$59.98")).toBeInTheDocument();
  });

  it("should display amount paid label", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Amount Paid")).toBeInTheDocument();
  });

  it("should display current status label", () => {
    renderWithProviders(<OrderDetailsSection {...defaultProps} />);

    expect(screen.getByText("Current Status")).toBeInTheDocument();
  });
});
