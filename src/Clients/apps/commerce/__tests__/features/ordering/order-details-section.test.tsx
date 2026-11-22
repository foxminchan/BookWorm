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

  it.each([
    [
      "Shipping Address heading",
      <OrderDetailsSection key="Shipping Address heading" {...defaultProps} />,
      "Shipping Address",
    ],
    [
      "buyer name",
      <OrderDetailsSection key="buyer name" {...defaultProps} />,
      "John Doe",
    ],
    [
      "buyer address",
      <OrderDetailsSection key="buyer address" {...defaultProps} />,
      "123 Main St, New York, NY 10001",
    ],
    [
      "order status heading",
      <OrderDetailsSection key="order status heading" {...defaultProps} />,
      "Order Status",
    ],
    [
      "status badge text",
      <OrderDetailsSection key="status badge text" {...defaultProps} />,
      "Completed",
    ],
    [
      "different status text",
      <OrderDetailsSection
        key="different status text"
        {...defaultProps}
        status="New"
      />,
      "New",
    ],
    [
      "order total heading",
      <OrderDetailsSection key="order total heading" {...defaultProps} />,
      "Order Total",
    ],
    [
      "formatted total price",
      <OrderDetailsSection key="formatted total price" {...defaultProps} />,
      "$59.98",
    ],
    [
      "amount paid label",
      <OrderDetailsSection key="amount paid label" {...defaultProps} />,
      "Amount Paid",
    ],
    [
      "current status label",
      <OrderDetailsSection key="current status label" {...defaultProps} />,
      "Current Status",
    ],
  ])("should display %s", (_label, element, text) => {
    renderWithProviders(element);

    expect(screen.getByText(text)).toBeInTheDocument();
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
});
