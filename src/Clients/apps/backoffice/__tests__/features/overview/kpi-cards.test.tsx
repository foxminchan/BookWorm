import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import type { Order } from "@workspace/types/ordering/orders";

import { createMockOrder } from "@/__tests__/factories";
import { KPICards } from "@/features/overview/kpi-cards";

const mockOrders: Order[] = [
  createMockOrder({ total: 100.5, status: "Completed" }),
  createMockOrder({ total: 250.75, status: "Completed" }),
  createMockOrder({ total: 75.25, status: "New" }),
];

describe("KPICards", () => {
  it("renders loading skeleton when isLoading is true", () => {
    render(
      <KPICards
        orders={[]}
        totalCustomers={0}
        totalBooks={0}
        isLoading={true}
      />,
    );

    expect(screen.getByText("Loading statistics...")).toBeInTheDocument();
  });

  it("calculates and displays total revenue correctly", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    const expectedRevenue = mockOrders.reduce(
      (sum, order) => sum + order.total,
      0,
    );
    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(
      screen.getByText(formatter.format(expectedRevenue)),
    ).toBeInTheDocument();
    expect(screen.getByText("Total Revenue")).toBeInTheDocument();
  });

  it("displays total orders count", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    expect(screen.getByText(mockOrders.length.toString())).toBeInTheDocument();
    expect(screen.getByText("Total Orders")).toBeInTheDocument();
    expect(screen.getByText("50 customers")).toBeInTheDocument();
  });

  it("displays active customers count", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    expect(screen.getByText("50")).toBeInTheDocument();
    expect(screen.getByText("Active Customers")).toBeInTheDocument();
    expect(screen.getByText("Total registered")).toBeInTheDocument();
  });

  it("displays books in catalog count", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    expect(screen.getByText("200")).toBeInTheDocument();
    expect(screen.getByText("Books in Catalog")).toBeInTheDocument();
    expect(screen.getByText("Available titles")).toBeInTheDocument();
  });

  it("handles empty orders array", () => {
    render(
      <KPICards
        orders={[]}
        totalCustomers={25}
        totalBooks={100}
        isLoading={false}
      />,
    );

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(0))).toBeInTheDocument();
    expect(screen.getByText("0")).toBeInTheDocument(); // Total orders
    expect(screen.getByText("25")).toBeInTheDocument(); // Customers
    expect(screen.getByText("100")).toBeInTheDocument(); // Books
  });

  it("handles orders with null or undefined total", () => {
    const ordersWithNullTotal: Order[] = [
      createMockOrder({ total: null as unknown as number }),
      createMockOrder({ total: undefined as unknown as number }),
    ];

    render(
      <KPICards
        orders={ordersWithNullTotal}
        totalCustomers={10}
        totalBooks={50}
        isLoading={false}
      />,
    );

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(screen.getByText(formatter.format(0))).toBeInTheDocument();
  });

  it("displays order count in revenue change text", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    expect(screen.getByText(`${mockOrders.length} orders`)).toBeInTheDocument();
  });

  it("renders all four KPI cards", () => {
    render(
      <KPICards
        orders={mockOrders}
        totalCustomers={50}
        totalBooks={200}
        isLoading={false}
      />,
    );

    expect(screen.getByText("Total Revenue")).toBeInTheDocument();
    expect(screen.getByText("Total Orders")).toBeInTheDocument();
    expect(screen.getByText("Active Customers")).toBeInTheDocument();
    expect(screen.getByText("Books in Catalog")).toBeInTheDocument();
  });
});
