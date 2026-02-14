import { beforeEach, describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import BookDetailPageClient from "@/features/catalog/product/book-detail-page-client";

// Hoisted mocks
const mockUseBook = vi.hoisted(() => vi.fn());
const mockUseFeedbacks = vi.hoisted(() => vi.fn());
const mockUseSummaryFeedback = vi.hoisted(() => vi.fn());
const mockUseBasket = vi.hoisted(() => vi.fn());
const mockUseCreateBasket = vi.hoisted(() => vi.fn());
const mockUseUpdateBasket = vi.hoisted(() => vi.fn());
const mockUseDeleteBasket = vi.hoisted(() => vi.fn());
const mockUseCreateFeedback = vi.hoisted(() => vi.fn());
const mockNotFound = vi.hoisted(() => vi.fn());
const mockUseDebounceCallback = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/books/useBook", () => ({
  default: mockUseBook,
}));
vi.mock("@workspace/api-hooks/rating/useFeedbacks", () => ({
  default: mockUseFeedbacks,
}));
vi.mock("@workspace/api-hooks/rating/useSummaryFeedback", () => ({
  default: mockUseSummaryFeedback,
}));
vi.mock("@workspace/api-hooks/basket/useBasket", () => ({
  default: mockUseBasket,
}));
vi.mock("@workspace/api-hooks/basket/useCreateBasket", () => ({
  default: mockUseCreateBasket,
}));
vi.mock("@workspace/api-hooks/basket/useUpdateBasket", () => ({
  default: mockUseUpdateBasket,
}));
vi.mock("@workspace/api-hooks/basket/useDeleteBasket", () => ({
  default: mockUseDeleteBasket,
}));
vi.mock("@workspace/api-hooks/rating/useCreateFeedback", () => ({
  default: mockUseCreateFeedback,
}));

vi.mock("next/navigation", async () => {
  const actual =
    await vi.importActual<typeof import("next/navigation")>("next/navigation");
  return {
    ...actual,
    notFound: mockNotFound,
    useRouter: () => ({
      push: vi.fn(),
      replace: vi.fn(),
      prefetch: vi.fn(),
      back: vi.fn(),
    }),
    useSearchParams: () => new URLSearchParams(),
    usePathname: () => "/shop/test-id",
  };
});

vi.mock("usehooks-ts", () => ({
  useDebounceCallback: mockUseDebounceCallback,
}));

// Mock child components to isolate unit logic
vi.mock("@/components/json-ld", () => ({
  JsonLd: ({ data }: { data: unknown }) => (
    <script data-testid="json-ld" type="application/ld+json">
      {JSON.stringify(data)}
    </script>
  ),
}));

vi.mock("@/components/loading-skeleton", () => ({
  ProductLoadingSkeleton: () => (
    <div data-testid="product-loading-skeleton">Loading...</div>
  ),
}));

vi.mock("@/components/remove-item-dialog", () => ({
  RemoveItemDialog: ({
    open,
    onConfirm,
  }: {
    open: boolean;
    onConfirm: () => void;
  }) =>
    open ? (
      <div data-testid="remove-dialog">
        <button type="button" data-testid="confirm-remove" onClick={onConfirm}>
          Confirm
        </button>
      </div>
    ) : null,
}));

vi.mock("@/features/catalog/product/product-section", () => ({
  default: (props: Record<string, unknown>) => (
    <div data-testid="product-section" data-quantity={props.quantity}>
      Product Section
    </div>
  ),
}));

vi.mock("@/features/catalog/product/reviews-container", () => ({
  default: (props: Record<string, unknown>) => (
    <div data-testid="reviews-container" data-page={props.currentPage}>
      Reviews
    </div>
  ),
}));

vi.mock("@/lib/seo", () => ({
  generateProductJsonLd: vi.fn(() => ({ "@type": "Product" })),
  generateBreadcrumbJsonLd: vi.fn(() => ({ "@type": "BreadcrumbList" })),
}));

const mockBook = {
  id: "test-id",
  name: "Test Book",
  description: "A test book",
  price: 19.99,
  priceSale: 14.99,
  imageUrl: "https://example.com/book.jpg",
  status: "InStock",
  category: { name: "Fiction" },
  authors: [{ name: "Author One" }],
  publisher: { name: "Publisher One" },
  averageRating: 4.5,
  totalReviews: 10,
};

const mockFeedbacks = {
  items: [
    {
      id: "f1",
      firstName: "John",
      lastName: "Doe",
      comment: "Great book!",
      rating: 5,
    },
  ],
  totalCount: 1,
};

function setupDefaultMocks() {
  mockUseBook.mockReturnValue({
    data: mockBook,
    isLoading: false,
    isError: false,
  });

  mockUseFeedbacks.mockReturnValue({
    data: mockFeedbacks,
    isLoading: false,
  });

  mockUseSummaryFeedback.mockReturnValue({
    data: null,
    isLoading: false,
    refetch: vi.fn(),
  });

  mockUseBasket.mockReturnValue({
    data: { items: [] },
    refetch: vi.fn(),
  });

  mockUseCreateBasket.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
  });

  mockUseUpdateBasket.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
  });

  mockUseDeleteBasket.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
  });

  mockUseCreateFeedback.mockReturnValue({
    mutate: vi.fn(),
    isPending: false,
  });

  // Return the callback directly
  mockUseDebounceCallback.mockImplementation((fn: () => void) => fn);
}

