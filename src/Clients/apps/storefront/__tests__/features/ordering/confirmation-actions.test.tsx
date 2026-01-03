import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ConfirmationActions from "@/features/ordering/checkout/confirmation-actions";

import { renderWithProviders } from "../../utils/test-utils";

describe("ConfirmationActions", () => {
  const orderId = "abc123def456";

  it("should display continue shopping button", () => {
    renderWithProviders(<ConfirmationActions orderId={orderId} />);

    expect(
      screen.getByRole("link", { name: /continue shopping/i }),
    ).toBeInTheDocument();
  });

  it("should display view order button", () => {
    renderWithProviders(<ConfirmationActions orderId={orderId} />);

    expect(
      screen.getByRole("link", { name: /view order/i }),
    ).toBeInTheDocument();
  });

  it("should link to shop page", () => {
    renderWithProviders(<ConfirmationActions orderId={orderId} />);

    const shopLink = screen.getByRole("link", { name: /continue shopping/i });
    expect(shopLink).toHaveAttribute("href", "/shop");
  });

  it("should link to order detail page with correct orderId", () => {
    renderWithProviders(<ConfirmationActions orderId={orderId} />);

    const orderLink = screen.getByRole("link", { name: /view order/i });
    expect(orderLink).toHaveAttribute("href", `/account/orders/${orderId}`);
  });

  it("should display arrow icon on continue shopping button", () => {
    const { container } = renderWithProviders(
      <ConfirmationActions orderId={orderId} />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThanOrEqual(1);
  });
});
