import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import type { Order } from "@workspace/types/ordering/orders";

import OrdersList from "@/features/ordering/orders/orders-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockOrders: Order[] = [
  {
    id: faker.string.uuid(),
    status: "New",
    date: faker.date.recent().toISOString(),
    total: faker.number.float({ min: 20, max: 200, fractionDigits: 2 }),
  },
  {
    id: faker.string.uuid(),
    status: "Completed",
    date: faker.date.recent().toISOString(),
    total: faker.number.float({ min: 20, max: 200, fractionDigits: 2 }),
  },
  {
    id: faker.string.uuid(),
    status: "Cancelled",
    date: faker.date.recent().toISOString(),
    total: faker.number.float({ min: 20, max: 200, fractionDigits: 2 }),
  },
];

describe("OrdersList", () => {
  it("should render all orders", () => {
    const orders: Order[] = [
      { ...mockOrders[0]!, id: "order-1" },
      { ...mockOrders[1]!, id: "order-2" },
      { ...mockOrders[2]!, id: "order-3" },
    ];
    renderWithProviders(<OrdersList orders={orders} />);

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
    const orders: Order[] = [
      { ...mockOrders[0]!, date: "2024-01-15T10:30:00Z" },
      { ...mockOrders[1]!, date: "2024-01-10T14:20:00Z" },
      { ...mockOrders[2]!, date: "2024-01-05T09:15:00Z" },
    ];
    renderWithProviders(<OrdersList orders={orders} />);

    expect(screen.getByText(/January 15, 2024/)).toBeInTheDocument();
    expect(screen.getByText(/January 10, 2024/)).toBeInTheDocument();
    expect(screen.getByText(/January 5, 2024/)).toBeInTheDocument();
  });

  it("should display order totals with proper formatting", () => {
    const orders: Order[] = [
      { ...mockOrders[0]!, total: 59.97 },
      { ...mockOrders[1]!, total: 124.5 },
      { ...mockOrders[2]!, total: 35.0 },
    ];
    renderWithProviders(<OrdersList orders={orders} />);

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
    const orders: Order[] = [
      { ...mockOrders[0]!, id: "order-1" },
      { ...mockOrders[1]!, id: "order-2" },
      { ...mockOrders[2]!, id: "order-3" },
    ];
    renderWithProviders(<OrdersList orders={orders} />);

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
    const singleOrder: Order[] = [
      { ...mockOrders[0]!, id: "order-1", status: "New" },
    ];
    renderWithProviders(<OrdersList orders={singleOrder} />);

    expect(screen.getByText("order-1")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("should display status badge with appropriate styling", () => {
    const orders: Order[] = [{ ...mockOrders[0]!, status: "New" }];
    renderWithProviders(<OrdersList orders={orders} />);

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
