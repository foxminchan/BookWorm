import { render } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { bookwormCatalog } from "@/a2ui/catalog";
import { BOOKWORM_CATALOG_ID } from "@/a2ui/constants";
import {
  BasketSummaryView,
  BookCardView,
  BookListView,
} from "@/a2ui/renderers";

vi.mock("@workspace/utils/format", () => ({
  formatPrice: (n: number) => `$${n.toFixed(2)}`,
  calculateDiscount: (price: number, sale: number) =>
    Math.round(((price - sale) / price) * 100),
}));

describe("bookwormCatalog", () => {
  it("registers BookCard, BookList, and BasketSummary alongside standardUI", () => {
    expect(bookwormCatalog.id).toBe(BOOKWORM_CATALOG_ID);

    const componentNames = Array.from(bookwormCatalog.components.keys());
    expect(componentNames).toContain("BookCard");
    expect(componentNames).toContain("BookList");
    expect(componentNames).toContain("BasketSummary");
    expect(componentNames).toContain("Card");
    expect(componentNames).toContain("Column");
    expect(componentNames).toContain("Row");
  });

  it("renders BookCardView with title, authors, price, and rating", () => {
    const { getByText } = render(
      <BookCardView
        id="book-1"
        title="Domain-Driven Design"
        authors={["Eric Evans"]}
        price={54.99}
        priceSale={null}
        status="InStock"
        averageRating={4.7}
      />,
    );

    expect(getByText("Domain-Driven Design")).toBeInTheDocument();
    expect(getByText("Eric Evans")).toBeInTheDocument();
    expect(getByText("$54.99")).toBeInTheDocument();
    expect(getByText("In Stock")).toBeInTheDocument();
    expect(getByText("4.7")).toBeInTheDocument();
  });

  it("renders BookCardView without authors, with sale price, description, and OutOfStock status", () => {
    const { getByText, queryByText } = render(
      <BookCardView
        id="book-2"
        title="Clean Architecture"
        authors={[]}
        price={50}
        priceSale={40}
        status="OutOfStock"
        description="A handbook of software structure"
      />,
    );

    expect(getByText("Clean Architecture")).toBeInTheDocument();
    expect(getByText("Out of Stock")).toBeInTheDocument();
    expect(getByText("A handbook of software structure")).toBeInTheDocument();
    expect(getByText("$40.00")).toBeInTheDocument();
    expect(getByText("$50.00")).toBeInTheDocument();
    expect(getByText("-20%")).toBeInTheDocument();
    expect(queryByText("Eric Evans")).not.toBeInTheDocument();
  });

  it("renders BookListView and empty state", () => {
    const { getByText } = render(<BookListView query="ddd" books={[]} />);
    expect(getByText("No books found.")).toBeInTheDocument();
  });

  it("renders BookListView with books and without query", () => {
    const { getByText, queryByText } = render(
      <BookListView
        books={[
          {
            id: "b1",
            title: "Refactoring",
            authors: ["Martin Fowler"],
            price: 49.99,
            priceSale: 39.99,
            status: "InStock",
          },
          {
            id: "b2",
            title: "Anonymous Book",
            authors: [],
            price: 19.99,
            priceSale: null,
            status: "OutOfStock",
          },
        ]}
      />,
    );

    expect(queryByText(/Results for/)).not.toBeInTheDocument();
    expect(getByText("Refactoring")).toBeInTheDocument();
    expect(getByText("Martin Fowler")).toBeInTheDocument();
    expect(getByText("$39.99")).toBeInTheDocument();
    expect(getByText("$49.99")).toBeInTheDocument();
    expect(getByText("Anonymous Book")).toBeInTheDocument();
    expect(getByText("$19.99")).toBeInTheDocument();
  });

  it("renders BasketSummaryView with items and total", () => {
    const { getAllByText, getByText } = render(
      <BasketSummaryView
        itemCount={2}
        totalPrice={59.98}
        items={[{ id: "b1", name: "Clean Code", quantity: 2, price: 29.99 }]}
      />,
    );

    expect(getByText("Your Basket (2 items)")).toBeInTheDocument();
    expect(getByText("Clean Code × 2")).toBeInTheDocument();
    expect(getAllByText("$59.98")).toHaveLength(2);
  });

  it("renders BasketSummaryView empty state and singular item with sale price and null name", () => {
    const { getByText, rerender } = render(
      <BasketSummaryView itemCount={0} totalPrice={0} items={[]} />,
    );
    expect(getByText("Your Basket (0 items)")).toBeInTheDocument();
    expect(getByText("Your basket is empty.")).toBeInTheDocument();

    rerender(
      <BasketSummaryView
        itemCount={1}
        totalPrice={15}
        items={[
          { id: "b1", name: null, quantity: 1, price: 25, priceSale: 15 },
        ]}
      />,
    );
    expect(getByText("Your Basket (1 item)")).toBeInTheDocument();
    expect(getByText("Unknown × 1")).toBeInTheDocument();
  });
});
