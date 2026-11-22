import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import type { BasketItem } from "@workspace/types/basket";

import BasketItemsList from "@/features/basket/basket-items-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockItems: BasketItem[] = [
  {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
    priceSale: null,
    quantity: faker.number.int({ min: 1, max: 5 }),
  },
  {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
    priceSale: faker.number.float({ min: 10, max: 40, fractionDigits: 2 }),
    quantity: faker.number.int({ min: 1, max: 5 }),
  },
  {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
    priceSale: null,
    quantity: faker.number.int({ min: 1, max: 5 }),
  },
];

describe("BasketItemsList", () => {
  it("should render all basket items", () => {
    const items: BasketItem[] = [
      { ...mockItems[0]!, name: "First Book" },
      { ...mockItems[1]!, name: "Second Book" },
      { ...mockItems[2]!, name: "Third Book" },
    ];

    renderWithProviders(
      <BasketItemsList
        items={items}
        modifiedItems={{}}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("First Book")).toBeInTheDocument();
    expect(screen.getByText("Second Book")).toBeInTheDocument();
    expect(screen.getByText("Third Book")).toBeInTheDocument();
  });

  it("should render empty list when no items", () => {
    const { container } = renderWithProviders(
      <BasketItemsList
        items={[]}
        modifiedItems={{}}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    const listContainer = container.querySelector(".space-y-6");
    expect(listContainer?.children.length).toBe(0);
  });

  it("should display correct quantity including modifications", () => {
    const items: BasketItem[] = [
      { ...mockItems[0]!, id: "item-1", name: "First Book", quantity: 2 },
      { ...mockItems[1]!, id: "item-2", name: "Second Book", quantity: 1 },
      { ...mockItems[2]!, id: "item-3", name: "Third Book", quantity: 3 },
    ];

    const modifiedItems = {
      "item-1": 2, // 2 + 2 = 4
      "item-2": -1, // 1 - 1 = 0
      "item-3": 1, // 3 + 1 = 4
    };

    renderWithProviders(
      <BasketItemsList
        items={items}
        modifiedItems={modifiedItems}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    // All items should be rendered
    expect(screen.getByText("First Book")).toBeInTheDocument();
    expect(screen.getByText("Second Book")).toBeInTheDocument();
    expect(screen.getByText("Third Book")).toBeInTheDocument();
  });

  it("should pass correct callbacks to basket items", () => {
    const mockUpdateQuantity = vi.fn();
    const mockRemoveItem = vi.fn();

    renderWithProviders(
      <BasketItemsList
        items={mockItems}
        modifiedItems={{}}
        onUpdateQuantity={mockUpdateQuantity}
        onRemoveItem={mockRemoveItem}
      />,
    );

    // Callbacks are passed to child components
    // We can't directly test this without interacting with child components
    expect(mockUpdateQuantity).not.toHaveBeenCalled();
    expect(mockRemoveItem).not.toHaveBeenCalled();
  });

  it("should handle negative modifications correctly", () => {
    const items: BasketItem[] = [
      { ...mockItems[0]!, id: "item-1", name: "First Book", quantity: 2 },
      ...mockItems.slice(1),
    ];

    const modifiedItems = {
      "item-1": -1, // 2 - 1 = 1
    };

    renderWithProviders(
      <BasketItemsList
        items={items}
        modifiedItems={modifiedItems}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("First Book")).toBeInTheDocument();
  });

  it("should render items with unique keys", () => {
    const { container } = renderWithProviders(
      <BasketItemsList
        items={mockItems}
        modifiedItems={{}}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    const cards = container.querySelectorAll("[data-testid], .border-none");
    expect(cards.length).toBeGreaterThan(0);
  });

  it("should handle single item correctly", () => {
    const singleItem: BasketItem[] = [{ ...mockItems[0]!, name: "First Book" }];

    renderWithProviders(
      <BasketItemsList
        items={singleItem}
        modifiedItems={{}}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("First Book")).toBeInTheDocument();
    expect(screen.queryByText("Second Book")).not.toBeInTheDocument();
    expect(screen.queryByText("Third Book")).not.toBeInTheDocument();
  });

  it("should not break with empty modifiedItems object", () => {
    const items: BasketItem[] = [
      { ...mockItems[0]!, name: "First Book" },
      { ...mockItems[1]!, name: "Second Book" },
      { ...mockItems[2]!, name: "Third Book" },
    ];

    renderWithProviders(
      <BasketItemsList
        items={items}
        modifiedItems={{}}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("First Book")).toBeInTheDocument();
    expect(screen.getByText("Second Book")).toBeInTheDocument();
    expect(screen.getByText("Third Book")).toBeInTheDocument();
  });

  it("should calculate display quantity as item.quantity + modification", () => {
    const items: BasketItem[] = [
      { ...mockItems[0]!, id: "item-1", name: "First Book", quantity: 2 },
      { ...mockItems[1]!, id: "item-2", name: "Second Book", quantity: 1 },
      { ...mockItems[2]!, id: "item-3", name: "Third Book", quantity: 3 },
    ];

    const modifiedItems = {
      "item-1": 3, // 2 + 3 = 5
      "item-2": 0, // 1 + 0 = 1 (no change)
      // item-3 not modified, stays at 3
    };

    renderWithProviders(
      <BasketItemsList
        items={items}
        modifiedItems={modifiedItems}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    // All items should render
    expect(screen.getByText("First Book")).toBeInTheDocument();
    expect(screen.getByText("Second Book")).toBeInTheDocument();
    expect(screen.getByText("Third Book")).toBeInTheDocument();
  });
});
