import { act, render, renderHook, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { useBasketConfirmation } from "@/hooks/useBasketConfirmation";

describe("useBasketConfirmation", () => {
  it("resolves promise when confirming", async () => {
    const { result } = renderHook(() => useBasketConfirmation());
    let confirmationPromise!: Promise<boolean>;
    await act(async () => {
      confirmationPromise = result.current.requestConfirmation(
        "book-1",
        2,
        "My Book",
        10,
      );
    });

    render(<result.current.ConfirmationDialog />);

    await userEvent.click(screen.getByRole("button", { name: /confirm/i }));

    await expect(confirmationPromise).resolves.toBe(true);
  });

  it("resolves false when cancelled", async () => {
    const { result } = renderHook(() => useBasketConfirmation());
    let confirmationPromise!: Promise<boolean>;
    await act(async () => {
      confirmationPromise = result.current.requestConfirmation(
        "book-2",
        1,
        "Other Book",
        5,
      );
    });

    render(<result.current.ConfirmationDialog />);

    await userEvent.click(screen.getByRole("button", { name: /cancel/i }));

    await expect(confirmationPromise).resolves.toBe(false);
  });

  it("renders book details when provided", async () => {
    const { result } = renderHook(() => useBasketConfirmation());

    act(() => {
      result.current.requestConfirmation("book-3", 3, "Detail Book", 15);
    });

    render(<result.current.ConfirmationDialog />);

    expect(await screen.findByText(/Detail Book/)).toBeInTheDocument();
    expect(screen.getByText(/Quantity:/)).toBeInTheDocument();
    expect(screen.getByText(/Price:/)).toBeInTheDocument();
  });
});
