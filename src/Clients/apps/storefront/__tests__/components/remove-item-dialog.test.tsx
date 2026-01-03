import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RemoveItemDialog } from "@/components/remove-item-dialog";

import { renderWithProviders } from "../utils/test-utils";

describe("RemoveItemDialog", () => {
  const mockOnConfirm = vi.fn();
  const mockOnOpenChange = vi.fn();

  const defaultProps = {
    open: true,
    onConfirm: mockOnConfirm,
    onOpenChange: mockOnOpenChange,
    items: [{ id: "1", name: "Test Book" }],
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display dialog when open", () => {
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    expect(
      screen.getByRole("heading", { name: /remove from basket/i }),
    ).toBeInTheDocument();
  });

  it("should display item name in message", () => {
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    expect(screen.getByText(/test book/i)).toBeInTheDocument();
  });

  it("should display cancel button", () => {
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /keep item/i }),
    ).toBeInTheDocument();
  });

  it("should display remove button", () => {
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    expect(screen.getByRole("button", { name: /remove/i })).toBeInTheDocument();
  });

  it("should call onOpenChange when cancel button clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    const cancelButton = screen.getByRole("button", { name: /keep item/i });
    await user.click(cancelButton);

    expect(mockOnOpenChange).toHaveBeenCalledWith(false);
  });

  it("should call onConfirm when remove button clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<RemoveItemDialog {...defaultProps} />);

    const removeButton = screen.getByRole("button", { name: /^remove$/i });
    await user.click(removeButton);

    expect(mockOnConfirm).toHaveBeenCalled();
  });

  it("should not render when closed", () => {
    renderWithProviders(<RemoveItemDialog {...defaultProps} open={false} />);

    expect(
      screen.queryByRole("heading", { name: /remove from basket/i }),
    ).not.toBeInTheDocument();
  });
});
