import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { createMockBook } from "@/__tests__/factories";
import { BookSelector } from "@/features/reviews/book-selector";

vi.mock("@workspace/api-hooks/catalog/books/useBooks");

const mockUseBooks = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/books/useBooks", () => ({
  default: mockUseBooks,
}));

describe("BookSelector", () => {
  const mockBooks = [
    createMockBook({ status: "InStock", averageRating: 4.5, totalReviews: 10 }),
    createMockBook({ status: "InStock", averageRating: 3.8, totalReviews: 5 }),
  ];

  it("renders selector label", () => {
    mockUseBooks.mockReturnValue({
      data: { items: mockBooks },
      isLoading: false,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    expect(screen.getByText("Select a Book")).toBeInTheDocument();
  });

  it("renders placeholder text", () => {
    mockUseBooks.mockReturnValue({
      data: { items: [] },
      isLoading: false,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    expect(
      screen.getByText("Choose a book to view reviews..."),
    ).toBeInTheDocument();
  });

  it("disables selector when loading", () => {
    mockUseBooks.mockReturnValue({
      data: null,
      isLoading: true,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    expect(screen.getByRole("combobox")).toBeDisabled();
  });

  it("handles empty book list", () => {
    mockUseBooks.mockReturnValue({
      data: { items: [] },
      isLoading: false,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    expect(
      screen.getByText("Choose a book to view reviews..."),
    ).toBeInTheDocument();
  });

  // Note: Radix UI Select dropdown doesn't render in test environment
  // Skipping tests that require interacting with dropdown options

  it("has proper label association", () => {
    mockUseBooks.mockReturnValue({
      data: { items: mockBooks },
      isLoading: false,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    const label = screen.getByText("Select a Book");
    const select = screen.getByRole("combobox");

    expect(label).toHaveAttribute("for", "book-select");
    expect(select).toHaveAttribute("id", "book-select");
  });

  it("handles undefined data gracefully", () => {
    mockUseBooks.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    render(<BookSelector onBookSelect={vi.fn()} />);

    expect(
      screen.getByText("Choose a book to view reviews..."),
    ).toBeInTheDocument();
  });
});
