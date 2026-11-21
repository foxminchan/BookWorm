import { Suspense } from "react";

import type { Metadata } from "next";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import { bookQueries } from "@workspace/api-hooks/catalog/queries";
import { ratingKeys } from "@workspace/api-hooks/keys";
import { formatPrice } from "@workspace/utils/format";

import { ProductLoadingSkeleton } from "@/components/loading-skeleton";
import { APP_CONFIG, REVIEWS_PER_PAGE } from "@/config/app-config";
import { env } from "@/env.mjs";
import BookDetailPageClient from "@/features/catalog/product/book-detail-page-client";
import { getCachedBook, getCachedFeedbacks } from "@/lib/cached-catalog";
import { getQueryClient } from "@/lib/query-client";
import { generateImageObject } from "@/lib/seo";

type BookDetailPageProps = {
  params: Promise<{ id: string }>;
};

/**
 * Generates dynamic metadata for book detail pages.
 * Provides rich SEO with Open Graph and Twitter Card support.
 */
export async function generateMetadata({
  params,
}: BookDetailPageProps): Promise<Metadata> {
  const { id } = await params;

  // Try to fetch book data, but don't fail if it's not available
  let book;
  try {
    book = await getCachedBook(id);
  } catch {
    return {
      title: "Book Details",
      description: "View book details at BookWorm online bookstore.",
    };
  }

  const bookName = book.name ?? "Book";
  const authorNames = book.authors
    .map((a) => a.name)
    .filter((name): name is string => name != null);
  const socialTitle = `${bookName} | ${APP_CONFIG.name}`;

  const priceText = book.priceSale
    ? `On sale for ${formatPrice(book.priceSale)}`
    : formatPrice(book.price);
  const byLine = authorNames.length > 0 ? ` by ${authorNames.join(", ")}` : "";
  const description =
    book.description ?? `Buy ${bookName}${byLine} at BookWorm. ${priceText}.`;
  const url = `${env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultUrl}/shop/${id}`;

  const keywords = [
    bookName,
    ...authorNames,
    book.category?.name,
    book.publisher?.name,
    "buy book online",
    "bookstore",
  ].filter((k): k is string => Boolean(k));

  const authors =
    authorNames.length > 0 ? authorNames.map((name) => ({ name })) : undefined;

  // Generate rich image object with proper alt text
  const imageObject = generateImageObject(
    book.imageUrl,
    bookName,
    book.authors,
  );
  const images = imageObject ? [imageObject] : undefined;

  return {
    title: bookName,
    description,
    keywords,
    ...(authors && { authors }),
    openGraph: {
      type: "book",
      title: socialTitle,
      description,
      url,
      ...(images && { images }),
      siteName: APP_CONFIG.name,
      locale: "en_US",
    },
    twitter: {
      card: "summary_large_image",
      title: socialTitle,
      description,
      ...(images && { images }),
    },
    alternates: {
      canonical: url,
    },
  };
}

async function BookDetailContent({ params }: Readonly<BookDetailPageProps>) {
  const { id } = await params;
  const queryClient = getQueryClient();

  const reviewParams = {
    bookId: id,
    pageSize: REVIEWS_PER_PAGE,
    pageIndex: 1,
    orderBy: "createdAt" as const,
    isDescending: true,
  };

  // Prefetch book data and initial reviews in parallel
  try {
    await Promise.all([
      queryClient.query({
        queryKey: bookQueries.detail(id).queryKey,
        queryFn: () => getCachedBook(id),
      }),
      queryClient.query({
        queryKey: ratingKeys.feedbacks.byBook(id, reviewParams),
        queryFn: () => getCachedFeedbacks(reviewParams),
      }),
    ]);
  } catch (error) {
    console.error("Failed to prefetch book data:", error);
  }

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <BookDetailPageClient id={id} />
    </HydrationBoundary>
  );
}

export default function BookDetailPage({
  params,
}: Readonly<BookDetailPageProps>) {
  return (
    <Suspense fallback={<ProductLoadingSkeleton />}>
      <BookDetailContent params={params} />
    </Suspense>
  );
}
