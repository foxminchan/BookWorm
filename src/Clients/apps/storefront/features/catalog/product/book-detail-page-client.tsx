"use client";

import { type ChangeEvent, useCallback, useReducer } from "react";

import Link from "next/link";
import { notFound } from "next/navigation";

import { ArrowLeft } from "lucide-react";
import { useDebounceCallback } from "usehooks-ts";

import useBasket from "@workspace/api-hooks/basket/useBasket";
import useCreateBasket from "@workspace/api-hooks/basket/useCreateBasket";
import useDeleteBasket from "@workspace/api-hooks/basket/useDeleteBasket";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useCreateFeedback from "@workspace/api-hooks/rating/useCreateFeedback";
import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import useSummaryFeedback from "@workspace/api-hooks/rating/useSummaryFeedback";
import type { CreateFeedbackRequest } from "@workspace/types/rating";
import { Separator } from "@workspace/ui/components/separator";

import { JsonLd } from "@/components/json-ld";
import { ProductLoadingSkeleton } from "@/components/loading-skeleton";
import { RemoveItemDialog } from "@/components/remove-item-dialog";
import ProductSection from "@/features/catalog/product/product-section";
import ReviewsContainer from "@/features/catalog/product/reviews-container";
import { getReviewSortParams } from "@/lib/pattern";
import { generateBreadcrumbJsonLd, generateProductJsonLd } from "@/lib/seo";

const REVIEWS_PER_PAGE = 5;

const INITIAL_REVIEW: Omit<CreateFeedbackRequest, "bookId"> = {
  firstName: "",
  lastName: "",
  comment: "",
  rating: 0,
};

type BookDetailState = {
  showReviewForm: boolean;
  currentPage: number;
  showRemoveDialog: boolean;
  sortBy: "newest" | "highest" | "lowest";
  newReview: Omit<CreateFeedbackRequest, "bookId">;
};

type BookDetailAction =
  | { type: "TOGGLE_REVIEW_FORM" }
  | { type: "HIDE_REVIEW_FORM" }
  | { type: "SET_PAGE"; page: number }
  | { type: "SHOW_REMOVE_DIALOG" }
  | { type: "HIDE_REMOVE_DIALOG" }
  | { type: "SET_SORT"; sortBy: "newest" | "highest" | "lowest" }
  | { type: "UPDATE_REVIEW"; field: string; value: string | number }
  | { type: "RESET_REVIEW" };

const initialState: BookDetailState = {
  showReviewForm: false,
  currentPage: 1,
  showRemoveDialog: false,
  sortBy: "newest",
  newReview: INITIAL_REVIEW,
};

function bookDetailReducer(
  state: BookDetailState,
  action: BookDetailAction,
): BookDetailState {
  switch (action.type) {
    case "TOGGLE_REVIEW_FORM":
      return { ...state, showReviewForm: !state.showReviewForm };
    case "HIDE_REVIEW_FORM":
      return { ...state, showReviewForm: false };
    case "SET_PAGE":
      return { ...state, currentPage: action.page };
    case "SHOW_REMOVE_DIALOG":
      return { ...state, showRemoveDialog: true };
    case "HIDE_REMOVE_DIALOG":
      return { ...state, showRemoveDialog: false };
    case "SET_SORT":
      return { ...state, sortBy: action.sortBy, currentPage: 1 };
    case "UPDATE_REVIEW":
      return {
        ...state,
        newReview: { ...state.newReview, [action.field]: action.value },
      };
    case "RESET_REVIEW":
      return {
        ...state,
        newReview: INITIAL_REVIEW,
        showReviewForm: false,
      };
    default:
      return state;
  }
}

type BookDetailPageClientProps = {
  id: string;
};

