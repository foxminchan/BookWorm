import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import OrderSummary from "@/features/ordering/order-detail/order-summary";

import { renderWithProviders } from "../../utils/test-utils";

describe("OrderSummary", () => {
  it("should render order summary title", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    expect(screen.getByText("Order Summary")).toBeInTheDocument();
  });

  it("should display subtotal with correct formatting", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    const amounts = screen.getAllByText("$99.99");
    expect(amounts).toHaveLength(2); // Appears in both subtotal and total
  });

  it("should display subtotal label", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    expect(screen.getByText("Subtotal")).toBeInTheDocument();
  });

  it("should display free shipping", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    expect(screen.getByText("Free")).toBeInTheDocument();
    expect(screen.getByText("Shipping")).toBeInTheDocument();
  });

  it("should display total label", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    const totalLabels = screen.getAllByText("Total");
    expect(totalLabels).toHaveLength(1);
  });

  it("should display total amount matching subtotal", () => {
    renderWithProviders(<OrderSummary total={150.5} />);

    // Should appear twice: once for subtotal, once for total
    const amounts = screen.getAllByText("$150.50");
    expect(amounts).toHaveLength(2);
  });

  it("should format decimal places correctly", () => {
    renderWithProviders(<OrderSummary total={49.9} />);

    const amounts = screen.getAllByText("$49.90");
    expect(amounts).toHaveLength(2);
  });

  it("should format whole numbers with decimal places", () => {
    renderWithProviders(<OrderSummary total={100} />);

    const amounts = screen.getAllByText("$100.00");
    expect(amounts).toHaveLength(2);
  });

  it("should handle zero total", () => {
    renderWithProviders(<OrderSummary total={0} />);

    const amounts = screen.getAllByText("$0.00");
    expect(amounts).toHaveLength(2);
  });

  it("should have proper styling classes", () => {
    const { container } = renderWithProviders(<OrderSummary total={99.99} />);

    const summaryContainer = container.querySelector(".rounded-lg");
    expect(summaryContainer).toBeInTheDocument();
  });

  it("should display shipping in green color", () => {
    renderWithProviders(<OrderSummary total={99.99} />);

    const freeShipping = screen.getByText("Free");
    expect(freeShipping).toHaveClass("text-green-600");
  });

  it("should have border separator between items and total", () => {
    const { container } = renderWithProviders(<OrderSummary total={99.99} />);

    const borderDiv = container.querySelector(".border-t");
    expect(borderDiv).toBeInTheDocument();
  });

  it("should render with hover effect", () => {
    const { container } = renderWithProviders(<OrderSummary total={99.99} />);

    const summaryContainer = container.querySelector(
      String.raw`.hover\:bg-secondary\/20`,
    );
    expect(summaryContainer).toBeInTheDocument();
  });
});
