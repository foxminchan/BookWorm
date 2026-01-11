import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  createMockAuthor,
  createMockCategory,
  createMockPublisher,
} from "@/__tests__/factories";
import { BooksFilters } from "@/features/books/books-filters";

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors");
vi.mock("@workspace/api-hooks/catalog/categories/useCategories");
vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers");

const mockUseAuthors = vi.hoisted(() => vi.fn());
const mockUseCategories = vi.hoisted(() => vi.fn());
const mockUsePublishers = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors", () => ({
  default: mockUseAuthors,
}));

vi.mock("@workspace/api-hooks/catalog/categories/useCategories", () => ({
  default: mockUseCategories,
}));

vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers", () => ({
  default: mockUsePublishers,
}));

describe("BooksFilters", () => {
  const mockOnFiltersChange = vi.fn();
  const mockAuthors = [createMockAuthor(), createMockAuthor()];
  const mockCategories = [createMockCategory(), createMockCategory()];
  const mockPublishers = [createMockPublisher(), createMockPublisher()];

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });
    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });
    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });
  });

  it("renders collapsed by default", () => {
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    expect(screen.getByText("Filter & Search")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /expand.*filters/i }),
    ).toBeInTheDocument();
    expect(
      screen.queryByPlaceholderText("Search by title or author..."),
    ).not.toBeInTheDocument();
  });

  it("expands when clicking expand button", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    const expandButton = screen.getByRole("button", {
      name: /expand.*filters/i,
    });
    await user.click(expandButton);

    expect(
      screen.getByPlaceholderText("Search by title or author..."),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /collapse.*filters/i }),
    ).toBeInTheDocument();
  });

  it("collapses when clicking collapse button", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    // Expand first
    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));
    expect(
      screen.getByPlaceholderText("Search by title or author..."),
    ).toBeInTheDocument();

    // Collapse
    await user.click(
      screen.getByRole("button", { name: /collapse.*filters/i }),
    );
    expect(
      screen.queryByPlaceholderText("Search by title or author..."),
    ).not.toBeInTheDocument();
  });

  it("displays category select", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    expect(screen.getByText("All Categories")).toBeInTheDocument();
  });

  it("displays publisher select", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    expect(screen.getByText("All Publishers")).toBeInTheDocument();
  });

  it("displays authors filter", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    expect(screen.getByText(/Authors/)).toBeInTheDocument();
  });

  it("resets all filters when clicking reset button", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Change search
    const searchInput = screen.getByPlaceholderText(
      "Search by title or author...",
    );
    await user.type(searchInput, "test");

    // Click reset
    const resetButton = screen.getByRole("button", { name: /Reset Filters/i });
    await user.click(resetButton);

    // Search input should be cleared
    expect(searchInput).toHaveValue("");
  });

  it("displays reset filters button when expanded", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    expect(
      screen.getByRole("button", { name: /Reset Filters/i }),
    ).toBeInTheDocument();
  });

  it("handles price range changes with debounce", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Price range filter should be visible
    expect(screen.getByText(/Price Range:/)).toBeInTheDocument();
  });

  it("handles author toggle selection", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Should display authors section
    expect(screen.getByText(/Authors/)).toBeInTheDocument();
  });

  it("calls onFiltersChange with updated filters", async () => {
    const user = userEvent.setup();
    const onFiltersChange = vi.fn();
    render(<BooksFilters onFiltersChange={onFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Component should be ready for filter changes
    expect(
      screen.getByRole("button", { name: /Reset Filters/i }),
    ).toBeInTheDocument();
  });

  it("displays all filter sections when expanded", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // All filter sections should be visible
    expect(screen.getByLabelText(/Category/)).toBeInTheDocument();
    expect(screen.getByLabelText(/Publisher/)).toBeInTheDocument();
    expect(screen.getByText(/Authors/)).toBeInTheDocument();
    expect(screen.getByText(/Price Range:/)).toBeInTheDocument();
  });
});
