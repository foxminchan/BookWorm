import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { basketItemCountAtom, basketItemsAtom } from "@/atoms/basket-atom";
import { useBasketContext } from "@/hooks/useBasketContext";

const { mockUseAtomValue, mockUseCopilotReadable } = vi.hoisted(() => ({
  mockUseAtomValue: vi.fn(),
  mockUseCopilotReadable: vi.fn(),
}));

vi.mock("jotai", async () => {
  const actual = await vi.importActual<typeof import("jotai")>("jotai");
  return {
    ...actual,
    useAtomValue: (atom: unknown) => mockUseAtomValue(atom),
  };
});

vi.mock("@copilotkit/react-core", () => ({
  useCopilotReadable: (args: unknown) => mockUseCopilotReadable(args),
}));

describe("useBasketContext", () => {
  it("publishes basket data to Copilot", () => {
    mockUseAtomValue.mockImplementation((atom: unknown) => {
      if (atom === basketItemsAtom) {
        return [
          { id: "1", name: "Book A", quantity: 2, price: 10, priceSale: 8 },
          { id: "2", name: "Book B", quantity: 1, price: 5 },
        ];
      }
      if (atom === basketItemCountAtom) {
        return 3;
      }
      return undefined;
    });

    renderHook(() => useBasketContext());

    expect(mockUseCopilotReadable).toHaveBeenCalledTimes(1);
    expect(mockUseCopilotReadable).toHaveBeenCalledWith({
      description:
        "The current user's shopping basket with books they plan to purchase",
      value: {
        itemCount: 3,
        items: [
          { id: "1", name: "Book A", quantity: 2, price: 10, priceSale: 8 },
          {
            id: "2",
            name: "Book B",
            quantity: 1,
            price: 5,
            priceSale: undefined,
          },
        ],
        totalPrice: 21,
      },
    });
  });
});
