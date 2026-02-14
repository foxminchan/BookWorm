import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import ShopPageClient from "@/features/catalog/shop/shop-page-client";

// Hoisted mocks
const mockPush = vi.hoisted(() => vi.fn());
const mockUseShopFilters = vi.hoisted(() => vi.fn());
const mockUseShopData = vi.hoisted(() => vi.fn());

vi.mock("next/navigation", async () => {
  const actual =
    await vi.importActual<typeof import("next/navigation")>("next/navigation");
  return {
    ...actual,
    useRouter: () => ({
      push: mockPush,
      replace: vi.fn(),
      prefetch: vi.fn(),
      back: vi.fn(),
    }),
    useSearchParams: () => new URLSearchParams(),
    usePathname: () => "/shop",
  };
});

vi.mock("@/hooks/useShopFilters", () => ({
  useShopFilters: mockUseShopFilters,
}));

vi.mock("@/hooks/useShopData", () => ({
  useShopData: mockUseShopData,
}));

// Mock child components using alias paths (resolved from source file location)
vi.mock("@/features/catalog/shop/book-grid", () => ({
  default: ({
    books,
    isLoading,
    totalPages,
    currentPage,
    onClearFilters,
  }: Record<string, unknown>) => (
    <div
      data-testid="book-grid"
      data-count={Array.isArray(books) ? books.length : 0}
      data-loading={String(isLoading)}
      data-pages={totalPages}
      data-page={currentPage}
    >
      <button
        type="button"
        data-testid="clear-filters"
        onClick={onClearFilters as () => void}
      >
        Clear
      </button>
    </div>
  ),
}));

vi.mock("@/features/catalog/shop/shop-filters", () => ({
  default: ({
    onToggleCategory,
    onTogglePublisher,
    onToggleAuthor,
  }: Record<string, unknown>) => (
    <div data-testid="shop-filters">
      <button
        type="button"
        data-testid="toggle-category"
        onClick={() => (onToggleCategory as (id: string) => void)("cat-1")}
      >
        Toggle Cat
      </button>
      <button
        type="button"
        data-testid="toggle-publisher"
        onClick={() => (onTogglePublisher as (id: string) => void)("pub-1")}
      >
        Toggle Pub
      </button>
      <button
        type="button"
        data-testid="toggle-author"
        onClick={() => (onToggleAuthor as (id: string) => void)("auth-1")}
      >
        Toggle Author
      </button>
    </div>
  ),
}));

vi.mock("@/features/catalog/shop/shop-toolbar", () => ({
  default: ({
    onClearSearch,
    onOpenFilters,
    onSortChange,
  }: Record<string, unknown>) => (
    <div data-testid="shop-toolbar">
      <button
        type="button"
        data-testid="clear-search"
        onClick={onClearSearch as () => void}
      >
        Clear Search
      </button>
      <button
        type="button"
        data-testid="open-filters"
        onClick={onOpenFilters as () => void}
      >
        Open Filters
      </button>
      <button
        type="button"
        data-testid="sort-change"
        onClick={() => (onSortChange as (sort: string) => void)("price-low")}
      >
        Sort
      </button>
    </div>
  ),
}));

const defaultFilterState = {
  currentPage: 1,
  setCurrentPage: vi.fn(),
  priceRange: [0, 100],
  setPriceRange: vi.fn(),
  selectedAuthors: [] as string[],
  setSelectedAuthors: vi.fn(),
  selectedPublishers: [] as string[],
  setSelectedPublishers: vi.fn(),
  selectedCategories: [] as string[],
  setSelectedCategories: vi.fn(),
  searchQuery: "",
  sortBy: "name",
  setSortBy: vi.fn(),
  isFilterOpen: false,
  setIsFilterOpen: vi.fn(),
};

const defaultShopData = {
  booksData: {
    items: [
      { id: "1", name: "Book 1", price: 10 },
      { id: "2", name: "Book 2", price: 20 },
    ],
    totalCount: 2,
  },
  categories: [],
  publishers: [],
  authors: [],
  isLoading: false,
};

function setupMocks(
  filterOverrides: Partial<typeof defaultFilterState> = {},
  dataOverrides: Partial<typeof defaultShopData> = {},
) {
  mockUseShopFilters.mockReturnValue({
    ...defaultFilterState,
    ...filterOverrides,
  });
  mockUseShopData.mockReturnValue({
    ...defaultShopData,
    ...dataOverrides,
  });
}

