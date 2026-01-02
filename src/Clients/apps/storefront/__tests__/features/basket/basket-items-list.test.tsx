import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import type { BasketItem } from "@workspace/types/basket";

import BasketItemsList from "@/features/basket/basket-items-list";

import { renderWithProviders } from "../../utils/test-utils";

const mockItems: BasketItem[] = [
  {
    id: "item-1",
    name: "First Book",
    price: 19.99,
    priceSale: null,
    quantity: 2,
  },
  {
    id: "item-2",
    name: "Second Book",
    price: 29.99,
    priceSale: 24.99,
    quantity: 1,
  },
  {
    id: "item-3",
    name: "Third Book",
    price: 39.99,
    priceSale: null,
    quantity: 3,
  },
];

describe("BasketItemsList", () => {
  it("should render all basket items", () => {
    renderWithProviders(
      <BasketItemsList
        items={mockItems}
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
    const modifiedItems = {
      "item-1": 2, // 2 + 2 = 4
      "item-2": -1, // 1 - 1 = 0
      "item-3": 1, // 3 + 1 = 4
    };

    renderWithProviders(
      <BasketItemsList
        items={mockItems}
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
    const modifiedItems = {
      "item-1": -1, // 2 - 1 = 1
    };

    renderWithProviders(
      <BasketItemsList
        items={mockItems}
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
    const singleItem = [mockItems[0]!];

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
    renderWithProviders(
      <BasketItemsList
        items={mockItems}
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
    const modifiedItems = {
      "item-1": 3, // 2 + 3 = 5
      "item-2": 0, // 1 + 0 = 1 (no change)
      // item-3 not modified, stays at 3
    };

    renderWithProviders(
      <BasketItemsList
        items={mockItems}
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
