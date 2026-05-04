import { JSX } from "react";

import { render, renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useA2UIComponents } from "@/hooks/useA2UIComponents";

type RegisteredComponent = {
  name: string;
  description: string;
  parameters: unknown;
  render: (props: any) => JSX.Element;
};

const registeredComponents: RegisteredComponent[] = [];

vi.mock("@copilotkit/react-core/v2", () => ({
  useComponent: vi.fn((config: RegisteredComponent) => {
    registeredComponents.push(config);
  }),
}));

vi.mock("@workspace/utils/format", () => ({
  formatPrice: (n: number) => `$${n.toFixed(2)}`,
  calculateDiscount: (price: number, sale: number) =>
    Math.round(((price - sale) / price) * 100),
}));

describe("useA2UIComponents", () => {
  beforeEach(() => {
    registeredComponents.length = 0;
    vi.clearAllMocks();
  });

  it("registers showBookCard, showBookList, and showBasketSummary components", () => {
    renderHook(() => useA2UIComponents());

    const names = registeredComponents.map((c) => c.name);
    expect(names).toContain("showBookCard");
    expect(names).toContain("showBookList");
    expect(names).toContain("showBasketSummary");
    expect(registeredComponents).toHaveLength(3);
  });

  describe("showBookCard", () => {
    it("renders book title, authors, and price", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBookCard",
      );

      const { getByText } = render(
        component!.render({
          id: "book-1",
          title: "Clean Code",
          authors: ["Robert C. Martin"],
          price: 29.99,
          priceSale: null,
          status: "InStock",
          averageRating: 4.8,
        }),
      );

      expect(getByText("Clean Code")).toBeInTheDocument();
      expect(getByText("Robert C. Martin")).toBeInTheDocument();
      expect(getByText("$29.99")).toBeInTheDocument();
      expect(getByText("In Stock")).toBeInTheDocument();
      expect(getByText("4.8")).toBeInTheDocument();
    });

    it("renders sale price with discount badge when priceSale is set", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBookCard",
      );

      const { getByText } = render(
        component!.render({
          id: "book-2",
          title: "Refactoring",
          authors: ["Martin Fowler"],
          price: 40.0,
          priceSale: 30.0,
          status: "InStock",
        }),
      );

      expect(getByText("$30.00")).toBeInTheDocument();
      expect(getByText("$40.00")).toBeInTheDocument();
      expect(getByText(/-25%/)).toBeInTheDocument();
    });

    it("renders Out of Stock badge when status is OutOfStock", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBookCard",
      );

      const { getByText } = render(
        component!.render({
          id: "book-3",
          title: "Old Book",
          authors: [],
          price: 15.0,
          priceSale: null,
          status: "OutOfStock",
        }),
      );

      expect(getByText("Out of Stock")).toBeInTheDocument();
    });
  });

  describe("showBookList", () => {
    it("renders all books in the list", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBookList",
      );

      const { getByText } = render(
        component!.render({
          query: "clean architecture",
          books: [
            {
              id: "b1",
              title: "Clean Architecture",
              authors: ["Uncle Bob"],
              price: 35.0,
              priceSale: null,
              status: "InStock",
            },
            {
              id: "b2",
              title: "Domain-Driven Design",
              authors: ["Eric Evans"],
              price: 50.0,
              priceSale: 40.0,
              status: "InStock",
            },
          ],
        }),
      );

      expect(getByText("Clean Architecture")).toBeInTheDocument();
      expect(getByText("Uncle Bob")).toBeInTheDocument();
      expect(getByText("Domain-Driven Design")).toBeInTheDocument();
      expect(getByText("Eric Evans")).toBeInTheDocument();
      // The query text appears in both the subtitle line and the book title row
      expect(getByText(/Results for/i)).toBeInTheDocument();
    });

    it("shows empty state when no books", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBookList",
      );

      const { getByText } = render(component!.render({ books: [] }));

      expect(getByText(/No books found/i)).toBeInTheDocument();
    });
  });

  describe("showBasketSummary", () => {
    it("renders basket items and total", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBasketSummary",
      );

      const { getByText } = render(
        component!.render({
          itemCount: 2,
          totalPrice: 59.98,
          items: [
            {
              id: "i1",
              name: "Clean Code",
              quantity: 1,
              price: 29.99,
              priceSale: null,
            },
            {
              id: "i2",
              name: "Refactoring",
              quantity: 1,
              price: 29.99,
              priceSale: null,
            },
          ],
        }),
      );

      expect(getByText(/Your Basket/i)).toBeInTheDocument();
      expect(getByText(/Clean Code/i)).toBeInTheDocument();
      expect(getByText(/Refactoring/i)).toBeInTheDocument();
      expect(getByText("$59.98")).toBeInTheDocument();
    });

    it("shows empty basket message when no items", () => {
      renderHook(() => useA2UIComponents());

      const component = registeredComponents.find(
        (c) => c.name === "showBasketSummary",
      );

      const { getByText } = render(
        component!.render({
          itemCount: 0,
          totalPrice: 0,
          items: [],
        }),
      );

      expect(getByText(/Your basket is empty/i)).toBeInTheDocument();
    });
  });
});
