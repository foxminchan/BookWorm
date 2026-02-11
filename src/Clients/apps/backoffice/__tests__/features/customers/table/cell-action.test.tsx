import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { createMockCustomer } from "@/__tests__/factories";
import { renderWithProviders } from "@/__tests__/utils/test-utils";
import { CellAction } from "@/features/customers/table/cell-action";

const mockUseDeleteBuyer = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/ordering/buyers/useDeleteBuyer", () => ({
  default: mockUseDeleteBuyer,
}));

describe("Customers CellAction", () => {
  const mockCustomer = createMockCustomer();

  it("renders delete button", () => {
    mockUseDeleteBuyer.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    const { container } = renderWithProviders(
      <CellAction customer={mockCustomer} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    expect(deleteButton).toBeInTheDocument();
  });

  it("opens delete dialog when delete button clicked", async () => {
    const user = userEvent.setup();
    mockUseDeleteBuyer.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    const { container } = renderWithProviders(
      <CellAction customer={mockCustomer} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(screen.getByText("Delete Customer")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(`Are you sure you want to delete "${mockCustomer.name}`),
        ),
      ).toBeInTheDocument();
    });
  });

  it("disables delete button when mutation is pending", () => {
    mockUseDeleteBuyer.mockReturnValue({
      mutate: vi.fn(),
      isPending: true,
    });

    const { container } = renderWithProviders(
      <CellAction customer={mockCustomer} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    expect(deleteButton).toBeDisabled();
  });

  it("handles customer with null name", async () => {
    const user = userEvent.setup();
    mockUseDeleteBuyer.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    const customerWithoutName = { ...mockCustomer, name: null as any };
    const { container } = renderWithProviders(
      <CellAction customer={customerWithoutName} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(
        screen.getByText(/Are you sure you want to delete "this customer"/),
      ).toBeInTheDocument();
    });
  });

  it("calls delete mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn();
    mockUseDeleteBuyer.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    const { container } = renderWithProviders(
      <CellAction customer={mockCustomer} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    await waitFor(() => {
      expect(screen.getByText("Delete Customer")).toBeInTheDocument();
    });

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalledWith(
        mockCustomer.id,
        expect.any(Object),
      );
    });
  });

  it("closes delete dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn((customerId, { onSuccess }) => {
      onSuccess?.();
    });
    mockUseDeleteBuyer.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    const { container } = renderWithProviders(
      <CellAction customer={mockCustomer} />,
    );

    const deleteButton = container.querySelector(
      'button[class*="destructive"]',
    );
    await user.click(deleteButton!);

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalled();
    });
  });
});
