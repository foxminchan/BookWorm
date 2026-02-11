import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import AccountNavigation from "@/features/account/account-navigation";

import { renderWithProviders } from "../../utils/test-utils";

describe("AccountNavigation", () => {
  it("should render order history link", () => {
    renderWithProviders(<AccountNavigation />);

    expect(screen.getByText("Order History")).toBeInTheDocument();
  });

  it("should have correct link href", () => {
    renderWithProviders(<AccountNavigation />);

    const link = screen.getByRole("link", { name: /order history/i });
    expect(link).toHaveAttribute("href", "/account/orders");
  });

  it("should display chevron icon", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const chevronIcon = container.querySelector("svg");
    expect(chevronIcon).toBeInTheDocument();
  });

  it("should have hover effects", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const linkDiv = container.querySelector(String.raw`.hover\:bg-secondary\/20`);
    expect(linkDiv).toBeInTheDocument();
  });

  it("should have transition animations", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const linkDiv = container.querySelector(".transition-colors");
    expect(linkDiv).toBeInTheDocument();
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const container_div = container.querySelector(".rounded-lg");
    expect(container_div).toBeInTheDocument();
  });

  it("should have border styling", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const border_div = container.querySelector(String.raw`.border-border\/40`);
    expect(border_div).toBeInTheDocument();
  });

  it("should have proper padding", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const paddedDiv = container.querySelector(".p-4");
    expect(paddedDiv).toBeInTheDocument();
  });

  it("should have flex layout for alignment", () => {
    const { container } = renderWithProviders(<AccountNavigation />);

    const flexDiv = container.querySelector(
      ".flex.items-center.justify-between",
    );
    expect(flexDiv).toBeInTheDocument();
  });

  it("should display text with medium font weight", () => {
    renderWithProviders(<AccountNavigation />);

    const text = screen.getByText("Order History");
    expect(text).toHaveClass("font-medium");
  });
});