describe("ShopPageClient", () => {
  const user = userEvent.setup();

  beforeEach(() => {
    vi.clearAllMocks();
    setupMocks();
  });

  it("renders shop page with child components", () => {
    renderWithProviders(<ShopPageClient />);

    expect(screen.getByTestId("shop-filters")).toBeInTheDocument();
    expect(screen.getByTestId("shop-toolbar")).toBeInTheDocument();
    expect(screen.getByTestId("book-grid")).toBeInTheDocument();
  });

  it("renders sr-only heading", () => {
    renderWithProviders(<ShopPageClient />);

    expect(
      screen.getByRole("heading", { name: /shop books/i, level: 1 }),
    ).toBeInTheDocument();
  });

  it("passes correct book count to grid", () => {
    renderWithProviders(<ShopPageClient />);

    const grid = screen.getByTestId("book-grid");
    expect(grid).toHaveAttribute("data-count", "2");
  });

  it("calculates total pages correctly", () => {
    setupMocks({}, { booksData: { items: [], totalCount: 20 } });
    renderWithProviders(<ShopPageClient />);

    const grid = screen.getByTestId("book-grid");
    // 20 items / 8 per page = 3 pages (ceil)
    expect(grid).toHaveAttribute("data-pages", "3");
  });

  it("handles empty book data", () => {
    setupMocks(
      {},
      { booksData: undefined as unknown as typeof defaultShopData.booksData },
    );
    renderWithProviders(<ShopPageClient />);

    const grid = screen.getByTestId("book-grid");
    expect(grid).toHaveAttribute("data-count", "0");
    expect(grid).toHaveAttribute("data-pages", "0");
  });

  it("navigates on category toggle", async () => {
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-category"));

    expect(defaultFilterState.setSelectedCategories).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith(
      expect.stringContaining("category=cat-1"),
    );
  });

  it("navigates on publisher toggle", async () => {
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-publisher"));

    expect(defaultFilterState.setSelectedPublishers).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith(
      expect.stringContaining("publisher=pub-1"),
    );
  });

  it("navigates on author toggle", async () => {
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-author"));

    expect(defaultFilterState.setSelectedAuthors).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith(
      expect.stringContaining("author=auth-1"),
    );
  });

  it("removes category when already selected", async () => {
    setupMocks({ selectedCategories: ["cat-1"] });
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-category"));

    // Should deselect cat-1 since it's already selected
    expect(defaultFilterState.setSelectedCategories).toHaveBeenCalledWith([]);
  });

  it("removes publisher when already selected", async () => {
    setupMocks({ selectedPublishers: ["pub-1"] });
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-publisher"));

    expect(defaultFilterState.setSelectedPublishers).toHaveBeenCalledWith([]);
  });

  it("removes author when already selected", async () => {
    setupMocks({ selectedAuthors: ["auth-1"] });
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("toggle-author"));

    expect(defaultFilterState.setSelectedAuthors).toHaveBeenCalledWith([]);
  });

  it("clears search by navigating with current filters", async () => {
    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("clear-search"));

    expect(mockPush).toHaveBeenCalledWith(expect.stringContaining("page=1"));
  });

  it("clears all filters and resets state", async () => {
    setupMocks({
      selectedCategories: ["cat-1"],
      selectedPublishers: ["pub-1"],
      selectedAuthors: ["auth-1"],
    });

    renderWithProviders(<ShopPageClient />);

    await user.click(screen.getByTestId("clear-filters"));

    expect(defaultFilterState.setSelectedCategories).toHaveBeenCalledWith([]);
    expect(defaultFilterState.setSelectedPublishers).toHaveBeenCalledWith([]);
    expect(defaultFilterState.setSelectedAuthors).toHaveBeenCalledWith([]);
    expect(defaultFilterState.setPriceRange).toHaveBeenCalledWith([0, 100]);
    expect(defaultFilterState.setCurrentPage).toHaveBeenCalledWith(1);
    expect(mockPush).toHaveBeenCalledWith("/shop?page=1");
  });

  it("shows loading state", () => {
    setupMocks({}, { isLoading: true });
    renderWithProviders(<ShopPageClient />);

    const grid = screen.getByTestId("book-grid");
    expect(grid).toHaveAttribute("data-loading", "true");
  });

  it("handles non-array items in booksData", () => {
    setupMocks(
      {},
      {
        booksData: {
          items: null as unknown as typeof defaultShopData.booksData.items,
          totalCount: 0,
        },
      },
    );
    renderWithProviders(<ShopPageClient />);

    const grid = screen.getByTestId("book-grid");
    expect(grid).toHaveAttribute("data-count", "0");
  });
});
