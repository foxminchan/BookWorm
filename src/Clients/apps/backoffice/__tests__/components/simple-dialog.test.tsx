import { describe, expect, it, vi } from "vitest";

import {
  renderWithProviders,
  screen,
  userEvent,
} from "@/__tests__/utils/test-utils";
import { SimpleDialog } from "@/components/simple-dialog";

describe("SimpleDialog", () => {
  it("should render dialog with title and description", () => {
    renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={() => {}}
        title="Test Dialog"
        description="Test description"
        placeholder="Enter value"
        onSubmit={vi.fn()}
      />,
    );

    expect(screen.getByText("Test Dialog")).toBeInTheDocument();
    expect(screen.getByText("Test description")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter value")).toBeInTheDocument();
  });

  it("should not render when closed", () => {
    const { container } = renderWithProviders(
      <SimpleDialog
        open={false}
        onOpenChange={() => {}}
        title="Test Dialog"
        description="Test description"
        placeholder="Enter value"
        onSubmit={vi.fn()}
      />,
    );

    expect(container.querySelector('[role="dialog"]')).not.toBeInTheDocument();
  });

  it("should call onSubmit when form is submitted with value", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    const onOpenChange = vi.fn();

    renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={onOpenChange}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={onSubmit}
      />,
    );

    const input = screen.getByPlaceholderText("Item name");
    await user.type(input, "Test Item");

    const createButton = screen.getByText("Create");
    await user.click(createButton);

    expect(onSubmit).toHaveBeenCalledWith("Test Item");
  });

  it("should not call onSubmit with empty value", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();

    renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={() => {}}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={onSubmit}
      />,
    );

    const createButton = screen.getByText("Create");
    await user.click(createButton);

    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("should disable submit button when loading", () => {
    renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={() => {}}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={vi.fn()}
        isLoading={true}
      />,
    );

    const submitButton = screen.getByText("Submitting...");
    expect(submitButton).toBeDisabled();
  });

  it("should call onOpenChange when cancel is clicked", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={onOpenChange}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={vi.fn()}
      />,
    );

    const cancelButton = screen.getByText("Cancel");
    await user.click(cancelButton);

    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});
