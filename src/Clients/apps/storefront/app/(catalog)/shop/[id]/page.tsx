"use client";

import type React from "react";
import { useDebounceCallback } from "usehooks-ts";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { Separator } from "@workspace/ui/components/separator";
import {
  Star,
  ShoppingBasket,
  ArrowLeft,
  MessageSquare,
  User,
  Send,
  Sparkles,
  Loader2,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
} from "lucide-react";
import Link from "next/link";
import type { CreateFeedbackRequest } from "@workspace/types/rating";
import { useState, use } from "react";
import cn from "classnames";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@workspace/ui/components/dropdown-menu";
import { useRouter, notFound } from "next/navigation";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@workspace/ui/components/alert-dialog";
import { JsonLd } from "@/components/json-ld";
import { generateProductJsonLd, generateBreadcrumbJsonLd } from "@/lib/seo";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useFeedbacks from "@workspace/api-hooks/rating/useFeedbacks";
import useCreateFeedback from "@workspace/api-hooks/rating/useCreateFeedback";
import useSummaryFeedback from "@workspace/api-hooks/rating/useSummaryFeedback";
import useBasket from "@workspace/api-hooks/basket/useBasket";
import useCreateBasket from "@workspace/api-hooks/basket/useCreateBasket";
import useUpdateBasket from "@workspace/api-hooks/basket/useUpdateBasket";

const REVIEWS_PER_PAGE = 5;

interface BookDetailPageProps {
  params: Promise<{ id: string }>;
}

