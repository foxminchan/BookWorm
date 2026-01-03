import type { Book } from "@workspace/types/catalog/books";
import type { Feedback } from "@workspace/types/rating/index";

import { APP_CONFIG } from "./constants";

export function generateProductJsonLd(book: Book, reviews?: Feedback[]) {
  const aggregateRating =
    reviews && reviews.length > 0
      ? {
          "@type": "AggregateRating",
          ratingValue: book.averageRating,
          reviewCount: book.totalReviews,
          bestRating: 5,
          worstRating: 1,
        }
      : undefined;

  const reviewsData = reviews?.map((review) => ({
    "@type": "Review",
    author: {
      "@type": "Person",
      name:
        `${review.firstName || ""} ${review.lastName || ""}`.trim() ||
        "Anonymous",
    },
    datePublished: new Date().toISOString(),
    reviewBody: review.comment || "",
    reviewRating: {
      "@type": "Rating",
      ratingValue: review.rating,
      bestRating: 5,
      worstRating: 1,
    },
  }));

  return {
    "@context": "https://schema.org",
    "@type": "Book",
    name: book.name,
    description: book.description,
    image: book.imageUrl ? book.imageUrl : undefined,
    author: book.authors.map((author) => ({
      "@type": "Person",
      name: author.name,
    })),
    publisher: {
      "@type": "Organization",
      name: book.publisher?.name,
    },
    offers: {
      "@type": "Offer",
      price: book.priceSale || book.price,
      priceCurrency: "USD",
      availability:
        book.status === "InStock"
          ? "https://schema.org/InStock"
          : "https://schema.org/OutOfStock",
      url: `/shop/${book.id}`,
      priceValidUntil: new Date(
        Date.now() + 30 * 24 * 60 * 60 * 1000,
      ).toISOString(),
    },
    aggregateRating,
    review: reviewsData,
  };
}

export function generateBreadcrumbJsonLd(
  items: Array<{ name: string; url: string }>,
) {
  return {
    "@context": "https://schema.org",
    "@type": "BreadcrumbList",
    itemListElement: items.map((item, index) => ({
      "@type": "ListItem",
      position: index + 1,
      name: item.name,
      item: item.url,
    })),
  };
}

export function generateOrganizationJsonLd() {
  return {
    "@context": "https://schema.org",
    "@type": "Organization",
    name: APP_CONFIG.name,
    logo: `/logo.svg`,
    description:
      "Curated online bookstore with literature, design, and inspiration books",
    sameAs: [APP_CONFIG.social.twitter, APP_CONFIG.social.facebook],
    contactPoint: {
      "@type": "ContactPoint",
      email: APP_CONFIG.email.support,
      contactType: "Customer Service",
    },
  };
}

export function generateWebsiteJsonLd() {
  return {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name: APP_CONFIG.name,
    potentialAction: {
      "@type": "SearchAction",
      target: {
        "@type": "EntryPoint",
        urlTemplate: `/shop?search={search_term_string}`,
      },
      "query-input": "required name=search_term_string",
    },
  };
}
