import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import type { OrderStatus } from "@workspace/types/ordering/orders";

import OrdersList from "@/features/ordering/orders/orders-list";

import { renderWithProviders } from "../../utils/test-utils";

type Order = {
  id: string;
  status: OrderStatus;
  date: string;
  total: number;
};

const mockOrders: Order[] = [
  {
    id: "order-1",
    status: "New",
    date: "2024-01-15T10:30:00Z",
    total: 59.97,
  },
  {
    id: "order-2",
    status: "Completed",
    date: "2024-01-10T14:20:00Z",
    total: 124.5,
  },
  {
    id: "order-3",
    status: "Cancelled",
    date: "2024-01-05T09:15:00Z",
    total: 35.0,
  },
];

describe("OrdersList", () => {
  it("should render all orders", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    expect(screen.getByText("order-1")).toBeInTheDocument();
    expect(screen.getByText("order-2")).toBeInTheDocument();
    expect(screen.getByText("order-3")).toBeInTheDocument();
  });

  it("should display order status badges", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("should format order dates correctly", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    expect(screen.getByText(/January 15, 2024/)).toBeInTheDocument();
    expect(screen.getByText(/January 10, 2024/)).toBeInTheDocument();
    expect(screen.getByText(/January 5, 2024/)).toBeInTheDocument();
  });

  it("should display order totals with proper formatting", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    expect(screen.getByText("$59.97")).toBeInTheDocument();
    expect(screen.getByText("$124.50")).toBeInTheDocument();
    expect(screen.getByText("$35.00")).toBeInTheDocument();
  });

  it("should render view details buttons for all orders", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    const viewDetailsButtons = screen.getAllByText("View Details");
    expect(viewDetailsButtons).toHaveLength(3);
  });

  it("should render links to order detail pages", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    const links = screen.getAllByRole("link");
    expect(links[0]).toHaveAttribute("href", "/account/orders/order-1");
    expect(links[1]).toHaveAttribute("href", "/account/orders/order-2");
    expect(links[2]).toHaveAttribute("href", "/account/orders/order-3");
  });

  it("should display order ID labels", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    const orderIdLabels = screen.getAllByText("Order ID");
    expect(orderIdLabels).toHaveLength(3);
  });

  it("should display order date labels", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    const orderDateLabels = screen.getAllByText("Order Date");
    expect(orderDateLabels).toHaveLength(3);
  });

  it("should display total amount labels", () => {
    renderWithProviders(<OrdersList orders={mockOrders} />);

    const totalAmountLabels = screen.getAllByText("Total Amount");
    expect(totalAmountLabels).toHaveLength(3);
  });

  it("should render empty list when no orders provided", () => {
    const { container } = renderWithProviders(<OrdersList orders={[]} />);

    const orderItems = container.querySelectorAll("a");
    expect(orderItems).toHaveLength(0);
  });

  it("should handle single order", () => {
    const singleOrder = [mockOrders[0]!];
    renderWithProviders(<OrdersList orders={singleOrder} />);

    expect(screen.getByText("order-1")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("should display status badge with appropriate styling", () => {
    renderWithProviders(<OrdersList orders={[mockOrders[0]!]} />);

    const badge = screen.getByText("New");
    expect(badge).toBeInTheDocument();
  });

  it("should have hover effects on order items", () => {
    const { container } = renderWithProviders(
      <OrdersList orders={[mockOrders[0]!]} />,
    );

    const orderItem = container.querySelector(".group");
    expect(orderItem).toHaveClass("hover:bg-secondary/20");
  });

  it("should render chevron icons in view details buttons", () => {
    const { container } = renderWithProviders(
      <OrdersList orders={[mockOrders[0]!]} />,
    );

    const chevronIcon = container.querySelector("svg");
    expect(chevronIcon).toBeInTheDocument();
  });
});
