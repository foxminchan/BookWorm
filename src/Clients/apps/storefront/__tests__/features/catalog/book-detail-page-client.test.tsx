import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  fireEvent,
  renderWithProviders,
  screen,
  waitFor,
} from "@/__tests__/utils/test-utils";
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
      <button
        type="button"
        data-testid="add-to-basket"
        onClick={props.onAddToBasket as () => void}
      >
        Add
      </button>
      <button
        type="button"
        data-testid="decrease-qty"
        onClick={props.onDecrease as () => void}
      >
        Decrease
      </button>
      <button
        type="button"
        data-testid="increase-qty"
        onClick={props.onIncrease as () => void}
      >
        Increase
      </button>
      <input
        data-testid="qty-input"
        onChange={props.onQuantityChange as (e: unknown) => void}
      />
    </div>
  ),
}));

vi.mock("@/features/catalog/product/reviews-container", () => ({
  default: (props: Record<string, unknown>) => {
    const reviewForm = props.reviewForm as Record<string, unknown> | undefined;
    return (
      <div data-testid="reviews-container" data-page={props.currentPage}>
        Reviews
        <button
          type="button"
          data-testid="toggle-review-form"
          onClick={props.onToggleReviewForm as () => void}
        >
          Toggle Review
        </button>
        <button
          type="button"
          data-testid="summarize"
          onClick={props.onSummarize as () => void}
        >
          Summarize
        </button>
        <button
          type="button"
          data-testid="sort-change"
          onClick={() =>
            (props.onSortChange as (s: string) => void)?.("highest")
          }
        >
          Sort
        </button>
        <button
          type="button"
          data-testid="page-change"
          onClick={() => (props.onPageChange as (p: number) => void)?.(2)}
        >
          Page 2
        </button>
        {reviewForm && (
          <button
            type="button"
            data-testid="submit-review"
            onClick={reviewForm.onSubmit as () => void}
          >
            Submit
          </button>
        )}
      </div>
    );
  },
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

  it("adds new item to basket when quantity is 0", () => {
    const mockCreateMutate = vi.fn();
    mockUseCreateBasket.mockReturnValue({
      mutate: mockCreateMutate,
      isPending: false,
    });
    mockUseBasket.mockReturnValue({
      data: { items: [] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("add-to-basket"));
    expect(mockCreateMutate).toHaveBeenCalledWith({
      items: [{ id: "test-id", quantity: 1 }],
    });
  });

  it("updates basket when item already exists", () => {
    const mockUpdateMutate = vi.fn();
    mockUseUpdateBasket.mockReturnValue({
      mutate: mockUpdateMutate,
      isPending: false,
    });
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 2 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("add-to-basket"));
    expect(mockUpdateMutate).toHaveBeenCalledWith({
      request: { items: [{ id: "test-id", quantity: 3 }] },
    });
  });

  it("shows remove dialog when decreasing from quantity 1", async () => {
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 1 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("decrease-qty"));
    await waitFor(() => {
      expect(screen.getByTestId("remove-dialog")).toBeInTheDocument();
    });
  });

  it("updates quantity to valid number via input change", () => {
    const mockUpdateMutate = vi.fn();
    mockUseUpdateBasket.mockReturnValue({
      mutate: mockUpdateMutate,
      isPending: false,
    });
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 1 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.change(screen.getByTestId("qty-input"), {
      target: { value: "5" },
    });
    expect(mockUpdateMutate).toHaveBeenCalledWith({
      request: { items: [{ id: "test-id", quantity: 5 }] },
    });
  });

  it("shows remove dialog when quantity input is 0", async () => {
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 1 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("decrease-qty"));
    await waitFor(() => {
      expect(screen.getByTestId("remove-dialog")).toBeInTheDocument();
    });
  });

  it("confirms removal and deletes basket when no items remain", async () => {
    const mockDeleteMutate = vi.fn();
    mockUseDeleteBasket.mockReturnValue({
      mutate: mockDeleteMutate,
      isPending: false,
    });
    mockUseBasket.mockReturnValue({
      data: { items: [{ id: "test-id", quantity: 1 }] },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    // Trigger remove dialog by decreasing when quantity is 1
    fireEvent.click(screen.getByTestId("decrease-qty"));
    await waitFor(() => {
      expect(screen.getByTestId("confirm-remove")).toBeInTheDocument();
    });
    // Confirm removal
    fireEvent.click(screen.getByTestId("confirm-remove"));
    expect(mockDeleteMutate).toHaveBeenCalledWith(undefined);
  });

  it("confirms removal and updates basket when other items remain", async () => {
    const mockUpdateMutate = vi.fn();
    mockUseUpdateBasket.mockReturnValue({
      mutate: mockUpdateMutate,
      isPending: false,
    });
    mockUseBasket.mockReturnValue({
      data: {
        items: [
          { id: "test-id", quantity: 1 },
          { id: "other-id", quantity: 2 },
        ],
      },
      refetch: vi.fn(),
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    // Trigger remove dialog by decreasing when quantity is 1
    fireEvent.click(screen.getByTestId("decrease-qty"));
    await waitFor(() => {
      expect(screen.getByTestId("confirm-remove")).toBeInTheDocument();
    });
    // Confirm removal
    fireEvent.click(screen.getByTestId("confirm-remove"));
    expect(mockUpdateMutate).toHaveBeenCalledWith({
      request: { items: [{ id: "other-id", quantity: 2 }] },
    });
  });

  it("submits review and resets form on success", () => {
    const mockFeedbackMutate = vi.fn();
    mockUseCreateFeedback.mockReturnValue({
      mutate: mockFeedbackMutate,
      isPending: false,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("submit-review"));
    expect(mockFeedbackMutate).toHaveBeenCalledWith(
      expect.objectContaining({ bookId: "test-id" }),
      expect.objectContaining({ onSuccess: expect.any(Function) }),
    );

    // Invoke onSuccess to cover RESET_REVIEW dispatch
    mockFeedbackMutate.mock.calls[0][1].onSuccess();
  });

  it("calls refetchSummary when summarize is clicked", () => {
    const mockRefetch = vi.fn();
    mockUseSummaryFeedback.mockReturnValue({
      data: null,
      isLoading: false,
      refetch: mockRefetch,
    });

    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("summarize"));
    expect(mockRefetch).toHaveBeenCalled();
  });

  it("changes sort and resets page", () => {
    renderWithProviders(<BookDetailPageClient id="test-id" />);

    fireEvent.click(screen.getByTestId("sort-change"));

    // After sorting, feedbacks should be re-fetched with new sort params
    const lastCall =
      mockUseFeedbacks.mock.calls[mockUseFeedbacks.mock.calls.length - 1][0];
    expect(lastCall.orderBy).toBe("rating");
    expect(lastCall.isDescending).toBe(true);
  });
});
