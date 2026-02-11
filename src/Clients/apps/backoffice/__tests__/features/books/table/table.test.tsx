import { screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createMockBook } from "@/__tests__/factories";
import { renderWithProviders } from "@/__tests__/utils/test-utils";
import { BooksTable } from "@/features/books/table/table";

// Mock the data hook
const mockUseBooks = vi.hoisted(() => vi.fn());
vi.mock("@workspace/api-hooks/catalog/books/useBooks", () => ({
  default: mockUseBooks,
}));

// Mock the delete hook used by CellAction
vi.mock("@workspace/api-hooks/catalog/books/useDeleteBook", () => ({
  default: () => ({ mutate: vi.fn(), isPending: false }),
}));

describe("BooksTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render loading state", () => {
    mockUseBooks.mockReturnValue({
      data: undefined,
      isLoading: true,
    });

    renderWithProviders(<BooksTable query={{}} />);

    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("should render books data in table", () => {
    const books = [
      createMockBook({ name: "Clean Code" }),
      createMockBook({ name: "Design Patterns" }),
    ];

    mockUseBooks.mockReturnValue({
      data: { items: books, totalCount: 2 },
      isLoading: false,
    });

    renderWithProviders(<BooksTable query={{}} />);

    expect(screen.getByText("Clean Code")).toBeInTheDocument();
    expect(screen.getByText("Design Patterns")).toBeInTheDocument();
  });

  it("should display total count in description", () => {
    mockUseBooks.mockReturnValue({
      data: { items: [], totalCount: 42 },
      isLoading: false,
    });

    renderWithProviders(<BooksTable query={{}} />);

    expect(
      screen.getAllByText("Total: 42 books").length,
    ).toBeGreaterThanOrEqual(1);
  });

  it("should render table title", () => {
    mockUseBooks.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(<BooksTable query={{}} />);

    expect(screen.getByText("All Books")).toBeInTheDocument();
  });

  it("should handle empty data gracefully", () => {
    mockUseBooks.mockReturnValue({
      data: undefined,
      isLoading: false,
    });

    renderWithProviders(<BooksTable query={{}} />);

    expect(screen.getAllByText("Total: 0 books").length).toBeGreaterThanOrEqual(
      1,
    );
  });

  it("should pass query filters to the hook", () => {
    mockUseBooks.mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    });

    renderWithProviders(
      <BooksTable query={{ categoryId: "cat-1", search: "test" }} />,
    );

    expect(mockUseBooks).toHaveBeenCalledWith(
      expect.objectContaining({
        categoryId: "cat-1",
        search: "test",
        pageIndex: 1,
        pageSize: 10,
      }),
    );
  });
});
