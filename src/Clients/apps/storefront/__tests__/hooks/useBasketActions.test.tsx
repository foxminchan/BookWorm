import { JSX } from "react";

import { act, render, renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useBasketActions } from "@/hooks/useBasketActions";

const registeredActions: Array<{
  name: string;
  handler: (args: any) => Promise<any>;
  render: (args: any) => JSX.Element;
}> = [];

const { mockRequestConfirmation, mockUpdate, mockGet } = vi.hoisted(() => ({
  mockRequestConfirmation: vi.fn(),
  mockUpdate: vi.fn(),
  mockGet: vi.fn(),
}));

vi.mock("@copilotkit/react-core", () => ({
  useCopilotAction: vi.fn((config) => {
    registeredActions.push(config);
  }),
}));

vi.mock("@/hooks/useBasketConfirmation", () => ({
  useBasketConfirmation: () => ({
    requestConfirmation: mockRequestConfirmation,
    ConfirmationDialog: () => <div data-testid="confirmation-dialog" />,
  }),
}));

vi.mock("@workspace/api-client/basket/baskets", () => ({
  default: {
    update: mockUpdate,
    get: mockGet,
  },
}));

describe("useBasketActions", () => {
  beforeEach(() => {
    registeredActions.length = 0;
    vi.clearAllMocks();
  });

  it("registers addToBasket and calls update after confirmation", async () => {
    mockRequestConfirmation.mockResolvedValue(true);
    mockUpdate.mockResolvedValue({});

    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "addToBasket");

    expect(action).toBeDefined();

    await act(async () => {
      await action?.handler({
        bookId: "book-1",
        quantity: 2,
        bookTitle: "Book Title",
      });
    });

    expect(mockRequestConfirmation).toHaveBeenCalledWith(
      "book-1",
      2,
      "Book Title",
      undefined,
    );
    expect(mockUpdate).toHaveBeenCalledWith({
      items: [{ id: "book-1", quantity: 2 }],
    });
  });

  it("returns cancellation result when user does not confirm", async () => {
    mockRequestConfirmation.mockResolvedValue(false);

    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "addToBasket");
    let result: unknown;

    await act(async () => {
      result = await action?.handler({ bookId: "book-1" });
    });

    expect(result).toEqual({
      success: false,
      cancelled: true,
      message: "Action cancelled by user",
    });
    expect(mockUpdate).not.toHaveBeenCalled();
  });

  it("renders loading and success states for addToBasket", () => {
    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "addToBasket");

    const loading = action?.render({ status: "executing" } as any);
    const { getByText, rerender } = render(loading ?? <div />);

    expect(getByText(/Adding to basket/i)).toBeInTheDocument();

    const success = action?.render({
      status: "complete",
      result: { message: "Added item" },
    } as any);

    rerender(success ?? <div />);

    expect(getByText(/Added item/)).toBeInTheDocument();
  });

  it("computes totals in viewBasket handler", async () => {
    mockGet.mockResolvedValue({
      items: [
        { id: "1", name: "One", price: 10, quantity: 2 },
        { id: "2", name: "Two", price: 5, priceSale: 4, quantity: 1 },
      ],
    });

    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "viewBasket");
    let result: unknown;

    await act(async () => {
      result = await action?.handler({});
    });

    expect(result).toEqual({
      items: [
        { id: "1", name: "One", price: 10, quantity: 2 },
        { id: "2", name: "Two", price: 5, priceSale: 4, quantity: 1 },
      ],
      totalPrice: 24,
      itemCount: 2,
    });
  });

  it("renders empty state for viewBasket", () => {
    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "viewBasket");

    const loading = action?.render({ status: "executing" } as any);
    const { getByText, rerender } = render(loading ?? <div />);

    expect(getByText(/Loading basket/i)).toBeInTheDocument();

    const empty = action?.render({
      status: "complete",
      result: { items: [], totalPrice: 0, itemCount: 0 },
    } as any);

    rerender(empty ?? <div />);

    expect(getByText(/Your basket is empty/i)).toBeInTheDocument();
  });

  it("renders list state for viewBasket", () => {
    renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "viewBasket");

    const result = {
      items: [
        { id: "1", name: "One", price: 10, priceSale: undefined, quantity: 2 },
        { id: "2", name: "Two", price: 5, priceSale: 4, quantity: 1 },
      ],
      totalPrice: 24,
      itemCount: 2,
    };

    const view = action?.render({ status: "complete", result } as any);

    const { getByText } = render(view ?? <div />);

    expect(getByText(/Your Basket \(2 items\)/i)).toBeInTheDocument();
    expect(getByText(/One/)).toBeInTheDocument();
    expect(getByText(/Two/)).toBeInTheDocument();
    expect(getByText(/Total:/)).toBeInTheDocument();
  });

  it("exposes LiveRegion with latest announcement", async () => {
    mockRequestConfirmation.mockResolvedValue(true);
    mockUpdate.mockResolvedValue({});

    const { result: hook } = renderHook(() => useBasketActions());

    const action = registeredActions.find((a) => a.name === "addToBasket");

    await act(async () => {
      await action?.handler({
        bookId: "book-1",
        quantity: 1,
        bookTitle: "Basket Book",
      });
    });

    const { getByRole } = render(<hook.current.LiveRegion />);

    expect(getByRole("status").textContent).toContain("Basket Book");
  });
});
