import { fireEvent, waitFor } from "@testing-library/react";
import { toast } from "sonner";
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

  it("should submit when pressing Enter with a non-empty value", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

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

    const input = screen.getByPlaceholderText("Item name");
    await user.type(input, "  Enter Item  {Enter}");

    expect(onSubmit).toHaveBeenCalledWith("Enter Item");
  });

  it("should ignore Enter key during IME composition", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

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

    const input = screen.getByPlaceholderText("Item name");
    await user.type(input, "IME Item");

    fireEvent.keyDown(input, { key: "Enter", isComposing: true });
    fireEvent.keyDown(input, { key: "Enter", keyCode: 229 });

    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("should not submit on Enter when value is whitespace or while loading", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    const { rerender } = renderWithProviders(
      <SimpleDialog
        open={true}
        onOpenChange={() => {}}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={onSubmit}
        isLoading={false}
      />,
    );

    const input = screen.getByPlaceholderText("Item name");
    await user.type(input, "   {Enter}");
    expect(onSubmit).not.toHaveBeenCalled();

    await user.type(input, "Valid Item");
    rerender(
      <SimpleDialog
        open={true}
        onOpenChange={() => {}}
        title="Create Item"
        description="Enter item name"
        placeholder="Item name"
        onSubmit={onSubmit}
        isLoading={true}
      />,
    );

    fireEvent.keyDown(input, { key: "Enter" });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("should log error and show toast when onSubmit rejects", async () => {
    const user = userEvent.setup();
    const error = new Error("Save failed");
    const onSubmit = vi.fn().mockRejectedValue(error);
    const consoleErrorSpy = vi
      .spyOn(console, "error")
      .mockImplementation(() => {});
    const toastErrorSpy = vi.spyOn(toast, "error").mockImplementation(() => "");

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

    await user.type(screen.getByPlaceholderText("Item name"), "Failing Item");
    await user.click(screen.getByText("Create"));

    await waitFor(() => {
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        "Dialog submission failed:",
        error,
      );
      expect(toastErrorSpy).toHaveBeenCalledWith(
        "Failed to save changes. Please try again.",
      );
    });

    consoleErrorSpy.mockRestore();
    toastErrorSpy.mockRestore();
  });
});
