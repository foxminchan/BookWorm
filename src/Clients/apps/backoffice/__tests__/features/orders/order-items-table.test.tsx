import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import type { OrderItem } from "@workspace/types/ordering/orders";

import { createMockOrderItem } from "@/__tests__/factories";
import { OrderItemsTable } from "@/features/orders/order-items-table";

const mockOrderItems: OrderItem[] = [
  createMockOrderItem({ quantity: 2, price: 25.5 }),
  createMockOrderItem({ quantity: 1, price: 15.99 }),
  createMockOrderItem({ quantity: 3, price: 10.0 }),
];

const total = mockOrderItems.reduce(
  (sum, item) => sum + item.price * item.quantity,
  0,
);

describe("OrderItemsTable", () => {
  it("renders table title", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    expect(screen.getByText("Order Items")).toBeInTheDocument();
  });

  it("renders table headers", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    expect(screen.getByText("Item Name")).toBeInTheDocument();
    expect(screen.getByText("Quantity")).toBeInTheDocument();
    expect(screen.getByText("Price")).toBeInTheDocument();
    const totalHeaders = screen.getAllByText("Total");
    expect(totalHeaders.length).toBeGreaterThan(0);
  });

  it("displays all order items", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    mockOrderItems.forEach((item) => {
      if (item.name) {
        expect(screen.getByText(item.name)).toBeInTheDocument();
      }
    });
  });

  it("displays item quantities", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    mockOrderItems.forEach((item) => {
      expect(screen.getByText(item.quantity.toString())).toBeInTheDocument();
    });
  });

  it("formats item prices as currency", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    mockOrderItems.forEach((item) => {
      expect(
        screen.getAllByText(formatter.format(item.price)).length,
      ).toBeGreaterThan(0);
    });
  });

  it("calculates and displays item line totals", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    mockOrderItems.forEach((item) => {
      const lineTotal = item.quantity * item.price;
      expect(
        screen.getAllByText(formatter.format(lineTotal)).length,
      ).toBeGreaterThan(0);
    });
  });

  it("displays order total in footer", () => {
    render(<OrderItemsTable items={mockOrderItems} total={total} />);

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    const totals = screen.getAllByText(formatter.format(total));
    expect(totals.length).toBeGreaterThan(0);
  });

  it("handles items with null name", () => {
    const itemsWithNullName: OrderItem[] = [
      createMockOrderItem({ name: null }),
    ];

    const nullNameTotal =
      itemsWithNullName[0]!.quantity * itemsWithNullName[0]!.price;
    render(<OrderItemsTable items={itemsWithNullName} total={nullNameTotal} />);

    expect(screen.getByText("Product")).toBeInTheDocument();
  });

  it("handles empty items array", () => {
    render(<OrderItemsTable items={[]} total={0} />);

    expect(screen.getByText("Order Items")).toBeInTheDocument();
    expect(screen.getByText("Item Name")).toBeInTheDocument();

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });
    expect(screen.getByText(formatter.format(0))).toBeInTheDocument();
  });

  it("renders table with correct structure", () => {
    const { container } = render(
      <OrderItemsTable items={mockOrderItems} total={total} />,
    );

    const table = container.querySelector("table");
    const thead = container.querySelector("thead");
    const tbody = container.querySelector("tbody");
    const tfoot = container.querySelector("tfoot");

    expect(table).toBeInTheDocument();
    expect(thead).toBeInTheDocument();
    expect(tbody).toBeInTheDocument();
    expect(tfoot).toBeInTheDocument();
  });

  it("renders footer with correct colspan", () => {
    const { container } = render(
      <OrderItemsTable items={mockOrderItems} total={total} />,
    );

    const footerCell = container.querySelector("tfoot td[colspan='3']");
    expect(footerCell).toBeInTheDocument();
    expect(footerCell?.textContent).toBe("Total");
  });

  it("applies font-medium to line total cells", () => {
    const { container } = render(
      <OrderItemsTable items={mockOrderItems} total={total} />,
    );

    const lineTotalCells = container.querySelectorAll("tbody td.font-medium");
    expect(lineTotalCells).toHaveLength(mockOrderItems.length);
  });

  it("applies font-bold to footer total", () => {
    const { container } = render(
      <OrderItemsTable items={mockOrderItems} total={total} />,
    );

    const footerTotal = container.querySelector("tfoot td.font-bold");
    expect(footerTotal).toBeInTheDocument();
  });
});
