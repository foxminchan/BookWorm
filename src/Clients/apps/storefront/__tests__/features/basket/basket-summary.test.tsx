import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import BasketSummary from "@/features/basket/basket-summary";

import { renderWithProviders } from "../../utils/test-utils";

describe("BasketSummary", () => {
  const defaultProps = {
    subtotal: 100.0,
    shipping: 10.0,
    total: 110.0,
    isCheckingOut: false,
    onCheckout: vi.fn(),
  };

  it("should render order summary with correct values", () => {
    renderWithProviders(<BasketSummary {...defaultProps} />);

    expect(screen.getByText("Order Summary")).toBeInTheDocument();
    expect(screen.getByText("$100.00")).toBeInTheDocument();
    expect(screen.getByText("$10.00")).toBeInTheDocument();
    expect(screen.getByText("$110.00")).toBeInTheDocument();
  });

  it("should display subtotal, shipping, and total labels", () => {
    renderWithProviders(<BasketSummary {...defaultProps} />);

    expect(screen.getByText("Subtotal")).toBeInTheDocument();
    expect(screen.getByText("Shipping")).toBeInTheDocument();
    expect(screen.getByText("Total")).toBeInTheDocument();
  });

  it("should call onCheckout when checkout button is clicked", async () => {
    const user = userEvent.setup();
    const mockCheckout = vi.fn();

    renderWithProviders(
      <BasketSummary {...defaultProps} onCheckout={mockCheckout} />,
    );

    const checkoutButton = screen.getByRole("button", { name: /checkout/i });
    await user.click(checkoutButton);

    expect(mockCheckout).toHaveBeenCalledTimes(1);
  });

  it("should disable checkout button when isCheckingOut is true", () => {
    renderWithProviders(<BasketSummary {...defaultProps} isCheckingOut />);

    const checkoutButton = screen.getByRole("button", {
      name: /processing/i,
    });
    expect(checkoutButton).toBeDisabled();
  });

  it("should show processing text when checking out", () => {
    renderWithProviders(<BasketSummary {...defaultProps} isCheckingOut />);

    expect(screen.getByText(/processing/i)).toBeInTheDocument();
  });

  it("should show checkout text when not checking out", () => {
    renderWithProviders(
      <BasketSummary {...defaultProps} isCheckingOut={false} />,
    );

    expect(
      screen.getByRole("button", { name: /checkout/i }),
    ).toBeInTheDocument();
  });

  it("should display tax notice", () => {
    renderWithProviders(<BasketSummary {...defaultProps} />);

    expect(
      screen.getByText("Taxes calculated at checkout"),
    ).toBeInTheDocument();
  });

  it("should format prices to 2 decimal places", () => {
    const propsWithDecimals = {
      ...defaultProps,
      subtotal: 99.99,
      shipping: 5.5,
      total: 105.49,
    };

    renderWithProviders(<BasketSummary {...propsWithDecimals} />);

    expect(screen.getByText("$99.99")).toBeInTheDocument();
    expect(screen.getByText("$5.50")).toBeInTheDocument();
    expect(screen.getByText("$105.49")).toBeInTheDocument();
  });

  it("should handle zero values correctly", () => {
    const zeroProps = {
      ...defaultProps,
      subtotal: 0,
      shipping: 0,
      total: 0,
    };

    renderWithProviders(<BasketSummary {...zeroProps} />);

    const zeroValues = screen.getAllByText("$0.00");
    expect(zeroValues).toHaveLength(3); // subtotal, shipping, total
  });

  it("should handle large values correctly", () => {
    const largeProps = {
      ...defaultProps,
      subtotal: 9999.99,
      shipping: 50.0,
      total: 10049.99,
    };

    renderWithProviders(<BasketSummary {...largeProps} />);

    expect(screen.getByText("$9999.99")).toBeInTheDocument();
    expect(screen.getByText("$50.00")).toBeInTheDocument();
    expect(screen.getByText("$10049.99")).toBeInTheDocument();
  });

  it("should not call onCheckout when button is disabled", async () => {
    const user = userEvent.setup();
    const mockCheckout = vi.fn();

    renderWithProviders(
      <BasketSummary
        {...defaultProps}
        isCheckingOut
        onCheckout={mockCheckout}
      />,
    );

    const checkoutButton = screen.getByRole("button", {
      name: /processing/i,
    });

    // Try to click disabled button
    await user.click(checkoutButton);

    // Should not be called because button is disabled
    expect(mockCheckout).not.toHaveBeenCalled();
  });

  it("should render with sticky positioning for desktop", () => {
    const { container } = renderWithProviders(
      <BasketSummary {...defaultProps} />,
    );

    const summaryCard = container.querySelector(".sticky");
    expect(summaryCard).toBeInTheDocument();
  });

  it("should handle free shipping", () => {
    const freeShippingProps = {
      ...defaultProps,
      shipping: 0,
      total: 100.0,
    };

    renderWithProviders(<BasketSummary {...freeShippingProps} />);

    const shippingValues = screen.getAllByText("$0.00");
    expect(shippingValues.length).toBeGreaterThan(0);
  });
});
