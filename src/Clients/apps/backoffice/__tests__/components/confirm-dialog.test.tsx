import { describe, expect, it, vi } from "vitest";

import {
  renderWithProviders,
  screen,
  userEvent,
} from "@/__tests__/utils/test-utils";
import { ConfirmDialog } from "@/components/confirm-dialog";

describe("ConfirmDialog", () => {
  it("should render with title and description", () => {
    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={() => {}}
        title="Confirm Delete"
        description="Are you sure you want to delete this item?"
        actionLabel="Delete"
        isLoading={false}
        onConfirm={vi.fn()}
        actionType="delete"
      />,
    );

    expect(screen.getByText("Confirm Delete")).toBeInTheDocument();
    expect(
      screen.getByText("Are you sure you want to delete this item?"),
    ).toBeInTheDocument();
    expect(screen.getByText("Delete")).toBeInTheDocument();
  });

  it("should not render when closed", () => {
    const { container } = renderWithProviders(
      <ConfirmDialog
        open={false}
        onOpenChange={() => {}}
        title="Confirm Delete"
        description="Are you sure?"
        actionLabel="Delete"
        isLoading={false}
        onConfirm={vi.fn()}
      />,
    );

    expect(
      container.querySelector('[role="alertdialog"]'),
    ).not.toBeInTheDocument();
  });

  it("should call onConfirm when action button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn().mockResolvedValue(undefined);

    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={() => {}}
        title="Confirm Action"
        description="Proceed with action?"
        actionLabel="Confirm"
        isLoading={false}
        onConfirm={onConfirm}
      />,
    );

    const confirmButton = screen.getByText("Confirm");
    await user.click(confirmButton);

    expect(onConfirm).toHaveBeenCalled();
  });

  it("should show loading state", () => {
    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={() => {}}
        title="Processing"
        description="Please wait..."
        actionLabel="Submit"
        isLoading={true}
        onConfirm={vi.fn()}
      />,
    );

    // Button should be disabled when loading
    const button = screen.getByText("Processing...").closest("button");
    expect(button).toBeDisabled();

    // Loading spinner should be visible
    const spinner = document.querySelector("svg.lucide-loader-circle");
    expect(spinner).toBeInTheDocument();
  });

  it("should call onOpenChange when cancel is clicked", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();

    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={onOpenChange}
        title="Confirm"
        description="Are you sure?"
        actionLabel="Yes"
        isLoading={false}
        onConfirm={vi.fn()}
      />,
    );

    const cancelButton = screen.getByText("Cancel");
    await user.click(cancelButton);

    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("should apply correct styles for delete action type", () => {
    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={() => {}}
        title="Delete Item"
        description="This action cannot be undone"
        actionLabel="Delete"
        isLoading={false}
        onConfirm={vi.fn()}
        actionType="delete"
      />,
    );

    const deleteButton = screen.getByText("Delete");
    expect(deleteButton).toHaveClass("bg-destructive");
  });

  it("should apply correct styles for complete action type", () => {
    renderWithProviders(
      <ConfirmDialog
        open={true}
        onOpenChange={() => {}}
        title="Complete Task"
        description="Mark as complete?"
        actionLabel="Complete"
        isLoading={false}
        onConfirm={vi.fn()}
        actionType="complete"
      />,
    );

    const completeButton = screen.getByText("Complete");
    expect(completeButton).toHaveClass("bg-green-600");
  });
});
