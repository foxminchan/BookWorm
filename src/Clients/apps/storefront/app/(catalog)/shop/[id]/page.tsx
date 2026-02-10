import type { Metadata } from "next";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";
import feedbacksApiClient from "@workspace/api-client/rating/feedbacks";
import { catalogKeys, ratingKeys } from "@workspace/api-hooks/keys";

import { env } from "@/env.mjs";
import BookDetailPageClient from "@/features/catalog/product/book-detail-page-client";
import { currencyFormatter } from "@/lib/constants";
import { getQueryClient } from "@/lib/query-client";
import { generateImageObject } from "@/lib/seo";

const REVIEWS_PER_PAGE = 5;

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
    book = await booksApiClient.get(id);
  } catch {
    return {
      title: "Book Details | BookWorm",
      description: "View book details at BookWorm online bookstore.",
    };
  }

  const bookName = book.name ?? "Book";
  const authorNames = book.authors
    .map((a) => a.name)
    .filter((name): name is string => name != null);
  const title = `${bookName} | BookWorm`;

  const priceText = book.priceSale
    ? `On sale for ${currencyFormatter.format(book.priceSale)}`
    : currencyFormatter.format(book.price);
  const byLine = authorNames.length > 0 ? ` by ${authorNames.join(", ")}` : "";
  const description =
    book.description ?? `Buy ${bookName}${byLine} at BookWorm. ${priceText}.`;
  const url = `${env.NEXT_PUBLIC_APP_URL || "https://bookworm.com"}/shop/${id}`;

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
    title,
    description,
    keywords,
    ...(authors && { authors }),
    openGraph: {
      type: "book",
      title,
      description,
      url,
      ...(images && { images }),
      siteName: "BookWorm",
      locale: "en_US",
    },
    twitter: {
      card: "summary_large_image",
      title,
      description,
      ...(images && { images }),
    },
    alternates: {
      canonical: url,
    },
  };
}

export default async function BookDetailPage({
  params,
}: Readonly<BookDetailPageProps>) {
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
      queryClient.prefetchQuery({
        queryKey: catalogKeys.books.detail(id),
        queryFn: () => booksApiClient.get(id),
      }),
      queryClient.prefetchQuery({
        queryKey: ratingKeys.feedbacks.byBook(id, reviewParams),
        queryFn: () => feedbacksApiClient.list(reviewParams),
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
