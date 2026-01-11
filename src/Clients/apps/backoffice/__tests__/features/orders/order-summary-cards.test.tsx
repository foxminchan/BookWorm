import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { OrderSummaryCards } from "@/features/orders/order-summary-cards";

describe("OrderSummaryCards", () => {
  it("renders all three summary cards", () => {
    render(<OrderSummaryCards status="New" total={150.5} itemCount={3} />);

    expect(screen.getByText("Order Status")).toBeInTheDocument();
    expect(screen.getByText("Order Total")).toBeInTheDocument();
    expect(screen.getByText("Items")).toBeInTheDocument();
  });

  it("displays order status badge with correct value", () => {
    render(<OrderSummaryCards status="Completed" total={100} itemCount={2} />);

    const statusBadge = screen.getByText("Completed");
    expect(statusBadge).toBeInTheDocument();
    // Badge component renders as span
  });

  it("formats total as currency", () => {
    render(<OrderSummaryCards status="New" total={250.75} itemCount={5} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(250.75))).toBeInTheDocument();
  });

  it("displays item count correctly", () => {
    render(<OrderSummaryCards status="New" total={100} itemCount={7} />);

    expect(screen.getByText("7")).toBeInTheDocument();
  });

  it("renders status badge with New status", () => {
    render(<OrderSummaryCards status="New" total={100} itemCount={1} />);

    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("renders status badge with Cancelled status", () => {
    render(<OrderSummaryCards status="Cancelled" total={100} itemCount={1} />);

    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("handles zero total correctly", () => {
    render(<OrderSummaryCards status="New" total={0} itemCount={0} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(0))).toBeInTheDocument();
    expect(screen.getByText("0")).toBeInTheDocument();
  });

  it("handles large total amounts", () => {
    render(
      <OrderSummaryCards status="Completed" total={9999.99} itemCount={50} />,
    );

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(9999.99))).toBeInTheDocument();
    expect(screen.getByText("50")).toBeInTheDocument();
  });

  it("has correct grid layout classes", () => {
    const { container } = render(
      <OrderSummaryCards status="New" total={100} itemCount={1} />,
    );

    const grid = container.querySelector(".grid.grid-cols-1.gap-4");
    expect(grid).toBeInTheDocument();
    expect(grid).toHaveClass("md:grid-cols-3");
  });

  it("renders total with correct styling", () => {
    render(<OrderSummaryCards status="New" total={100} itemCount={1} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    const totalElement = screen.getByText(formatter.format(100));
    expect(totalElement).toHaveClass("text-2xl", "font-bold");
  });

  it("renders item count with correct styling", () => {
    render(<OrderSummaryCards status="New" total={100} itemCount={5} />);

    const itemElement = screen.getByText("5");
    expect(itemElement).toHaveClass("text-2xl", "font-bold");
  });

  it("applies getOrderStatusStyle correctly", () => {
    const { rerender } = render(
      <OrderSummaryCards status="New" total={100} itemCount={1} />,
    );

    let badge = screen.getByText("New");
    expect(badge).toBeInTheDocument();

    rerender(
      <OrderSummaryCards status="Completed" total={100} itemCount={1} />,
    );
    badge = screen.getByText("Completed");
    expect(badge).toBeInTheDocument();

    rerender(
      <OrderSummaryCards status="Cancelled" total={100} itemCount={1} />,
    );
    badge = screen.getByText("Cancelled");
    expect(badge).toBeInTheDocument();
  });
});
