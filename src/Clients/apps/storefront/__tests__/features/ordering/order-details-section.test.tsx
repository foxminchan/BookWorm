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
      <OrderDetailsSection {...defaultProps} />,
      "Shipping Address",
    ],
    ["buyer name", <OrderDetailsSection {...defaultProps} />, "John Doe"],
    [
      "buyer address",
      <OrderDetailsSection {...defaultProps} />,
      "123 Main St, New York, NY 10001",
    ],
    [
      "order status heading",
      <OrderDetailsSection {...defaultProps} />,
      "Order Status",
    ],
    [
      "status badge text",
      <OrderDetailsSection {...defaultProps} />,
      "Completed",
    ],
    [
      "different status text",
      <OrderDetailsSection {...defaultProps} status="New" />,
      "New",
    ],
    [
      "order total heading",
      <OrderDetailsSection {...defaultProps} />,
      "Order Total",
    ],
    [
      "formatted total price",
      <OrderDetailsSection {...defaultProps} />,
      "$59.98",
    ],
    [
      "amount paid label",
      <OrderDetailsSection {...defaultProps} />,
      "Amount Paid",
    ],
    [
      "current status label",
      <OrderDetailsSection {...defaultProps} />,
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