export default function BookDetailPage({ params }: BookDetailPageProps) {
  const router = useRouter();
  const { id } = use(params);

  // Local state
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
  const getSortParams = () => {
    switch (sortBy) {
      case "highest":
        return { orderBy: "rating", isDescending: true };
      case "lowest":
        return { orderBy: "rating", isDescending: false };
      case "newest":
      default:
        return { orderBy: "createdAt", isDescending: true };
    }
  };

  const sortParams = getSortParams();

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
  const { data: basket } = useBasket();
  const createBasketMutation = useCreateBasket();
  const updateBasketMutation = useUpdateBasket();
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
          <div className="space-y-8 animate-pulse">
            <div className="h-6 bg-secondary rounded w-32" />
            <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
              <div className="aspect-3/4 bg-secondary rounded-2xl" />
              <div className="space-y-6">
                <div className="h-8 bg-secondary rounded w-3/4" />
                <div className="h-6 bg-secondary rounded w-1/2" />
                <div className="h-24 bg-secondary rounded" />
              </div>
            </div>
          </div>
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
    createFeedbackMutation.mutate({
      ...newReview,
      bookId: id,
    });
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
      updateBasketMutation.mutate({
        request: {
          items: [{ id: id, quantity: 0 }],
        },
      });
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
    updateBasketMutation.mutate({
      request: {
        items: [{ id: id, quantity: 0 }],
      },
    });
    setShowRemoveDialog(false);
  };

  const handleQuantityIncrease = () => {
    const newQuantity = Math.min(99, quantity + 1);
    updateBasketMutation.mutate({
      request: {
        items: [{ id: id, quantity: newQuantity }],
      },
    });
  };

  // API handles pagination
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

        <article
          className="grid grid-cols-1 md:grid-cols-2 gap-12 lg:gap-20 mb-16"
          itemScope
          itemType="https://schema.org/Book"
        >
          {/* Book Image */}
          <figure className="aspect-3/4 bg-secondary rounded-2xl overflow-hidden shadow-sm relative group">
            {book?.priceSale && (
              <Badge className="absolute top-6 left-6 z-10 bg-destructive text-destructive-foreground font-bold px-3 py-1">
                SALE
              </Badge>
            )}
            <img
              src={book?.imageUrl || "/placeholder.svg"}
              alt={`${book?.name} book cover`}
              className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
              itemProp="image"
            />
          </figure>

          {/* Book Info */}
          <div className="flex flex-col">
            <div className="mb-6">
              <p className="text-sm font-bold uppercase tracking-widest text-primary mb-2">
                {book?.category?.name}
              </p>
              <h1
                className="text-4xl md:text-5xl font-serif font-medium leading-tight mb-4"
                itemProp="name"
              >
                {book?.name}
              </h1>
              <p className="text-xl text-muted-foreground">
                by{" "}
                <span itemProp="author">
                  {book?.authors.map((a) => a.name).join(", ")}
                </span>
              </p>
            </div>

            <div className="flex items-center gap-4 mb-8">
              <div className="flex items-center gap-1">
                {[...Array(5)].map((_, i) => (
                  <Star
                    key={i}
                    className={cn(
                      "size-5",
                      i < Math.floor(book?.averageRating || 0)
                        ? "fill-primary text-primary"
                        : "text-muted-foreground/30",
                    )}
                    aria-hidden="true"
                  />
                ))}
                <span
                  className="ml-2 font-medium"
                  itemProp="aggregateRating"
                  itemScope
                  itemType="https://schema.org/AggregateRating"
                >
                  <meta
                    itemProp="ratingValue"
                    content={String(book?.averageRating || 0)}
                  />
                  <meta
                    itemProp="reviewCount"
                    content={String(book?.totalReviews || 0)}
                  />
                  {book?.averageRating?.toFixed(1) ?? "0.0"}
                </span>
              </div>
              <Separator
                orientation="vertical"
                className="h-4"
                aria-hidden="true"
              />
              <span className="text-sm text-muted-foreground">
                {book?.totalReviews} Reviews
              </span>
            </div>

            <div className="mb-8">
              <div
                className="flex items-baseline gap-3 mb-2"
                itemProp="offers"
                itemScope
                itemType="https://schema.org/Offer"
              >
                {book?.priceSale ? (
                  <>
                    <span
                      className="text-3xl font-bold text-primary"
                      itemProp="price"
                    >
                      ${book.priceSale.toFixed(2)}
                    </span>
                    <span className="text-xl text-muted-foreground line-through">
                      ${book.price.toFixed(2)}
                    </span>
                  </>
                ) : (
                  <span className="text-3xl font-bold" itemProp="price">
                    ${book?.price.toFixed(2)}
                  </span>
                )}
                <meta itemProp="priceCurrency" content="USD" />
                <meta
                  itemProp="availability"
                  content={
                    book?.status === "InStock" ? "InStock" : "OutOfStock"
                  }
                />
              </div>
              <p
                className={cn(
                  "text-sm font-medium",
                  book?.status === "InStock"
                    ? "text-emerald-600"
                    : "text-destructive",
                )}
              >
                {book?.status === "InStock"
                  ? "Available in Store"
                  : "Out of Stock"}
              </p>
            </div>

            <div className="space-y-6 mb-10">
              <p
                className="text-muted-foreground leading-relaxed text-lg"
                itemProp="description"
              >
                {book?.description}
              </p>

              <div className="grid grid-cols-2 gap-y-4 text-sm">
                <div>
                  <p className="font-bold text-muted-foreground uppercase tracking-wider text-[10px]">
                    Publisher
                  </p>
                  <p className="font-medium" itemProp="publisher">
                    {book?.publisher?.name}
                  </p>
                </div>
                <div>
                  <p className="font-bold text-muted-foreground uppercase tracking-wider text-[10px]">
                    Category
                  </p>
                  <p className="font-medium" itemProp="bookEdition">
                    {book?.category?.name}
                  </p>
                </div>
              </div>
            </div>

            <div className="flex flex-col sm:flex-row gap-4">
              {quantity === 0 ? (
                <Button
                  size="lg"
                  className="rounded-full h-14 text-lg gap-3 w-full sm:w-auto px-10 shadow-lg shadow-primary/20"
                  onClick={handleAddToBasket}
                  disabled={
                    book?.status !== "InStock" ||
                    createBasketMutation.isPending ||
                    updateBasketMutation.isPending
                  }
                >
                  {createBasketMutation.isPending ||
                  updateBasketMutation.isPending ? (
                    <>
                      <Loader2 className="size-5 animate-spin" />
                      Adding...
                    </>
                  ) : (
                    <>
                      <ShoppingBasket className="size-5" /> Add to Basket
                    </>
                  )}
                </Button>
              ) : (
                <div className="flex items-center bg-secondary/50 rounded-full h-12 p-1 w-fit shadow-inner">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="rounded-full size-10 hover:bg-background shadow-sm transition-all"
                    onClick={handleQuantityDecreaseDebounced}
                    aria-label="Decrease quantity"
                  >
                    <span className="text-lg font-medium">-</span>
                  </Button>
                  <input
                    type="text"
                    inputMode="numeric"
                    min="1"
                    max="99"
                    value={quantity}
                    onChange={handleQuantityChange}
                    className="w-12 bg-transparent text-center text-lg font-serif font-bold focus:outline-none border-none [&::-webkit-outer-spin-button]:hidden [&::-webkit-inner-spin-button]:hidden"
                    aria-label="Book quantity"
                  />
                  <Button
                    variant="ghost"
                    size="icon"
                    className="rounded-full size-10 hover:bg-background shadow-sm transition-all"
                    onClick={handleQuantityIncreaseDebounced}
                    aria-label="Increase quantity"
                  >
                    <span className="text-lg font-medium">+</span>
                  </Button>
                </div>
              )}
            </div>
          </div>
        </article>

        <Separator className="my-16" aria-hidden="true" />

        {/* Reviews Section */}
        <section
          id="reviews"
          className="max-w-4xl mx-auto"
          aria-label="Customer Reviews"
        >
          {isLoading ? (
            // Loading skeleton for reviews section
            <div className="space-y-6">
              <div className="h-10 bg-secondary rounded-lg animate-pulse w-1/3" />
              <div className="h-6 bg-secondary rounded-lg animate-pulse w-1/2" />
              <div className="space-y-8">
                {[...Array(3)].map((_, i) => (
                  <div key={i} className="space-y-4">
                    <div className="flex gap-3">
                      <div className="size-10 rounded-full bg-secondary animate-pulse shrink-0" />
                      <div className="flex-1 space-y-2">
                        <div className="h-4 bg-secondary rounded animate-pulse w-1/4" />
                        <div className="flex gap-1">
                          {[...Array(5)].map((_, j) => (
                            <div
                              key={j}
                              className="size-3 bg-secondary rounded animate-pulse"
                            />
                          ))}
                        </div>
                      </div>
                    </div>
                    <div className="h-16 bg-secondary rounded animate-pulse" />
                  </div>
                ))}
              </div>
            </div>
          ) : visibleFeedbacks.length === 0 ? (
            // Empty state when no reviews exist
            <div className="text-center py-16 space-y-4">
              <MessageSquare className="size-12 text-muted-foreground/50 mx-auto" />
              <div>
                <h3 className="text-lg font-medium mb-2">No reviews yet</h3>
                <p className="text-muted-foreground mb-6">
                  Be the first to share your thoughts about this book
                </p>
                <Button
                  className="rounded-full"
                  onClick={() => setShowReviewForm(true)}
                >
                  <MessageSquare className="size-4 mr-2" /> Write First Review
                </Button>
              </div>
            </div>
          ) : (
            <>
              <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-12">
                <header>
                  <h2 className="text-3xl font-serif font-medium mb-2">
                    Customer Feedback
                  </h2>
                  <p className="text-muted-foreground">
                    What our readers are saying about this book
                  </p>
                </header>
                <div className="flex flex-wrap gap-3">
                  {/* Sorting Dropdown */}
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button
                        variant="outline"
                        className="rounded-full gap-2 px-6 bg-transparent"
                        aria-label="Sort reviews by"
                      >
                        Sort by:{" "}
                        {sortBy.charAt(0).toUpperCase() + sortBy.slice(1)}{" "}
                        <ChevronDown className="size-4" aria-hidden="true" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="rounded-xl">
                      <DropdownMenuItem onClick={() => setSortBy("newest")}>
                        Newest
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => setSortBy("highest")}>
                        Highest Rating
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => setSortBy("lowest")}>
                        Lowest Rating
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>

                  <Button
                    variant="outline"
                    className="rounded-full gap-2 px-6 border-primary/20 text-primary hover:bg-primary/5 transition-all bg-transparent"
                    onClick={handleSummarize}
                    disabled={isSummarizing}
                    aria-label="Summarize reviews"
                  >
                    {isSummarizing ? (
                      <Loader2
                        className="size-4 animate-spin"
                        aria-hidden="true"
                      />
                    ) : (
                      <Sparkles
                        className="size-4 text-primary"
                        aria-hidden="true"
                      />
                    )}
                    {isSummarizing ? "Summarizing..." : "Summarize Reviews"}
                  </Button>
                  <Button
                    variant={showReviewForm ? "ghost" : "outline"}
                    className="rounded-full gap-2 px-6 bg-transparent"
                    onClick={() => setShowReviewForm(!showReviewForm)}
                    aria-label={
                      showReviewForm
                        ? "Cancel writing a review"
                        : "Write a review"
                    }
                  >
                    <MessageSquare className="size-4" aria-hidden="true" />{" "}
                    {showReviewForm ? "Cancel Review" : "Write a Review"}
                  </Button>
                </div>
              </div>

              {summaryData?.summary && (
                <aside className="mb-12 bg-primary/5 border border-primary/10 rounded-2xl p-8 animate-in fade-in slide-in-from-top-4 duration-500">
                  <div className="flex items-center gap-2 mb-4 text-primary">
                    <Sparkles className="size-5" aria-hidden="true" />
                    <h3 className="font-serif font-bold tracking-tight">
                      AI Review Summary
                    </h3>
                  </div>
                  <p className="text-lg leading-relaxed text-foreground/80 italic font-serif">
                    "{summaryData.summary}"
                  </p>
                </aside>
              )}

              <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">
                {/* Rating Summary */}
                <div className="lg:col-span-1 space-y-8">
                  <div className="bg-secondary/30 p-8 rounded-2xl text-center">
                    <p className="text-5xl font-serif font-bold mb-2">
                      {book?.averageRating?.toFixed(1) ?? "0.0"}
                    </p>
                    <div className="flex justify-center gap-1 mb-2">
                      {[...Array(5)].map((_, i) => (
                        <Star
                          key={i}
                          className={cn(
                            "size-4",
                            i < Math.floor(book?.averageRating || 0)
                              ? "fill-primary text-primary"
                              : "text-muted-foreground/30",
                          )}
                        />
                      ))}
                    </div>
                    <p className="text-sm text-muted-foreground">
                      Based on {book?.totalReviews} reviews
                    </p>
                  </div>

                  {showReviewForm && (
                    <div className="bg-background border border-border p-6 rounded-2xl shadow-sm space-y-4">
                      <h3 className="font-serif font-medium text-lg">
                        Your Review
                      </h3>
                      <div className="space-y-3">
                        <div className="flex gap-1">
                          {[1, 2, 3, 4, 5].map((star) => (
                            <button
                              key={star}
                              onClick={() =>
                                setNewReview({ ...newReview, rating: star })
                              }
                              className="focus:outline-none transition-transform active:scale-90"
                            >
                              <Star
                                className={cn(
                                  "size-6",
                                  star <= newReview.rating
                                    ? "fill-primary text-primary"
                                    : "text-muted-foreground/30",
                                )}
                              />
                            </button>
                          ))}
                        </div>
                        <div className="grid grid-cols-2 gap-3">
                          <input
                            type="text"
                            placeholder="First Name"
                            className="bg-secondary/50 border-none rounded-lg px-4 py-2 text-sm focus:ring-1 focus:ring-primary outline-none"
                            value={newReview.firstName || ""}
                            onChange={(e) =>
                              setNewReview({
                                ...newReview,
                                firstName: e.target.value,
                              })
                            }
                          />
                          <input
                            type="text"
                            placeholder="Last Name"
                            className="bg-secondary/50 border-none rounded-lg px-4 py-2 text-sm focus:ring-1 focus:ring-primary outline-none"
                            value={newReview.lastName || ""}
                            onChange={(e) =>
                              setNewReview({
                                ...newReview,
                                lastName: e.target.value,
                              })
                            }
                          />
                        </div>
                        <textarea
                          placeholder="Share your thoughts..."
                          rows={4}
                          className="w-full bg-secondary/50 border-none rounded-lg px-4 py-3 text-sm focus:ring-1 focus:ring-primary outline-none resize-none"
                          value={newReview.comment || ""}
                          onChange={(e) =>
                            setNewReview({
                              ...newReview,
                              comment: e.target.value,
                            })
                          }
                        />
                        <Button
                          className="w-full rounded-full gap-2"
                          onClick={handleSubmitReview}
                          disabled={
                            createFeedbackMutation.isPending ||
                            newReview.rating === 0
                          }
                        >
                          {createFeedbackMutation.isPending ? (
                            <>
                              <Loader2 className="size-4 animate-spin" />
                              Submitting...
                            </>
                          ) : (
                            <>
                              <Send className="size-4" /> Submit Review
                            </>
                          )}
                        </Button>
                      </div>
                    </div>
                  )}
                </div>

                {/* Review List */}
                <div className="lg:col-span-2 space-y-8">
                  {visibleFeedbacks.map((feedback) => (
                    <div key={feedback.id} className="group">
                      <div className="flex items-center justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="size-10 rounded-full bg-secondary flex items-center justify-center">
                            <User className="size-5 text-muted-foreground" />
                          </div>
                          <div>
                            <p className="font-medium">
                              {feedback.firstName} {feedback.lastName}
                            </p>
                            <div className="flex gap-0.5">
                              {[...Array(5)].map((_, i) => (
                                <Star
                                  key={i}
                                  className={cn(
                                    "size-3",
                                    i < feedback.rating
                                      ? "fill-primary text-primary"
                                      : "text-muted-foreground/30",
                                  )}
                                />
                              ))}
                            </div>
                          </div>
                        </div>
                      </div>
                      <p className="text-muted-foreground leading-relaxed pl-13">
                        {feedback.comment}
                      </p>
                      <Separator className="mt-8 group-last:hidden" />
                    </div>
                  ))}

                  {totalPages > 1 && (
                    <div className="pt-8 flex items-center justify-center gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        className="rounded-full"
                        onClick={() =>
                          setCurrentPage((p) => Math.max(1, p - 1))
                        }
                        disabled={currentPage === 1}
                      >
                        <ChevronLeft className="size-4" />
                      </Button>

                      {[...Array(totalPages)].map((_, i) => (
                        <Button
                          key={i}
                          variant={currentPage === i + 1 ? "default" : "ghost"}
                          size="sm"
                          className={cn(
                            "rounded-full min-w-8",
                            currentPage === i + 1 && "shadow-md",
                          )}
                          onClick={() => setCurrentPage(i + 1)}
                        >
                          {i + 1}
                        </Button>
                      ))}

                      <Button
                        variant="ghost"
                        size="icon"
                        className="rounded-full"
                        onClick={() =>
                          setCurrentPage((p) => Math.min(totalPages, p + 1))
                        }
                        disabled={currentPage === totalPages}
                      >
                        <ChevronRight className="size-4" />
                      </Button>
                    </div>
                  )}
                </div>
              </div>
            </>
          )}
        </section>
      </main>

      <AlertDialog open={showRemoveDialog} onOpenChange={setShowRemoveDialog}>
        <AlertDialogContent className="rounded-2xl">
          <AlertDialogHeader>
            <AlertDialogTitle className="text-2xl">
              Remove from Basket?
            </AlertDialogTitle>
            <AlertDialogDescription className="text-base pt-2">
              You're about to remove{" "}
              <span className="font-semibold text-foreground">
                {book?.name}
              </span>{" "}
              from your basket. This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="flex gap-3 justify-end">
            <AlertDialogCancel className="rounded-full px-6">
              Keep Item
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmRemove}
              className="rounded-full px-6 bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Remove
            </AlertDialogAction>
          </div>
        </AlertDialogContent>
      </AlertDialog>

      <Footer />
    </div>
  );
}
