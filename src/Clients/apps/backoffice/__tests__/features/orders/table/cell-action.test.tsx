import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import type { Order } from "@workspace/types/ordering/orders";

import { createMockOrder } from "@/__tests__/factories";
import { CellAction } from "@/features/orders/table/cell-action";

vi.mock("@workspace/api-hooks/ordering/orders/useCancelOrder");
vi.mock("@workspace/api-hooks/ordering/orders/useCompleteOrder");
vi.mock("@workspace/api-hooks/ordering/orders/useDeleteOrder");
vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: vi.fn(),
    refresh: vi.fn(),
  }),
}));

const mockUseCancelOrder = vi.hoisted(() => vi.fn());
const mockUseCompleteOrder = vi.hoisted(() => vi.fn());
const mockUseDeleteOrder = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/ordering/orders/useCancelOrder", () => ({
  default: mockUseCancelOrder,
}));

vi.mock("@workspace/api-hooks/ordering/orders/useCompleteOrder", () => ({
  default: mockUseCompleteOrder,
}));

vi.mock("@workspace/api-hooks/ordering/orders/useDeleteOrder", () => ({
  default: mockUseDeleteOrder,
}));

describe("Orders CellAction", () => {
  const mockOrder = createMockOrder({ status: "New" });

  const renderWithQueryClient = (component: React.ReactElement) => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    return render(
      <QueryClientProvider client={queryClient}>
        {component}
      </QueryClientProvider>,
    );
  };

  beforeEach(() => {
    mockUseCancelOrder.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });
    mockUseCompleteOrder.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });
    mockUseDeleteOrder.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });
  });

  it("renders view, complete, and cancel buttons for New order", () => {
    renderWithQueryClient(<CellAction order={mockOrder} />);

    expect(
      screen.getByLabelText(
        new RegExp(`View order details ${mockOrder.id.slice(0, 8)}`),
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(
        new RegExp(`Complete order ${mockOrder.id.slice(0, 8)}`),
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText(
        new RegExp(`Cancel order ${mockOrder.id.slice(0, 8)}`),
      ),
    ).toBeInTheDocument();
  });

  it("view button links to order details page", () => {
    renderWithQueryClient(<CellAction order={mockOrder} />);

    const viewLink = screen.getByLabelText(
      new RegExp(`View order details ${mockOrder.id.slice(0, 8)}`),
    );
    expect(viewLink).toHaveAttribute("href", `/orders/${mockOrder.id}`);
  });

  it("does not show complete button for Completed order", () => {
    const completedOrder: Order = { ...mockOrder, status: "Completed" };
    renderWithQueryClient(<CellAction order={completedOrder} />);

    expect(screen.queryByLabelText(/Complete order/)).not.toBeInTheDocument();
  });

  it("does not show cancel button for Cancelled order", () => {
    const cancelledOrder: Order = { ...mockOrder, status: "Cancelled" };
    renderWithQueryClient(<CellAction order={cancelledOrder} />);

    expect(screen.queryByLabelText(/Cancel order/)).not.toBeInTheDocument();
  });

  it("does not show complete button for Cancelled order", () => {
    const cancelledOrder: Order = { ...mockOrder, status: "Cancelled" };
    renderWithQueryClient(<CellAction order={cancelledOrder} />);

    expect(screen.queryByLabelText(/Complete order/)).not.toBeInTheDocument();
  });

  it("opens complete dialog when complete button clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<CellAction order={mockOrder} />);

    const completeButton = screen.getByLabelText(
      new RegExp(`Complete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(completeButton);

    await waitFor(() => {
      expect(screen.getByText("Complete Order")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(
            `Are you sure you want to mark order #${mockOrder.id.slice(0, 8)}`,
          ),
        ),
      ).toBeInTheDocument();
    });
  });

  it("opens cancel dialog when cancel button clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<CellAction order={mockOrder} />);

    const cancelButton = screen.getByLabelText(
      new RegExp(`Cancel order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(cancelButton);

    await waitFor(() => {
      expect(screen.getByText("Cancel Order")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(
            `Are you sure you want to cancel order #${mockOrder.id.slice(0, 8)}`,
          ),
        ),
      ).toBeInTheDocument();
    });
  });

  it("renders delete button", () => {
    renderWithQueryClient(<CellAction order={mockOrder} />);

    expect(
      screen.getByLabelText(
        new RegExp(`Delete order ${mockOrder.id.slice(0, 8)}`),
      ),
    ).toBeInTheDocument();
  });

  it("opens delete dialog when delete button clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<CellAction order={mockOrder} />);

    const deleteButton = screen.getByLabelText(
      new RegExp(`Delete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(deleteButton);

    await waitFor(() => {
      expect(screen.getByText("Delete Order")).toBeInTheDocument();
      expect(
        screen.getByText(
          new RegExp(
            `Are you sure you want to delete order #${mockOrder.id.slice(0, 8)}`,
          ),
        ),
      ).toBeInTheDocument();
    });
  });

  it("calls complete mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn();
    mockUseCompleteOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const completeButton = screen.getByLabelText(
      new RegExp(`Complete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(completeButton);

    await waitFor(() => {
      expect(screen.getByText("Complete Order")).toBeInTheDocument();
    });

    const confirmButton = screen.getByRole("button", { name: /complete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalledWith(mockOrder.id, expect.any(Object));
    });
  });

  it("calls cancel mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn();
    mockUseCancelOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const cancelButton = screen.getByLabelText(
      new RegExp(`Cancel order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(cancelButton);

    await waitFor(() => {
      expect(screen.getByText("Cancel Order")).toBeInTheDocument();
    });

    const cancelButtons = screen.getAllByRole("button", { name: /cancel/i });
    expect(cancelButtons.length).toBeGreaterThan(1);
    const confirmButton = cancelButtons[1] as HTMLElement;
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalledWith(mockOrder.id, expect.any(Object));
    });
  });

  it("calls delete mutation when confirmed", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn();
    mockUseDeleteOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const deleteButton = screen.getByLabelText(
      new RegExp(`Delete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(deleteButton);

    await waitFor(() => {
      expect(screen.getByText("Delete Order")).toBeInTheDocument();
    });

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(mutateFn).toHaveBeenCalledWith(mockOrder.id, expect.any(Object));
    });
  });

  it("closes complete dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn((orderId, { onSuccess }) => {
      Promise.resolve().then(() => onSuccess?.());
    });
    mockUseCompleteOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const completeButton = screen.getByLabelText(
      new RegExp(`Complete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(completeButton);

    const confirmButton = screen.getByRole("button", { name: /complete/i });
    await user.click(confirmButton);

    await waitFor(
      () => {
        expect(mutateFn).toHaveBeenCalled();
      },
      { timeout: 3000 },
    );
  });

  it("closes cancel dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn((orderId, { onSuccess }) => {
      Promise.resolve().then(() => onSuccess?.());
    });
    mockUseCancelOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const cancelButton = screen.getByLabelText(
      new RegExp(`Cancel order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(cancelButton);

    const cancelButtons = screen.getAllByRole("button", { name: /cancel/i });
    expect(cancelButtons.length).toBeGreaterThan(1);
    const confirmButton = cancelButtons[1] as HTMLElement;
    await user.click(confirmButton);

    await waitFor(
      () => {
        expect(mutateFn).toHaveBeenCalled();
      },
      { timeout: 3000 },
    );
  });

  it("closes delete dialog on successful mutation", async () => {
    const user = userEvent.setup();
    const mutateFn = vi.fn((_, { onSuccess }) => {
      Promise.resolve().then(() => onSuccess?.());
    });
    mockUseDeleteOrder.mockReturnValue({
      mutate: mutateFn,
      isPending: false,
    });

    renderWithQueryClient(<CellAction order={mockOrder} />);

    const deleteButton = screen.getByLabelText(
      new RegExp(`Delete order ${mockOrder.id.slice(0, 8)}`),
    );
    await user.click(deleteButton);

    const confirmButton = screen.getByRole("button", { name: /delete/i });
    await user.click(confirmButton);

    await waitFor(
      () => {
        expect(mutateFn).toHaveBeenCalled();
      },
      { timeout: 3000 },
    );
  });
});
