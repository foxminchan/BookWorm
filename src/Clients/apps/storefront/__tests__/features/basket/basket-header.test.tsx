import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import BasketHeader from "@/features/basket/basket-header";

import { renderWithProviders } from "../../utils/test-utils";

describe("BasketHeader", () => {
  const mockOnSaveChanges = vi.fn();
  const mockOnClearBasket = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display basket title", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={false}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(screen.getByText("Your Basket")).toBeInTheDocument();
  });

  it("should show save changes button when hasChanges is true", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={true}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(
      screen.getByRole("button", { name: /save changes/i }),
    ).toBeInTheDocument();
  });

  it("should not show save changes button when hasChanges is false", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(screen.queryByText(/save changes/i)).not.toBeInTheDocument();
  });

  it("should show clear basket button when hasItems is true", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(
      screen.getByRole("button", { name: /clear basket/i }),
    ).toBeInTheDocument();
  });

  it("should not show clear basket button when hasItems is false", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={false}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(screen.queryByText(/clear/i)).not.toBeInTheDocument();
  });

  it("should call onSaveChanges when save button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <BasketHeader
        hasChanges={true}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    const saveButton = screen.getByRole("button", { name: /save/i });
    await user.click(saveButton);

    expect(mockOnSaveChanges).toHaveBeenCalledTimes(1);
  });

  it("should show confirmation dialog when clear button is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    const clearButton = screen.getByRole("button", { name: /clear/i });
    await user.click(clearButton);

    expect(screen.getByText("Are you absolutely sure?")).toBeInTheDocument();
  });

  it("should display Check icon when hasChanges is true", () => {
    const { container } = renderWithProviders(
      <BasketHeader
        hasChanges={true}
        hasItems={false}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThan(0);
  });

  it("should display Trash icon when hasItems is true", () => {
    const { container } = renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    const icons = container.querySelectorAll("svg");
    expect(icons.length).toBeGreaterThan(0);
  });

  it("should have serif font for title", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={false}
        hasItems={false}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    const title = screen.getByText("Your Basket");
    expect(title).toHaveClass("font-serif");
  });

  it("should show both buttons when hasChanges and hasItems are true", () => {
    renderWithProviders(
      <BasketHeader
        hasChanges={true}
        hasItems={true}
        onSaveChanges={mockOnSaveChanges}
        onClearBasket={mockOnClearBasket}
      />,
    );

    expect(
      screen.getByRole("button", { name: /save changes/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /clear basket/i }),
    ).toBeInTheDocument();
  });
});
