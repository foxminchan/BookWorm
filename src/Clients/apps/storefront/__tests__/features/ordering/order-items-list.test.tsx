import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import OrderItemsList from "@/features/ordering/order-detail/order-items-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockItems = [
  {
    id: "item-1",
    name: "The Great Gatsby",
    quantity: 2,
    price: 15.99,
  },
  {
    id: "item-2",
    name: "To Kill a Mockingbird",
    quantity: 1,
    price: 12.5,
  },
  {
    id: "item-3",
    name: null,
    quantity: 3,
    price: 10.0,
  },
];

describe("OrderItemsList", () => {
  it("should render order items heading", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("Order Items")).toBeInTheDocument();
  });

  it("should render all items", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("The Great Gatsby")).toBeInTheDocument();
    expect(screen.getByText("To Kill a Mockingbird")).toBeInTheDocument();
  });

  it("should display item quantities", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("Quantity: 2")).toBeInTheDocument();
    expect(screen.getByText("Quantity: 1")).toBeInTheDocument();
    expect(screen.getByText("Quantity: 3")).toBeInTheDocument();
  });

  it("should calculate and display total prices", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    // 2 * $15.99 = $31.98
    expect(screen.getByText("$31.98")).toBeInTheDocument();
    // 1 * $12.50 = $12.50
    expect(screen.getByText("$12.50")).toBeInTheDocument();
    // 3 * $10.00 = $30.00
    expect(screen.getByText("$30.00")).toBeInTheDocument();
  });

  it("should display unit prices", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("$15.99 each")).toBeInTheDocument();
    expect(screen.getByText("$12.50 each")).toBeInTheDocument();
    expect(screen.getByText("$10.00 each")).toBeInTheDocument();
  });

  it("should display 'Unnamed Item' for null names", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("Unnamed Item")).toBeInTheDocument();
  });

  it("should render package icon", () => {
    const { container } = renderWithProviders(
      <OrderItemsList items={mockItems} />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should have serif font for heading", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    const heading = screen.getByText("Order Items");
    expect(heading).toHaveClass("font-serif");
  });

  it("should have hover effects on items", () => {
    const { container } = renderWithProviders(
      <OrderItemsList items={mockItems} />,
    );

    const hoverElements = container.querySelectorAll(
      ".hover\\:bg-secondary\\/20",
    );
    expect(hoverElements.length).toBeGreaterThan(0);
  });

  it("should have rounded corners", () => {
    const { container } = renderWithProviders(
      <OrderItemsList items={mockItems} />,
    );

    const rounded = container.querySelector(".rounded-lg");
    expect(rounded).toBeInTheDocument();
  });

  it("should render empty list", () => {
    const { container } = renderWithProviders(<OrderItemsList items={[]} />);

    const divider = container.querySelector(".divide-y");
    expect(divider).toBeInTheDocument();
    expect(screen.queryByText("The Great Gatsby")).not.toBeInTheDocument();
  });

  it("should handle single item", () => {
    renderWithProviders(<OrderItemsList items={[mockItems[0]!]} />);

    expect(screen.getByText("The Great Gatsby")).toBeInTheDocument();
    expect(screen.queryByText("To Kill a Mockingbird")).not.toBeInTheDocument();
  });
});
