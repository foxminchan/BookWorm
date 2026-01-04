import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import type { BasketItem as BasketItemType } from "@workspace/types/basket";

import BasketItem from "@/features/basket/basket-item";

import { renderWithProviders } from "../../utils/test-utils";

const mockItem: BasketItemType = {
  id: faker.string.uuid(),
  name: faker.commerce.productName(),
  price: faker.number.float({ min: 10, max: 50, fractionDigits: 2 }),
  priceSale: null,
  quantity: faker.number.int({ min: 1, max: 5 }),
};

describe("BasketItem", () => {
  it("should render basket item with correct details", () => {
    const mockUpdateQuantity = vi.fn();
    const mockRemoveItem = vi.fn();
    const item = { ...mockItem, name: "Test Book", price: 29.99 };

    renderWithProviders(
      <BasketItem
        item={item}
        displayQuantity={2}
        onUpdateQuantity={mockUpdateQuantity}
        onRemoveItem={mockRemoveItem}
      />,
    );

    expect(screen.getByText("Test Book")).toBeInTheDocument();
    expect(screen.getByText("Hardcover")).toBeInTheDocument();
    expect(screen.getByText("$59.98")).toBeInTheDocument(); // 29.99 * 2
  });

  it("should display sale price when available", () => {
    const itemWithSale: BasketItemType = {
      ...mockItem,
      price: 29.99,
      priceSale: 19.99,
    };

    renderWithProviders(
      <BasketItem
        item={itemWithSale}
        displayQuantity={2}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    // Sale price
    expect(screen.getByText("$39.98")).toBeInTheDocument(); // 19.99 * 2
    // Original price with strikethrough
    expect(screen.getByText("$59.98")).toBeInTheDocument(); // 29.99 * 2
  });

  it("should call onUpdateQuantity when increasing quantity", async () => {
    const user = userEvent.setup();
    const mockUpdateQuantity = vi.fn();
    const item = { ...mockItem, id: "item-1" };

    renderWithProviders(
      <BasketItem
        item={item}
        displayQuantity={2}
        onUpdateQuantity={mockUpdateQuantity}
        onRemoveItem={vi.fn()}
      />,
    );

    const increaseButton = screen.getByRole("button", { name: /increase/i });
    await user.click(increaseButton);

    expect(mockUpdateQuantity).toHaveBeenCalledWith("item-1", 1);
  });

  it("should call onUpdateQuantity when decreasing quantity", async () => {
    const user = userEvent.setup();
    const mockUpdateQuantity = vi.fn();
    const item = { ...mockItem, id: "item-1" };

    renderWithProviders(
      <BasketItem
        item={item}
        displayQuantity={2}
        onUpdateQuantity={mockUpdateQuantity}
        onRemoveItem={vi.fn()}
      />,
    );

    const decreaseButton = screen.getByRole("button", { name: /decrease/i });
    await user.click(decreaseButton);

    expect(mockUpdateQuantity).toHaveBeenCalledWith("item-1", -1);
  });

  it("should show remove dialog when trash button is clicked", async () => {
    const user = userEvent.setup();

    renderWithProviders(
      <BasketItem
        item={mockItem}
        displayQuantity={2}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    const trashButton = screen.getByRole("button", {
      name: /remove .* from basket/i,
    });
    await user.click(trashButton);

    // Dialog should appear (alertdialog role, not dialog)
    expect(
      screen.getByRole("alertdialog", { name: /remove from basket/i }),
    ).toBeInTheDocument();
  });

  it("should call onRemoveItem when confirming removal", async () => {
    const user = userEvent.setup();
    const mockRemoveItem = vi.fn();
    const item = { ...mockItem, id: "item-1" };

    renderWithProviders(
      <BasketItem
        item={item}
        displayQuantity={2}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={mockRemoveItem}
      />,
    );

    // Click trash button to open dialog
    const trashButton = screen.getByRole("button", {
      name: /remove .* from basket/i,
    });
    await user.click(trashButton);

    // Find and click confirm button in dialog
    const confirmButton = screen.getByRole("button", { name: /remove/i });
    await user.click(confirmButton);

    expect(mockRemoveItem).toHaveBeenCalledWith("item-1");
  });

  it("should display correct total for modified quantity", () => {
    const item = { ...mockItem, price: 29.99 };

    renderWithProviders(
      <BasketItem
        item={item}
        displayQuantity={5}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("$149.95")).toBeInTheDocument(); // 29.99 * 5
  });

  it("should handle zero quantity edge case", () => {
    renderWithProviders(
      <BasketItem
        item={mockItem}
        displayQuantity={0}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    expect(screen.getByText("$0.00")).toBeInTheDocument();
  });

  it("should calculate sale price correctly with modified quantity", () => {
    const itemWithSale: BasketItemType = {
      ...mockItem,
      price: 29.99,
      priceSale: 24.99,
      quantity: 1,
    };

    renderWithProviders(
      <BasketItem
        item={itemWithSale}
        displayQuantity={3}
        onUpdateQuantity={vi.fn()}
        onRemoveItem={vi.fn()}
      />,
    );

    // Sale price: 24.99 * 3 = 74.97
    expect(screen.getByText("$74.97")).toBeInTheDocument();
    // Original price: 29.99 * 3 = 89.97
    expect(screen.getByText("$89.97")).toBeInTheDocument();
  });
});
