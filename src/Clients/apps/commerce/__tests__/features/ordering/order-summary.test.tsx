import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import OrderSummary from "@/features/ordering/order-detail/order-summary";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrderSummary", () => {
  it("should render order summary title", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    expect(screen.getByText("Order Summary")).toBeInTheDocument();
  });

  it.each([
    ["subtotal formatting", 99.99, "$99.99", 2],
    ["total amount matching subtotal", 150.5, "$150.50", 2],
    ["decimal places", 49.9, "$49.90", 2],
    ["whole numbers with decimal places", 100, "$100.00", 2],
    ["zero total", 0, "$0.00", 2],
  ])("should display %s", (_label, total, amount, expectedCount) => {
    renderWithProviders(<OrderSummary total={total} />);

    expect(screen.getAllByText(amount)).toHaveLength(expectedCount);
  });

  it.each([
    ["subtotal label", "Subtotal"],
    ["free shipping", "Free"],
    ["shipping label", "Shipping"],
    ["total label", "Total"],
  ])("should display %s", (_label, text) => {
    renderWithProviders(<OrderSummary total={99.99} />);

    expect(screen.getByText(text)).toBeInTheDocument();
  });

  it.each<[string, string]>([
    ["proper styling classes", ".rounded-lg"],
    ["border separator", ".border-t"],
    ["hover effect", String.raw`.hover\:bg-secondary\/20`],
  ])("should have %s", (_label, selector) => {
    const { container } = renderWithProviders(<OrderSummary total={99.99} />);

    const summaryContainer = container.querySelector(selector);
    expect(summaryContainer).toBeInTheDocument();
  });

  it.each<[string, string, string[]]>([
    ["green shipping color", "Free", ["text-green-600"]],
  ])("should have %s", (_label, text, classNames) => {
    renderWithProviders(<OrderSummary total={99.99} />);

    const element = screen.getByText(text);
    expect(element).toHaveClass(...classNames);
  });
});
