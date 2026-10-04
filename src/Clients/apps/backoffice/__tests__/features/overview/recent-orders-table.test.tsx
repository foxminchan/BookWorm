import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import type { Order } from "@workspace/types/ordering/orders";

import { createMockOrder } from "@/__tests__/factories";
import { RecentOrdersTable } from "@/features/overview/recent-orders-table";

const mockOrders: Order[] = [
  createMockOrder({ total: 100.5, status: "New" }),
  createMockOrder({ total: 250.75, status: "Completed" }),
  createMockOrder({ total: 75.25, status: "Cancelled" }),
];

describe("RecentOrdersTable", () => {
  it("renders loading skeleton when isLoading is true", () => {
    render(<RecentOrdersTable orders={[]} isLoading={true} />);

    expect(screen.getByText("Loading recent orders...")).toBeInTheDocument();
  });

  it("renders table title and description", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    expect(screen.getByText("Recent Orders")).toBeInTheDocument();
    expect(screen.getByText("Last 5 orders placed")).toBeInTheDocument();
  });

  it("renders table headers", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    expect(screen.getByText("Order ID")).toBeInTheDocument();
    expect(screen.getByText("Amount")).toBeInTheDocument();
    expect(screen.getByText("Status")).toBeInTheDocument();
    expect(screen.getByText("Date")).toBeInTheDocument();
  });

  it("displays order ID truncated to first 8 characters", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    mockOrders.forEach((order) => {
      const truncatedId = `#${order.id.substring(0, 8)}`;
      expect(screen.getByText(truncatedId)).toBeInTheDocument();
    });
  });

  it("formats amount as currency", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    mockOrders.forEach((order) => {
      expect(
        screen.getByText(formatter.format(order.total)),
      ).toBeInTheDocument();
    });
  });

  it("displays order status as badge", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("formats date correctly", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    mockOrders.forEach((order) => {
      const date = new Date(order.date);
      const formattedDate = date.toLocaleDateString("en-US", {
        month: "short",
        day: "2-digit",
        year: "numeric",
      });
      expect(screen.getByText(formattedDate)).toBeInTheDocument();
    });
  });

  it("displays the first 5 orders in the backend ordering", () => {
    const manyOrders: Order[] = Array.from({ length: 10 }, (_, i) => ({
      ...createMockOrder({ total: 100, status: "New" }),
      id: `order-${i}`,
      date: `2024-01-${(10 - i).toString().padStart(2, "0")}T10:00:00Z`,
    }));

    render(<RecentOrdersTable orders={manyOrders} isLoading={false} />);

    // Backend sorts newest first; keep indices 0-4.
    expect(screen.getByText("#order-0")).toBeInTheDocument();
    expect(screen.getByText("#order-1")).toBeInTheDocument();
    expect(screen.getByText("#order-2")).toBeInTheDocument();
    expect(screen.getByText("#order-3")).toBeInTheDocument();
    expect(screen.getByText("#order-4")).toBeInTheDocument();

    // Older orders should not be visible
    expect(screen.queryByText("#order-5")).not.toBeInTheDocument();
    expect(screen.queryByText("#order-9")).not.toBeInTheDocument();
  });

  it("displays empty state when no orders", () => {
    render(<RecentOrdersTable orders={[]} isLoading={false} />);

    expect(screen.getByText("No orders found.")).toBeInTheDocument();
  });

  it("handles orders with null total", () => {
    const ordersWithNullTotal: Order[] = [
      createMockOrder({ total: null as unknown as number }),
    ];

    render(
      <RecentOrdersTable orders={ordersWithNullTotal} isLoading={false} />,
    );

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(0))).toBeInTheDocument();
  });

  it("renders table with correct structure", () => {
    const { container } = render(
      <RecentOrdersTable orders={mockOrders} isLoading={false} />,
    );

    const table = container.querySelector("table");
    const thead = container.querySelector("thead");
    const tbody = container.querySelector("tbody");

    expect(table).toBeInTheDocument();
    expect(thead).toBeInTheDocument();
    expect(tbody).toBeInTheDocument();
  });

  it("has proper accessibility attributes", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    const headers = screen.getAllByRole("columnheader");
    expect(headers).toHaveLength(4);

    headers.forEach((header) => {
      expect(header).toHaveAttribute("scope", "col");
    });
  });

  it("renders correct number of rows", () => {
    render(<RecentOrdersTable orders={mockOrders} isLoading={false} />);

    const rows = screen.getAllByRole("row");
    // 1 header row + mockOrders.length data rows
    expect(rows).toHaveLength(mockOrders.length + 1);
  });
});
