import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { OrderItem } from "@workspace/types/ordering/orders";

import OrderItemsList from "@/features/ordering/order-detail/order-items-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockItems: OrderItem[] = [
  {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    quantity: faker.number.int({ min: 1, max: 5 }),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
  },
  {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    quantity: faker.number.int({ min: 1, max: 5 }),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
  },
  {
    id: faker.string.uuid(),
    name: null,
    quantity: faker.number.int({ min: 1, max: 5 }),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
  },
];

describe("OrderItemsList", () => {
  it("should render order items heading", () => {
    renderWithProviders(<OrderItemsList items={mockItems} />);

    expect(screen.getByText("Order Items")).toBeInTheDocument();
  });

  it("should render all items", () => {
    const items = [
      { ...mockItems[0]!, name: "The Great Gatsby" },
      { ...mockItems[1]!, name: "To Kill a Mockingbird" },
      mockItems[2]!,
    ];
    renderWithProviders(<OrderItemsList items={items} />);

    expect(screen.getByText("The Great Gatsby")).toBeInTheDocument();
    expect(screen.getByText("To Kill a Mockingbird")).toBeInTheDocument();
  });

  it("should display item quantities", () => {
    const items = [
      { ...mockItems[0]!, quantity: 2 },
      { ...mockItems[1]!, quantity: 1 },
      { ...mockItems[2]!, quantity: 3 },
    ];
    renderWithProviders(<OrderItemsList items={items} />);

    expect(screen.getByText("Quantity: 2")).toBeInTheDocument();
    expect(screen.getByText("Quantity: 1")).toBeInTheDocument();
    expect(screen.getByText("Quantity: 3")).toBeInTheDocument();
  });

  it("should calculate and display total prices", () => {
    const items = [
      { ...mockItems[0]!, quantity: 2, price: 15.99 },
      { ...mockItems[1]!, quantity: 1, price: 12.5 },
      { ...mockItems[2]!, quantity: 3, price: 10 },
    ];
    renderWithProviders(<OrderItemsList items={items} />);

    // 2 * $15.99 = $31.98
    expect(screen.getByText("$31.98")).toBeInTheDocument();
    // 1 * $12.50 = $12.50
    expect(screen.getByText("$12.50")).toBeInTheDocument();
    // 3 * $10.00 = $30.00
    expect(screen.getByText("$30.00")).toBeInTheDocument();
  });

  it("should display unit prices", () => {
    const items = [
      { ...mockItems[0]!, price: 15.99 },
      { ...mockItems[1]!, price: 12.5 },
      { ...mockItems[2]!, price: 10 },
    ];
    renderWithProviders(<OrderItemsList items={items} />);

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
      String.raw`.hover\:bg-secondary\/20`,
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
    const items = [{ ...mockItems[0]!, name: "The Great Gatsby" }];
    renderWithProviders(<OrderItemsList items={items} />);

    expect(screen.getByText("The Great Gatsby")).toBeInTheDocument();
    expect(screen.queryByText("To Kill a Mockingbird")).not.toBeInTheDocument();
  });
});
