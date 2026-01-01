"use client";

import type React from "react";
import { useDebounceCallback } from "usehooks-ts";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { RemoveItemDialog } from "@/components/remove-item-dialog";
import { ProductLoadingSkeleton } from "@/components/loading-skeleton";
import { Separator } from "@workspace/ui/components/separator";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import type { CreateFeedbackRequest } from "@workspace/types/rating";
import { useState, use } from "react";
import { notFound } from "next/navigation";
import { JsonLd } from "@/components/json-ld";
import { generateProductJsonLd, generateBreadcrumbJsonLd } from "@/lib/seo";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import useCreateFeedback from "@workspace/api-hooks/rating/useCreateFeedback";
import useSummaryFeedback from "@workspace/api-hooks/rating/useSummaryFeedback";
import useBasket from "@workspace/api-hooks/basket/useBasket";
import useCreateBasket from "@workspace/api-hooks/basket/useCreateBasket";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";
import useDeleteBasket from "@workspace/api-hooks/basket/useDeleteBasket";
import { getReviewSortParams } from "@/lib/pattern";
import { ProductSection } from "@/features/catalog/product/product-section";
import ReviewsContainer from "@/features/catalog/product/reviews-container";

const REVIEWS_PER_PAGE = 5;

interface BookDetailPageProps {
  params: Promise<{ id: string }>;
}

export default function BookDetailPage({ params }: BookDetailPageProps) {
  const { id } = use(params);

  const [showReviewForm, setShowReviewForm] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [showRemoveDialog, setShowRemoveDialog] = useState(false);
  const [sortBy, setSortBy] = useState<"newest" | "highest" | "lowest">(
    "newest",
  );
  const [newReview, setNewReview] = useState<
    Omit<CreateFeedbackRequest, "bookId">
  >({
    firstName: "",
    lastName: "",
    comment: "",
    rating: 0,
  });

  // Convert sortBy to API parameters
  const sortParams = getReviewSortParams(sortBy);

  // API hooks
  const { data: book, isLoading: isLoadingBook, isError } = useBook(id);
  const { data: feedbacksData, isLoading: isLoadingFeedbacks } = useFeedbacks({
    bookId: id,
    pageSize: REVIEWS_PER_PAGE,
    pageIndex: currentPage,
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

  // Check if item is in basket
  const basketItem = basket?.items?.find((item) => item.id === id);
  const quantity = basketItem?.quantity ?? 0;
  const isLoading = isLoadingBook || isLoadingFeedbacks;
  const feedbacks = feedbacksData?.items ?? [];

  const handleQuantityDecreaseDebounced = useDebounceCallback(
    () => {
      if (quantity === 1) {
        setShowRemoveDialog(true);
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

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-8">
          <ProductLoadingSkeleton />
        </main>
        <Footer />
      </div>
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
        ...newReview,
        bookId: id,
      },
      {
        onSuccess: () => {
          setNewReview({
            firstName: "",
            lastName: "",
            comment: "",
            rating: 0,
          });
          setShowReviewForm(false);
        },
      },
    );
  };

  const handleAddToBasket = async () => {
    if (!book) return;

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

  const handleQuantityChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;

    if (value === "") {
      setShowRemoveDialog(true);
      return;
    }

    const numValue = Number.parseInt(value, 10);

    if (!isNaN(numValue) && numValue > 0 && numValue <= 99) {
      updateBasketMutation.mutate({
        request: {
          items: [{ id: id, quantity: numValue }],
        },
      });
    } else if (numValue <= 0) {
      setShowRemoveDialog(true);
    }
  };

  const handleConfirmRemove = () => {
    // Filter out the current item from basket
    const remainingItems =
      basket?.items?.filter((item) => item.id !== id) ?? [];

    if (remainingItems.length === 0) {
      // Delete the basket if no items remain
      deleteBasketMutation.mutate("");
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
    setShowRemoveDialog(false);
  };

  const visibleFeedbacks = feedbacks;
  const totalCount = feedbacksData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / REVIEWS_PER_PAGE);

  const productJsonLd = book ? generateProductJsonLd(book, feedbacks) : null;
  const breadcrumbJsonLd = book
    ? generateBreadcrumbJsonLd([
        { name: "Home", url: "/" },
        { name: "Shop", url: "/shop" },
        { name: book.name || "Book", url: `/shop/${book.id}` },
      ])
    : null;

  return (
    <div className="min-h-screen flex flex-col bg-background">
      {productJsonLd && <JsonLd data={productJsonLd} />}
      {breadcrumbJsonLd && <JsonLd data={breadcrumbJsonLd} />}

      <Header />

      <main className="grow container mx-auto px-4 py-8" role="main">
        <Link
          href="/shop"
          className="inline-flex items-center text-sm text-muted-foreground hover:text-primary mb-8 transition-colors"
          rel="prev"
        >
          <ArrowLeft className="mr-2 size-4" aria-hidden="true" /> Back to Shop
        </Link>

        <ProductSection
          book={{
            imageUrl: book?.imageUrl,
            name: book?.name || "Book",
            priceSale: book?.priceSale,
            category: book?.category,
            authors: book?.authors || [],
            averageRating: book?.averageRating || 0,
            totalReviews: book?.totalReviews || 0,
            price: book?.price || 0,
            status: book?.status || "",
            description: book?.description,
            publisher: book?.publisher,
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
          className="max-w-4xl mx-auto"
          aria-label="Customer Reviews"
        >
          <ReviewsContainer
            isLoading={isLoading}
            reviews={visibleFeedbacks.map((f) => ({
              id: f.id,
              firstName: f.firstName || "",
              lastName: f.lastName || "",
              rating: f.rating,
              comment: f.comment || "",
            }))}
            averageRating={book?.averageRating || 0}
            totalReviews={book?.totalReviews || 0}
            sortBy={sortBy}
            onSortChange={setSortBy}
            isSummarizing={isSummarizing}
            onSummarize={handleSummarize}
            showReviewForm={showReviewForm}
            onToggleReviewForm={() => setShowReviewForm(!showReviewForm)}
            summary={summaryData?.summary}
            reviewForm={{
              firstName: newReview.firstName || "",
              lastName: newReview.lastName || "",
              comment: newReview.comment || "",
              rating: newReview.rating,
              isSubmitting: createFeedbackMutation.isPending,
              onChange: (field, value) =>
                setNewReview({ ...newReview, [field]: value }),
              onSubmit: handleSubmitReview,
            }}
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
        </section>
      </main>

      <RemoveItemDialog
        open={showRemoveDialog}
        onOpenChange={setShowRemoveDialog}
        items={book ? [{ id: book.id, name: book.name || "Book" }] : []}
        onConfirm={handleConfirmRemove}
        description={
          book ? (
            <>
              You're about to remove{" "}
              <span className="font-semibold text-foreground">{book.name}</span>{" "}
              from your basket. This action cannot be undone.
            </>
          ) : undefined
        }
      />

      <Footer />
    </div>
  );
}
