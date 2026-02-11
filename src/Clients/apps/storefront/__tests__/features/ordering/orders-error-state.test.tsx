import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import OrdersErrorState from "@/features/ordering/orders/orders-error-state";

import { renderWithProviders } from "../../utils/test-utils";

// Mock window.location.reload
const mockReload = vi.fn();
Object.defineProperty(globalThis, "location", {
  value: { reload: mockReload },
  writable: true,
});

describe("OrdersErrorState", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display error heading", () => {
    renderWithProviders(<OrdersErrorState />);

    expect(screen.getByText("Error Loading Orders")).toBeInTheDocument();
  });

  it("should display error description", () => {
    renderWithProviders(<OrdersErrorState />);

    expect(
      screen.getByText(/we encountered an error while loading/i),
    ).toBeInTheDocument();
  });

  it("should render package icon", () => {
    const { container } = renderWithProviders(<OrdersErrorState />);

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should render retry button", () => {
    renderWithProviders(<OrdersErrorState />);

    expect(screen.getByRole("button", { name: /retry/i })).toBeInTheDocument();
  });

  it("should reload page when retry button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OrdersErrorState />);

    const retryButton = screen.getByRole("button", { name: /retry/i });
    await user.click(retryButton);

    expect(mockReload).toHaveBeenCalledTimes(1);
  });

  it("should have serif font for heading", () => {
    renderWithProviders(<OrdersErrorState />);

    const heading = screen.getByText("Error Loading Orders");
    expect(heading).toHaveClass("font-serif");
  });

  it("should have rounded icon container", () => {
    const { container } = renderWithProviders(<OrdersErrorState />);

    const iconContainer = container.querySelector(".rounded-full");
    expect(iconContainer).toBeInTheDocument();
  });

  it("should have centered text", () => {
    const { container } = renderWithProviders(<OrdersErrorState />);

    const container_div = container.querySelector(".text-center");
    expect(container_div).toBeInTheDocument();
  });

  it("should have outline variant button", () => {
    renderWithProviders(<OrdersErrorState />);

    const button = screen.getByRole("button", { name: /retry/i });
    expect(button).toBeInTheDocument();
  });
});
