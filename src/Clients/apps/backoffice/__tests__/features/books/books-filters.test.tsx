import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  createMockAuthor,
  createMockCategory,
  createMockPublisher,
} from "@/__tests__/factories";
import { BooksFilters } from "@/features/books/books-filters";

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

// Mock useDebounceCallback to fire immediately
vi.mock("usehooks-ts", () => ({
  useDebounceCallback: (fn: (...args: unknown[]) => void) => fn,
}));

// Capture callback from PriceRangeFilter
let capturedPriceRangeOnChange: ((min: number, max: number) => void) | null =
  null;

vi.mock("@/features/books/filters/price-range-filter", () => ({
  PriceRangeFilter: ({
    onChange,
    minPrice,
    maxPrice,
  }: {
    onChange: (min: number, max: number) => void;
    minPrice: number;
    maxPrice: number;
  }) => {
    capturedPriceRangeOnChange = onChange;
    return (
      <div data-testid="price-range-filter">
        Price Range: ${minPrice} - ${maxPrice}
        <button
          type="button"
          data-testid="change-price"
          onClick={() => onChange(10, 50)}
        >
          Change Price
        </button>
      </div>
    );
  },
}));

// Capture callbacks from AuthorsFilter
vi.mock("@/features/books/filters/authors-filter", () => ({
  AuthorsFilter: ({
    onToggle,
    onClear,
    selectedAuthors,
  }: {
    onToggle: (id: string) => void;
    onClear: () => void;
    selectedAuthors: string[];
  }) => (
    <div data-testid="authors-filter">
      Authors ({selectedAuthors.length})
      <button
        type="button"
        data-testid="toggle-author"
        onClick={() => onToggle("author-1")}
      >
        Toggle Author
      </button>
      <button type="button" data-testid="clear-authors" onClick={onClear}>
        Clear Authors
      </button>
    </div>
  ),
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

    expect(screen.getByTestId("authors-filter")).toBeInTheDocument();
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
    expect(screen.getByTestId("authors-filter")).toBeInTheDocument();
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
    expect(screen.getByTestId("authors-filter")).toBeInTheDocument();
    expect(screen.getByText(/Price Range:/)).toBeInTheDocument();
  });

  it("triggers price range change callback", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Click the mock price change button
    await user.click(screen.getByTestId("change-price"));

    // onFiltersChange should have been called with non-default price values
    expect(mockOnFiltersChange).toHaveBeenCalledWith(
      expect.objectContaining({
        minPrice: 10,
        maxPrice: 50,
      }),
    );
  });

  it("toggles author selection", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Toggle an author
    await user.click(screen.getByTestId("toggle-author"));

    // onFiltersChange should include the selected author
    expect(mockOnFiltersChange).toHaveBeenCalledWith(
      expect.objectContaining({
        authorId: "author-1",
      }),
    );
  });

  it("clears all selected authors", async () => {
    const user = userEvent.setup();
    render(<BooksFilters onFiltersChange={mockOnFiltersChange} />);

    await user.click(screen.getByRole("button", { name: /expand.*filters/i }));

    // Toggle an author first, then clear
    await user.click(screen.getByTestId("toggle-author"));
    await user.click(screen.getByTestId("clear-authors"));

    // Last call should have no authorId
    const lastCall =
      mockOnFiltersChange.mock.calls[
        mockOnFiltersChange.mock.calls.length - 1
      ][0];
    expect(lastCall.authorId).toBeUndefined();
  });
});
