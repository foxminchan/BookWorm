import { addDays, formatISO } from "date-fns";

import type { Book } from "@workspace/types/catalog/books";
import type { Feedback } from "@workspace/types/rating";

export type OrganizationJsonLdOptions = {
  name: string;
  description?: string;
  logo?: string;
  sameAs?: string[];
  supportEmail?: string;
};

export type WebsiteJsonLdOptions = {
  name: string;
  baseUrl: string;
};

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
    datePublished: formatISO(new Date()),
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
    image: book.imageUrl
      ? {
          "@type": "ImageObject",
          url: book.imageUrl,
          contentUrl: book.imageUrl,
        }
      : undefined,
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
      priceValidUntil: formatISO(addDays(new Date(), 30)),
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

export function generateOrganizationJsonLd(
  options: OrganizationJsonLdOptions & { supportEmail: string },
): {
  "@context": string;
  "@type": string;
  name: string;
  logo: string;
  description: string;
  sameAs: string[];
  contactPoint: {
    "@type": string;
    email: string;
    contactType: string;
  };
};
export function generateOrganizationJsonLd(
  options: OrganizationJsonLdOptions,
): {
  "@context": string;
  "@type": string;
  name: string;
  logo: string;
  description: string;
  sameAs: string[];
  contactPoint?: {
    "@type": string;
    email: string;
    contactType: string;
  };
};
export function generateOrganizationJsonLd({
  name,
  description = "Curated online bookstore with literature, design, and inspiration books",
  logo = "/logo.svg",
  sameAs = [],
  supportEmail,
}: OrganizationJsonLdOptions) {
  return {
    "@context": "https://schema.org",
    "@type": "Organization",
    name,
    logo,
    description,
    sameAs,
    ...(supportEmail
      ? {
          contactPoint: {
            "@type": "ContactPoint",
            email: supportEmail,
            contactType: "Customer Service",
          },
        }
      : {}),
  };
}

export function generateWebsiteJsonLd({ name, baseUrl }: WebsiteJsonLdOptions) {
  return {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name,
    url: baseUrl,
    potentialAction: {
      "@type": "SearchAction",
      target: {
        "@type": "EntryPoint",
        urlTemplate: `${baseUrl}/shop?search={search_term_string}`,
      },
      "query-input": "required name=search_term_string",
    },
  };
}

/**
 * Generates ItemList structured data for book listings.
 */
export function generateItemListJsonLd(
  books: Book[],
  listName: string,
  baseUrl = "https://bookworm.com",
) {
  return {
    "@context": "https://schema.org",
    "@type": "ItemList",
    name: listName,
    numberOfItems: books.length,
    itemListElement: books.map((book, index) => ({
      "@type": "ListItem",
      position: index + 1,
      item: {
        "@type": "Book",
        "@id": `${baseUrl}/shop/${book.id}`,
        name: book.name,
        image: book.imageUrl,
        author: book.authors.map((author) => ({
          "@type": "Person",
          name: author.name,
        })),
        offers: {
          "@type": "Offer",
          price: book.priceSale || book.price,
          priceCurrency: "USD",
          availability:
            book.status === "InStock"
              ? "https://schema.org/InStock"
              : "https://schema.org/OutOfStock",
        },
      },
    })),
  };
}

/**
 * Generates FAQ structured data.
 */
export function generateFAQJsonLd(
  faqs: Array<{ question: string; answer: string }>,
) {
  return {
    "@context": "https://schema.org",
    "@type": "FAQPage",
    mainEntity: faqs.map((faq) => ({
      "@type": "Question",
      name: faq.question,
      acceptedAnswer: {
        "@type": "Answer",
        text: faq.answer,
      },
    })),
  };
}

/**
 * Generates CollectionPage structured data for category pages.
 */
export function generateCollectionPageJsonLd(
  categoryName: string,
  description: string,
  bookCount: number,
) {
  return {
    "@context": "https://schema.org",
    "@type": "CollectionPage",
    name: `${categoryName} Books`,
    description,
    about: {
      "@type": "Thing",
      name: categoryName,
    },
    numberOfItems: bookCount,
  };
}

/**
 * Truncates text for meta descriptions.
 */
export function truncateDescription(text: string, maxLength = 160): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength - 3).trim() + "...";
}

/**
 * Generates canonical URL for the current path.
 */
export function getCanonicalUrl(
  path: string,
  baseUrl = "https://bookworm.com",
): string {
  return `${baseUrl}${path}`;
}

/**
 * Generates rich image object with proper alt text for accessibility and SEO.
 */
export function generateImageObject(
  imageUrl: string | null | undefined,
  itemName: string,
  authors?: Array<{ name: string | null }>,
): { url: string; alt: string; width: number; height: number } | undefined {
  if (!imageUrl) return undefined;

  const authorNames = authors
    ?.map((a) => a.name)
    .filter((name): name is string => Boolean(name));

  const alt =
    authorNames && authorNames.length > 0
      ? `${itemName} by ${authorNames.join(", ")} book cover`
      : `${itemName} book cover`;

  return {
    url: imageUrl,
    alt,
    width: 800,
    height: 1200,
  };
}

/**
 * Builds query string from search params object, excluding empty values.
 */
export function buildQueryString(
  params: Record<string, string | undefined>,
): string {
  const searchParams = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value && value !== "1" && key !== "page") {
      searchParams.set(key, value);
    }
    if (key === "page" && value && value !== "1") {
      searchParams.set(key, value);
    }
  });
  return searchParams.toString();
}