export default function BookDetailPageClient({
  id,
}: Readonly<BookDetailPageClientProps>) {
  const [state, dispatch] = useReducer(bookDetailReducer, initialState);

  // Convert sortBy to API parameters
  const sortParams = getReviewSortParams(state.sortBy);

  // API hooks - these will use the hydrated data from the server
  const { data: book, isLoading: isLoadingBook, isError } = useBook(id);
  const { data: feedbacksData, isLoading: isLoadingFeedbacks } = useFeedbacks({
    bookId: id,
    pageSize: REVIEWS_PER_PAGE,
    pageIndex: state.currentPage,
    orderBy: sortParams.orderBy,
    isDescending: sortParams.isDescending,
  });
  const {
    data: summaryData,
    isLoading: isSummarizing,
    refetch: refetchSummary,
  } = useSummaryFeedback(id, { enabled: false });
  const { data: basket, refetch: refetchBasket } = useBasket();
  const createBasketMutation = useCreateBasket({
    onSuccess: () => {
      refetchBasket();
    },
  });
  const updateBasketMutation = useUpdateBasket({
    onSuccess: () => {
      refetchBasket();
    },
  });
  const deleteBasketMutation = useDeleteBasket();
  const createFeedbackMutation = useCreateFeedback();
  const basketItem = basket?.items?.find((item) => item.id === id);
  const quantity = basketItem?.quantity ?? 0;
  const isLoading = isLoadingBook || isLoadingFeedbacks;
  const feedbacks = feedbacksData?.items ?? [];

  const handleQuantityDecreaseDebounced = useDebounceCallback(
    () => {
      if (quantity === 1) {
        dispatch({ type: "SHOW_REMOVE_DIALOG" });
      } else {
        const newQuantity = Math.max(0, quantity - 1);
        updateBasketMutation.mutate({
          request: {
            items: [{ id: id, quantity: newQuantity }],
          },
        });
      }
    },
    300,
    { leading: true, trailing: false },
  );

  const handleQuantityIncreaseDebounced = useDebounceCallback(
    () => {
      const newQuantity = Math.min(99, quantity + 1);
      updateBasketMutation.mutate({
        request: {
          items: [{ id: id, quantity: newQuantity }],
        },
      });
    },
    300,
    { leading: true, trailing: false },
  );

  const handleSortChange = useCallback(
    (newSort: "newest" | "highest" | "lowest") => {
      dispatch({ type: "SET_SORT", sortBy: newSort });
    },
    [],
  );

  const handlePageChange = useCallback((page: number) => {
    dispatch({ type: "SET_PAGE", page });
  }, []);

  const handleToggleReviewForm = useCallback(() => {
    dispatch({ type: "TOGGLE_REVIEW_FORM" });
  }, []);

  const handleReviewFieldChange = useCallback(
    (field: string, value: string | number) => {
      dispatch({ type: "UPDATE_REVIEW", field, value });
    },
    [],
  );

  if (isLoading) {
    return (
      <main className="container mx-auto grow px-4 py-8">
        <h1 className="sr-only">Loading Book Details</h1>
        <ProductLoadingSkeleton />
      </main>
    );
  }

  if (isError || !book) {
    notFound();
  }

  const handleSummarize = () => {
    refetchSummary();
  };

  const handleSubmitReview = () => {
    createFeedbackMutation.mutate(
      {
        ...state.newReview,
        bookId: id,
      },
      {
        onSuccess: () => {
          dispatch({ type: "RESET_REVIEW" });
        },
      },
    );
  };

  const handleAddToBasket = () => {
    if (quantity > 0) {
      // Update existing basket item
      updateBasketMutation.mutate({
        request: {
          items: [{ id: id, quantity: quantity + 1 }],
        },
      });
    } else {
      // Add new item to basket
      createBasketMutation.mutate({
        items: [{ id: id, quantity: 1 }],
      });
    }
  };

  const handleQuantityChange = (e: ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;

    if (value === "") {
      dispatch({ type: "SHOW_REMOVE_DIALOG" });
      return;
    }

    const numValue = Number.parseInt(value, 10);

    if (!Number.isNaN(numValue) && numValue > 0 && numValue <= 99) {
      updateBasketMutation.mutate({
        request: {
          items: [{ id: id, quantity: numValue }],
        },
      });
    } else if (numValue <= 0) {
      dispatch({ type: "SHOW_REMOVE_DIALOG" });
    }
  };

  const handleConfirmRemove = () => {
    // Filter out the current item from basket
    const remainingItems =
      basket?.items?.filter((item) => item.id !== id) ?? [];

    if (remainingItems.length === 0) {
      // Delete the basket if no items remain
      deleteBasketMutation.mutate(undefined);
    } else {
      // Update basket with remaining items
      updateBasketMutation.mutate({
        request: {
          items: remainingItems.map((item) => ({
            id: item.id,
            quantity: item.quantity,
          })),
        },
      });
    }
    dispatch({ type: "HIDE_REMOVE_DIALOG" });
  };

  const totalCount = feedbacksData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / REVIEWS_PER_PAGE);

  const productJsonLd = generateProductJsonLd(book, feedbacks);
  const breadcrumbJsonLd = generateBreadcrumbJsonLd([
    { name: "Home", url: "/" },
    { name: "Shop", url: "/shop" },
    { name: book.name || "Book", url: `/shop/${book.id}` },
  ]);

  return (
    <>
      {productJsonLd && <JsonLd data={productJsonLd} />}
      {breadcrumbJsonLd && <JsonLd data={breadcrumbJsonLd} />}

      <main className="container mx-auto grow px-4 py-8">
        <Link
          href="/shop"
          className="text-muted-foreground hover:text-primary mb-8 inline-flex items-center text-sm transition-colors"
          rel="prev"
        >
          <ArrowLeft className="mr-2 size-4" aria-hidden="true" /> Back to Shop
        </Link>

        <ProductSection
          book={{
            imageUrl: book.imageUrl,
            name: book.name || "Book",
            priceSale: book.priceSale,
            category: book.category,
            authors: book.authors || [],
            averageRating: book.averageRating || 0,
            totalReviews: book.totalReviews || 0,
            price: book.price || 0,
            status: book.status || "",
            description: book.description,
            publisher: book.publisher,
          }}
          quantity={quantity}
          isAddingToBasket={
            createBasketMutation.isPending || updateBasketMutation.isPending
          }
          onAddToBasket={handleAddToBasket}
          onQuantityChange={handleQuantityChange}
          onDecrease={handleQuantityDecreaseDebounced}
          onIncrease={handleQuantityIncreaseDebounced}
        />

        <Separator className="my-16" aria-hidden="true" />

        {/* Reviews Section */}
        <section
          id="reviews"
          className="mx-auto max-w-4xl"
          aria-label="Customer Reviews"
        >
          <ReviewsContainer
            isLoading={isLoading}
            reviews={feedbacks}
            averageRating={book.averageRating || 0}
            totalReviews={book.totalReviews || 0}
            sortBy={state.sortBy}
            onSortChange={handleSortChange}
            isSummarizing={isSummarizing}
            onSummarize={handleSummarize}
            showReviewForm={state.showReviewForm}
            onToggleReviewForm={handleToggleReviewForm}
            summary={summaryData?.summary}
            reviewForm={{
              firstName: state.newReview.firstName || "",
              lastName: state.newReview.lastName || "",
              comment: state.newReview.comment || "",
              rating: state.newReview.rating,
              isSubmitting: createFeedbackMutation.isPending,
              onChange: handleReviewFieldChange,
              onSubmit: handleSubmitReview,
            }}
            currentPage={state.currentPage}
            totalPages={totalPages}
            onPageChange={handlePageChange}
          />
        </section>

        <RemoveItemDialog
          open={state.showRemoveDialog}
          onOpenChange={(open) =>
            !open && dispatch({ type: "HIDE_REMOVE_DIALOG" })
          }
          onConfirm={handleConfirmRemove}
        />
      </main>
    </>
  );
}
