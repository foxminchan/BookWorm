import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { userEvent } from "@/__tests__/utils/test-utils";
import { SimpleTable } from "@/components/simple-table";

type TestItem = {
  id: string;
  name: string | null;
};

describe("SimpleTable", () => {
  const mockOnUpdate = vi.fn();
  const mockOnDelete = vi.fn();

  const defaultProps = {
    title: "Test Table",
    description: "Test Description",
    items: [
      { id: "1", name: "Item 1" },
      { id: "2", name: "Item 2" },
      { id: "3", name: "Item 3" },
    ] as TestItem[],
    isLoading: false,
    onUpdate: mockOnUpdate,
    onDelete: mockOnDelete,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnUpdate.mockResolvedValue(undefined);
    mockOnDelete.mockResolvedValue(undefined);
  });

  it("should render table with title and description", () => {
    render(<SimpleTable {...defaultProps} />);

    expect(screen.getByText("Test Table")).toBeInTheDocument();
    expect(screen.getAllByText("Test Description")[0]).toBeInTheDocument();
  });

  it("should render all items in the table", () => {
    render(<SimpleTable {...defaultProps} />);

    expect(screen.getByText("Item 1")).toBeInTheDocument();
    expect(screen.getByText("Item 2")).toBeInTheDocument();
    expect(screen.getByText("Item 3")).toBeInTheDocument();
  });

  it("should show loading skeleton when isLoading is true", () => {
    render(<SimpleTable {...defaultProps} isLoading={true} />);

    const status = screen.getByRole("status");
    expect(status).toBeInTheDocument();
    expect(screen.getByText("Loading table...")).toBeInTheDocument();
  });

  it("should show edit input when clicking edit button", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    const input = screen.getByDisplayValue("Item 1");
    expect(input).toBeInTheDocument();
  });

  it("should call onUpdate when saving edited item", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    // Click edit button
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    // Update input value
    const input = screen.getByDisplayValue("Item 1");
    await user.clear(input);
    await user.type(input, "Updated Item 1");

    // Click save button
    const saveButton = screen.getByRole("button", { name: /save/i });
    await user.click(saveButton);

    await waitFor(() => {
      expect(mockOnUpdate).toHaveBeenCalledWith("1", "Updated Item 1");
    });
  });

  it("should cancel editing when clicking cancel button", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    // Click edit button
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    // Verify input is shown
    expect(screen.getByDisplayValue("Item 1")).toBeInTheDocument();

    // Click cancel button
    const cancelButton = screen.getByRole("button", { name: /cancel/i });
    await user.click(cancelButton);

    // Verify input is hidden and original text is shown
    expect(screen.queryByDisplayValue("Item 1")).not.toBeInTheDocument();
    expect(screen.getByText("Item 1")).toBeInTheDocument();
  });

  it("should show confirm dialog when clicking delete button", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    const deleteButtons = screen.getAllByRole("button", { name: /delete/i });
    await user.click(deleteButtons[0]!);

    expect(
      screen.getByText(/are you sure you want to delete/i),
    ).toBeInTheDocument();
  });

  it("should call onDelete when confirming delete", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    // Click delete button
    const deleteButtons = screen.getAllByRole("button", { name: /delete/i });
    await user.click(deleteButtons[0]!);

    // Click confirm button in dialog
    const confirmButton = screen.getByRole("button", { name: /^delete$/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mockOnDelete).toHaveBeenCalledWith("1");
    });
  });

  it("should show empty state when no items", () => {
    render(<SimpleTable {...defaultProps} items={[]} />);

    expect(screen.getByText(/no data found/i)).toBeInTheDocument();
  });

  it("should render row numbers starting from 1", () => {
    render(<SimpleTable {...defaultProps} />);

    expect(screen.getByText("1")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("should disable save button during update", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    // Click edit button
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    // Save button should be enabled when value is not empty
    const saveButton = screen.getByRole("button", { name: /save/i });
    expect(saveButton).toBeEnabled();
  });

  it("should handle items with null names", () => {
    const itemsWithNull = [
      { id: "1", name: null },
      { id: "2", name: "Item 2" },
    ] as TestItem[];

    render(<SimpleTable {...defaultProps} items={itemsWithNull} />);

    expect(screen.getByText("Item 2")).toBeInTheDocument();
  });

  it("should only save non-empty names", async () => {
    const user = userEvent.setup();
    render(<SimpleTable {...defaultProps} />);

    // Click edit button
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    // Clear input
    const input = screen.getByDisplayValue("Item 1");
    await user.clear(input);

    // Click save button
    const saveButton = screen.getByRole("button", { name: /save/i });
    await user.click(saveButton);

    // onUpdate should not be called for empty string
    expect(mockOnUpdate).not.toHaveBeenCalled();
  });
});