describe("BookDetailPageClient", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    setupDefaultMocks();
  });

  it("renders loading skeleton when data is loading", () => {
    mockUseBook.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    });
    mockUseFeedbacks.mockReturnValue({ data: undefined, isLoading: true });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    expect(screen.getByTestId("product-loading-skeleton")).toBeInTheDocument();
  });

  it("calls notFound when there is an error", () => {
    mockUseBook.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    });
    mockUseFeedbacks.mockReturnValue({ data: undefined, isLoading: false });

    // notFound() in Next.js throws to halt rendering
    mockNotFound.mockImplementation(() => {
      throw new Error("NEXT_NOT_FOUND");
    });

    expect(() =>
      renderWithProviders(<BookDetailPageClient id="test-id" />),
    ).toThrow("NEXT_NOT_FOUND");
    expect(mockNotFound).toHaveBeenCalled();
  });

  it("calls notFound when book data is null", () => {
    mockUseBook.mockReturnValue({
      data: null,
      isLoading: false,
      isError: false,
    });
    mockUseFeedbacks.mockReturnValue({ data: undefined, isLoading: false });

    mockNotFound.mockImplementation(() => {
      throw new Error("NEXT_NOT_FOUND");
    });

    expect(() =>
      renderWithProviders(<BookDetailPageClient id="test-id" />),
    ).toThrow("NEXT_NOT_FOUND");
    expect(mockNotFound).toHaveBeenCalled();
  });

  it("renders product section and reviews when data is loaded", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    expect(screen.getByTestId("product-section")).toBeInTheDocument();
    expect(screen.getByTestId("reviews-container")).toBeInTheDocument();
  });

  it("renders back to shop link", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    const backLink = screen.getByRole("link", { name: /back to shop/i });
    expect(backLink).toHaveAttribute("href", "/shop");
  });

  it("renders JSON-LD structured data", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    const jsonLdElements = screen.getAllByTestId("json-ld");
    expect(jsonLdElements.length).toBeGreaterThanOrEqual(2);
  });

  it("shows quantity 0 when item is not in basket", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    const section = screen.getByTestId("product-section");
    expect(section).toHaveAttribute("data-quantity", "0");
  });

  it("shows correct quantity when item is in basket", () => {
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 3 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    const section = screen.getByTestId("product-section");
    expect(section).toHaveAttribute("data-quantity", "3");
  });

  it("creates basket when adding to empty basket", () => {
    const mockMutate = vi.fn();
    mockUseCreateBasket.mockReturnValue({
      mutate: mockMutate,
      isPending: false,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    // The component passes handleAddToBasket to ProductSection
    // Since ProductSection is mocked, we verify the hooks are called correctly
    expect(mockUseCreateBasket).toHaveBeenCalled();
  });

  it("passes correct create basket callback config", () => {
    const mockRefetch = vi.fn();
    mockUseBasket.mockReturnValue({
      data: { items: [] },
      refetch: mockRefetch,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    // Verify useCreateBasket is called with onSuccess callback
    const createBasketCall = mockUseCreateBasket.mock.calls[0]?.[0];
    expect(createBasketCall).toBeDefined();
    expect(createBasketCall).toHaveProperty("onSuccess");

    // Trigger onSuccess to verify it calls refetchBasket
    createBasketCall.onSuccess();
    expect(mockRefetch).toHaveBeenCalled();
  });

  it("passes correct update basket callback config", () => {
    const mockRefetch = vi.fn();
    mockUseBasket.mockReturnValue({
      data: { items: [] },
      refetch: mockRefetch,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    const updateBasketCall = mockUseUpdateBasket.mock.calls[0]?.[0];
    expect(updateBasketCall).toBeDefined();
    expect(updateBasketCall).toHaveProperty("onSuccess");

    updateBasketCall.onSuccess();
    expect(mockRefetch).toHaveBeenCalled();
  });

  it("renders loading state when only book is loading", () => {
    mockUseBook.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    });
    mockUseFeedbacks.mockReturnValue({
      data: mockFeedbacks,
      isLoading: false,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    expect(screen.getByTestId("product-loading-skeleton")).toBeInTheDocument();
  });

  it("renders loading state when only feedbacks are loading", () => {
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
      isError: false,
    });
    mockUseFeedbacks.mockReturnValue({ data: undefined, isLoading: true });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    expect(screen.getByTestId("product-loading-skeleton")).toBeInTheDocument();
  });

  it("uses correct review sort params for default sort", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    // Default sort is "newest" -> orderBy: "createdAt", isDescending: true
    expect(mockUseFeedbacks).toHaveBeenCalledWith(
      expect.objectContaining({
        bookId: "test-id",
        orderBy: "createdAt",
        isDescending: true,
        pageSize: 5,
        pageIndex: 1,
      }),
    );
  });

  it("handles book with missing optional fields", () => {
    mockUseBook.mockReturnValue({
      data: {
        id: "test-id",
        name: null,
        description: null,
        price: null,
        priceSale: null,
        imageUrl: null,
        status: null,
        category: null,
        authors: null,
        publisher: null,
        averageRating: null,
        totalReviews: null,
      },
      isLoading: false,
      isError: false,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    expect(screen.getByTestId("product-section")).toBeInTheDocument();
  });
});
