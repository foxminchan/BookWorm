import { faker } from "@faker-js/faker";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import type { Book } from "@workspace/types/catalog/books";

import { BookCard } from "@/components/book-card";

import { renderWithProviders } from "../utils/test-utils";

const mockBook: Book = {
  id: faker.string.uuid(),
  name: faker.commerce.productName(),
  description: faker.commerce.productDescription(),
  imageUrl: faker.image.url(),
  price: faker.number.float({ min: 10, max: 100, fractionDigits: 2 }),
  priceSale: faker.number.float({ min: 5, max: 50, fractionDigits: 2 }),
  status: "InStock",
  category: { id: faker.string.uuid(), name: faker.commerce.department() },
  publisher: { id: faker.string.uuid(), name: faker.company.name() },
  authors: [
    { id: faker.string.uuid(), name: faker.person.fullName() },
    { id: faker.string.uuid(), name: faker.person.fullName() },
  ],
  averageRating: faker.number.float({ min: 0, max: 5, fractionDigits: 1 }),
  totalReviews: faker.number.int({ min: 0, max: 1000 }),
};

describe("BookCard", () => {
  it("should render book information correctly", () => {
    const book: Book = {
      ...mockBook,
      name: "Test Book Title",
      category: { id: faker.string.uuid(), name: "Fiction" },
      authors: [
        { id: faker.string.uuid(), name: "John Doe" },
        { id: faker.string.uuid(), name: "Jane Smith" },
      ],
      averageRating: 4.5,
    };

    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("Test Book Title")).toBeInTheDocument();
    expect(screen.getByText("Fiction")).toBeInTheDocument();
    expect(screen.getByText("John Doe, Jane Smith")).toBeInTheDocument();
    expect(screen.getByText("4.5")).toBeInTheDocument();
  });

  it("should display discount badge when priceSale is present", () => {
    const book: Book = {
      ...mockBook,
      price: 29.99,
      priceSale: 19.99,
    };

    renderWithProviders(<BookCard book={book} />);

    // Calculate discount: ((29.99 - 19.99) / 29.99) * 100 ≈ 33%
    expect(screen.getByText(/-\d+% OFF/)).toBeInTheDocument();
  });

  it("should not display discount badge when priceSale is null", () => {
    const book: Book = { ...mockBook, priceSale: null };
    renderWithProviders(<BookCard book={book} />);

    expect(screen.queryByText(/-\d+% OFF/)).not.toBeInTheDocument();
  });

  it("should display sale price when available", () => {
    const book: Book = {
      ...mockBook,
      price: 29.99,
      priceSale: 19.99,
    };

    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("$19.99")).toBeInTheDocument();
    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should display only regular price when no sale", () => {
    const book: Book = {
      ...mockBook,
      price: 29.99,
      priceSale: null,
    };

    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("$29.99")).toBeInTheDocument();
  });

  it("should call onClick when card is clicked", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();

    renderWithProviders(<BookCard book={mockBook} onClick={handleClick} />);

    const article = screen.getByRole("article");
    await user.click(article);

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it("should call onClick when Enter key is pressed", async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();

    renderWithProviders(<BookCard book={mockBook} onClick={handleClick} />);

    const article = screen.getByRole("article");
    article.focus();
    await user.keyboard("{Enter}");

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it("should handle multiple authors correctly", () => {
    const book: Book = {
      ...mockBook,
      authors: Array.from({ length: 5 }, () => ({
        id: faker.string.uuid(),
        name: faker.person.fullName(),
      })),
    };

    renderWithProviders(<BookCard book={book} />);

    // Should show first 3 authors plus count
    const authorsText = screen.getByText(/\+2 more/);
    expect(authorsText).toBeInTheDocument();
  });

  it("should display 'Unknown Author' when no authors", () => {
    const book: Book = { ...mockBook, authors: [] };
    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("Unknown Author")).toBeInTheDocument();
  });

  it("should display default rating when averageRating is zero", () => {
    const book: Book = { ...mockBook, averageRating: 0 };
    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("0.0")).toBeInTheDocument();
  });

  it("should render category name", () => {
    const book: Book = {
      ...mockBook,
      category: { id: faker.string.uuid(), name: "Fiction" },
    };

    renderWithProviders(<BookCard book={book} />);

    expect(screen.getByText("Fiction")).toBeInTheDocument();
  });

  it("should have proper accessibility attributes", () => {
    renderWithProviders(<BookCard book={mockBook} onClick={vi.fn()} />);

    const article = screen.getByRole("article");
    expect(article).toHaveAttribute("itemScope");
    expect(article).toHaveAttribute("itemType", "https://schema.org/Book");
    expect(article).toHaveAttribute("tabIndex", "0");
  });

  it("should display book image with proper alt text", () => {
    const book: Book = { ...mockBook, name: "Test Book Title" };
    renderWithProviders(<BookCard book={book} />);

    const image = screen.getByAltText("Test Book Title");
    expect(image).toBeInTheDocument();
  });
});
