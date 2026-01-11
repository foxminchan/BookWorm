import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { createMockBook } from "@/__tests__/factories";
import { columns } from "@/features/books/table/columns";

describe("Books Table Columns", () => {
  const mockBook = createMockBook({
    name: "The Great Gatsby",
    description: "A classic novel",
    status: "InStock",
  });

  it("has correct column count", () => {
    expect(columns).toHaveLength(8); // name, authors, category, publisher, price, status, rating, actions
  });

  it("renders title column with font-medium", () => {
    const titleColumn = columns[0]!;
    const cell = titleColumn.cell as any;
    const { container } = render(cell({ row: { original: mockBook } } as any));

    expect(screen.getByText(mockBook.name)).toBeInTheDocument();
    expect(container.querySelector(".font-medium")).toBeInTheDocument();
  });

  it("renders authors column with comma-separated names", () => {
    const authorsColumn = columns[1]!;
    const cell = authorsColumn.cell as any;
    const expectedAuthors = mockBook.authors.map((a) => a.name).join(", ");
    render(cell({ row: { original: mockBook } } as any));

    expect(screen.getByText(expectedAuthors)).toBeInTheDocument();
  });

  it("renders category name", () => {
    const categoryColumn = columns[2]!;
    const cell = categoryColumn.cell as any;
    render(cell({ row: { original: mockBook } } as any));

    expect(screen.getByText(mockBook.category.name)).toBeInTheDocument();
  });

  it("renders dash when category is null", () => {
    const categoryColumn = columns[2]!;
    const cell = categoryColumn.cell as any;
    const bookWithoutCategory = { ...mockBook, category: null };
    render(cell({ row: { original: bookWithoutCategory } } as any));

    expect(screen.getByText("-")).toBeInTheDocument();
  });

  it("renders publisher name", () => {
    const publisherColumn = columns[3]!;
    const cell = publisherColumn.cell as any;
    render(cell({ row: { original: mockBook } } as any));

    expect(screen.getByText(mockBook.publisher.name)).toBeInTheDocument();
  });

  it("renders dash when publisher is null", () => {
    const publisherColumn = columns[3]!;
    const cell = publisherColumn.cell as any;
    const bookWithoutPublisher = { ...mockBook, publisher: null };
    render(cell({ row: { original: bookWithoutPublisher } } as any));

    expect(screen.getByText("-")).toBeInTheDocument();
  });

  it("renders sale price with strikethrough on original price", () => {
    const mockBookWithSale = createMockBook({
      priceSale: mockBook.price * 0.7,
    });
    const priceColumn = columns[4]!;
    const cell = priceColumn.cell as any;
    const { container } = render(
      cell({ row: { original: mockBookWithSale } } as any),
    );

    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });

    expect(
      screen.getByText(formatter.format(mockBookWithSale.price)),
    ).toBeInTheDocument();
    expect(
      screen.getByText(formatter.format(mockBookWithSale.priceSale!)),
    ).toBeInTheDocument();
    expect(container.querySelector("del")).toHaveTextContent(
      formatter.format(mockBookWithSale.price),
    );
  });

  it("renders regular price when no sale", () => {
    const priceColumn = columns[4]!;
    const cell = priceColumn.cell as any;
    const bookNoSale = { ...mockBook, priceSale: null };
    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });
    render(cell({ row: { original: bookNoSale } } as any));

    expect(
      screen.getByText(formatter.format(bookNoSale.price)),
    ).toBeInTheDocument();
  });

  it("renders price with accessibility label when on sale", () => {
    const priceColumn = columns[4]!;
    const cell = priceColumn.cell as any;
    const formatter = new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    });
    const saleBook = { ...mockBook, priceSale: mockBook.price * 0.8 }; // 80% of original price
    render(cell({ row: { original: saleBook } } as any));

    const priceElement = screen.getByText((_content, element) => {
      const ariaLabel = element?.getAttribute("aria-label");
      return (
        (ariaLabel?.includes("Sale price") &&
          ariaLabel?.includes("original price")) ??
        false
      );
    });
    expect(priceElement).toBeInTheDocument();
  });

  it("renders InStock status with green styling", () => {
    const statusColumn = columns[5]!;
    const cell = statusColumn.cell as any;
    render(cell({ row: { original: mockBook } } as any));

    const statusElement = screen.getByRole("status", {
      name: "Book status: In Stock",
    });
    expect(statusElement).toBeInTheDocument();
    expect(statusElement).toHaveClass("bg-green-100");
  });

  it("renders OutOfStock status with red styling", () => {
    const statusColumn = columns[5]!;
    const cell = statusColumn.cell as any;
    const bookOutOfStock = { ...mockBook, status: "OutOfStock" };
    render(cell({ row: { original: bookOutOfStock } } as any));

    const statusElement = screen.getByRole("status", {
      name: "Book status: Out of Stock",
    });
    expect(statusElement).toBeInTheDocument();
    expect(statusElement).toHaveClass("bg-red-100");
  });

  it("has correct header labels", () => {
    expect(columns[0]!.header).toBe("Title");
    expect(columns[1]!.header).toBe("Authors");
    expect(columns[2]!.header).toBe("Category");
    expect(columns[3]!.header).toBe("Publisher");
    expect(columns[4]!.header).toBe("Price");
    expect(columns[5]!.header).toBe("Status");
    expect(columns[6]!.header).toBe("Rating");
  });

  it("has correct accessor keys", () => {
    expect((columns[0] as any).accessorKey).toBe("name");
    expect((columns[1] as any).accessorKey).toBe("authors");
    expect((columns[2] as any).accessorKey).toBe("category");
    expect((columns[3] as any).accessorKey).toBe("publisher");
    expect((columns[4] as any).accessorKey).toBe("price");
    expect((columns[5] as any).accessorKey).toBe("status");
    expect((columns[6] as any).accessorKey).toBe("averageRating");
  });

  it("renders rating with review count", () => {
    const ratingColumn = columns[6]!;
    const cell = ratingColumn.cell as any;
    const ratingBook = createMockBook({
      averageRating: 4.5,
      totalReviews: 150,
    });
    render(cell({ row: { original: ratingBook } } as any));

    expect(
      screen.getByText(
        `${ratingBook.averageRating} (${ratingBook.totalReviews})`,
      ),
    ).toBeInTheDocument();
  });

  it("renders rating with accessibility label", () => {
    const ratingColumn = columns[6]!;
    const cell = ratingColumn.cell as any;
    const ratingBook = createMockBook({
      averageRating: 4.5,
      totalReviews: 150,
    });
    render(cell({ row: { original: ratingBook } } as any));

    const ariaLabel = screen.getByLabelText(
      new RegExp(
        `Rating ${ratingBook.averageRating} out of 5 based on ${ratingBook.totalReviews} reviews`,
      ),
    );
    expect(ariaLabel).toBeInTheDocument();
  });
});
