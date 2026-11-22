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

  it("renders BookListView and empty state", () => {
    const { getByText } = render(<BookListView query="ddd" books={[]} />);
    expect(getByText("No books found.")).toBeInTheDocument();
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
});
