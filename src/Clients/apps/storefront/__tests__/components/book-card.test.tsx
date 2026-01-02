import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import type { Book } from "@workspace/types/catalog/books";

import { BookCard } from "@/components/book-card";

import { renderWithProviders } from "../utils/test-utils";

const mockBook: Book = {
  id: "book-1",
  name: "Test Book Title",
  description: "A great test book",
  imageUrl: "https://example.com/book.jpg",
  price: 29.99,
  priceSale: 19.99,
  status: "InStock",
  category: { id: "cat-1", name: "Fiction" },
  publisher: { id: "pub-1", name: "Test Publisher" },
  authors: [
    { id: "auth-1", name: "John Doe" },
    { id: "auth-2", name: "Jane Smith" },
  ],
  averageRating: 4.5,
  totalReviews: 120,
};

describe("BookCard", () => {
  it("should render book information correctly", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    expect(screen.getByText("Test Book Title")).toBeInTheDocument();
    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("John Doe, Jane Smith")).toBeInTheDocument();
    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display discount badge when priceSale is present", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    // Calculate discount: ((29.99 - 19.99) / 29.99) * 100 ≈ 33%
    expect(screen.getByText(/-\d+% OFF/)).toBeInTheDocument();
  });

  it("should not display discount badge when priceSale is null", () => {
    const bookWithoutSale = { ...mockBook, priceSale: null };
    renderWithProviders(<BookCard book={bookWithoutSale} />);

    expect(screen.queryByText(/-\d+% OFF/)).not.toBeInTheDocument();
  });

  it("should display sale price when available", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    expect(screen.getByText("$19.99")).toBeInTheDocument();
    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should display only regular price when no sale", () => {
    const bookWithoutSale = { ...mockBook, priceSale: null };
    renderWithProviders(<BookCard book={bookWithoutSale} />);

    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should call onClick when card is clicked", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();

    renderWithProviders(<BookCard book={mockBook} onClick={handleClick} />);

    const article = screen.getByRole("listitem");
    await user.click(article);

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it("should call onClick when Enter key is pressed", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();

    renderWithProviders(<BookCard book={mockBook} onClick={handleClick} />);

    const article = screen.getByRole("listitem");
    article.focus();
    await user.keyboard("{Enter}");

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it("should handle multiple authors correctly", () => {
    const bookWithManyAuthors = {
      ...mockBook,
      authors: [
        { id: "1", name: "Author 1" },
        { id: "2", name: "Author 2" },
        { id: "3", name: "Author 3" },
        { id: "4", name: "Author 4" },
        { id: "5", name: "Author 5" },
      ],
    };

    renderWithProviders(<BookCard book={bookWithManyAuthors} />);

    // Should show first 3 authors plus count
    expect(
      screen.getByText(/Author 1, Author 2, Author 3 \+2 more/),
    ).toBeInTheDocument();
  });

  it("should display 'Unknown Author' when no authors", () => {
    const bookWithoutAuthors = { ...mockBook, authors: [] };
    renderWithProviders(<BookCard book={bookWithoutAuthors} />);

    expect(screen.getByText("Unknown Author")).toBeInTheDocument();
  });

  it("should display default rating when averageRating is zero", () => {
    const bookWithoutRating = { ...mockBook, averageRating: 0 };
    renderWithProviders(<BookCard book={bookWithoutRating} />);

    expect(screen.getByText("0.0")).toBeInTheDocument();
  });

  it("should render category name", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    expect(screen.getByText("Fiction")).toBeInTheDocument();
  });

  it("should have proper accessibility attributes", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    const article = screen.getByRole("listitem");
    expect(article).toHaveAttribute("itemScope");
    expect(article).toHaveAttribute("itemType", "https://schema.org/Book");
    expect(article).toHaveAttribute("tabIndex", "0");
  });

  it("should display book image with proper alt text", () => {
    renderWithProviders(<BookCard book={mockBook} />);

    const image = screen.getByAltText("Test Book Title");
    expect(image).toBeInTheDocument();
  });
});
