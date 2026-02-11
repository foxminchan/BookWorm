import { describe, expect, it, vi } from "vitest";

import type { Book } from "@workspace/types/catalog/books";
import type { Feedback } from "@workspace/types/rating";

import {
  buildQueryString,
  generateBreadcrumbJsonLd,
  generateCollectionPageJsonLd,
  generateFAQJsonLd,
  generateImageObject,
  generateItemListJsonLd,
  generateOrganizationJsonLd,
  generateProductJsonLd,
  generateWebsiteJsonLd,
  getCanonicalUrl,
  truncateDescription,
} from "@/lib/seo";

vi.mock("@/env.mjs", () => ({
  env: {
    NEXT_PUBLIC_APP_URL: "https://bookworm.test",
  },
}));

const mockBook: Book = {
  id: "book-1",
  name: "Clean Code",
  description: "A handbook of agile software craftsmanship",
  imageUrl: "https://example.com/clean-code.jpg",
  price: 39.99,
  priceSale: 29.99,
  status: "InStock",
  category: { id: "cat-1", name: "Programming" },
  publisher: { id: "pub-1", name: "Prentice Hall" },
  authors: [
    { id: "auth-1", name: "Robert C. Martin" },
    { id: "auth-2", name: "Michael Feathers" },
  ],
  averageRating: 4.5,
  totalReviews: 120,
};

const mockReviews: Feedback[] = [
  {
    id: "r-1",
    bookId: "book-1",
    firstName: "John",
    lastName: "Doe",
    rating: 5,
    comment: "Excellent book!",
  },
  {
    id: "r-2",
    bookId: "book-1",
    firstName: "Jane",
    lastName: "",
    rating: 4,
    comment: "Very helpful",
  },
];

describe("seo utils", () => {
  describe("generateProductJsonLd", () => {
    it("should generate basic product JSON-LD without reviews", () => {
      const result = generateProductJsonLd(mockBook);

      expect(result["@context"]).toBe("https://schema.org");
      expect(result["@type"]).toBe("Book");
      expect(result.name).toBe("Clean Code");
      expect(result.description).toBe(
        "A handbook of agile software craftsmanship",
      );
      expect(result.aggregateRating).toBeUndefined();
      expect(result.review).toBeUndefined();
    });

    it("should include aggregate rating when reviews exist", () => {
      const result = generateProductJsonLd(mockBook, mockReviews);

      expect(result.aggregateRating).toBeDefined();
      expect(result.aggregateRating?.["@type"]).toBe("AggregateRating");
      expect(result.aggregateRating?.ratingValue).toBe(4.5);
      expect(result.aggregateRating?.reviewCount).toBe(120);
    });

    it("should include author data", () => {
      const result = generateProductJsonLd(mockBook);

      expect(result.author).toHaveLength(2);
      expect(result.author[0]).toEqual({
        "@type": "Person",
        name: "Robert C. Martin",
      });
    });

    it("should include offer data with sale price", () => {
      const result = generateProductJsonLd(mockBook);

      expect(result.offers["@type"]).toBe("Offer");
      expect(result.offers.price).toBe(29.99);
      expect(result.offers.priceCurrency).toBe("USD");
      expect(result.offers.availability).toBe("https://schema.org/InStock");
    });

    it("should use full price when no sale price exists", () => {
      const book: Book = { ...mockBook, priceSale: null };
      const result = generateProductJsonLd(book);

      expect(result.offers.price).toBe(39.99);
    });

    it("should set OutOfStock availability when book is not in stock", () => {
      const book: Book = { ...mockBook, status: "OutOfStock" };
      const result = generateProductJsonLd(book);

      expect(result.offers.availability).toBe("https://schema.org/OutOfStock");
    });

    it("should include image object when imageUrl is provided", () => {
      const result = generateProductJsonLd(mockBook);

      expect(result.image).toEqual({
        "@type": "ImageObject",
        url: "https://example.com/clean-code.jpg",
        contentUrl: "https://example.com/clean-code.jpg",
      });
    });

    it("should omit image when imageUrl is null", () => {
      const book: Book = { ...mockBook, imageUrl: null };
      const result = generateProductJsonLd(book);

      expect(result.image).toBeUndefined();
    });

    it("should include review data when reviews exist", () => {
      const result = generateProductJsonLd(mockBook, mockReviews);

      expect(result.review).toHaveLength(2);
      expect(result.review![0]!["@type"]).toBe("Review");
      expect(result.review![0]!.reviewBody).toBe("Excellent book!");
      expect(result.review![0]!.author.name).toBe("John Doe");
    });

    it("should use Anonymous when review author name is empty", () => {
      const reviews: Feedback[] = [
        {
          id: "r-3",
          bookId: "book-1",
          firstName: "",
          lastName: "",
          rating: 3,
          comment: "Okay",
        },
      ];
      const result = generateProductJsonLd(mockBook, reviews);

      expect(result.review![0]!.author.name).toBe("Anonymous");
    });
  });

  describe("generateBreadcrumbJsonLd", () => {
    it("should generate breadcrumb list", () => {
      const items = [
        { name: "Home", url: "/" },
        { name: "Shop", url: "/shop" },
        { name: "Clean Code", url: "/shop/book-1" },
      ];

      const result = generateBreadcrumbJsonLd(items);

      expect(result["@type"]).toBe("BreadcrumbList");
      expect(result.itemListElement).toHaveLength(3);
      expect(result.itemListElement[0]!.position).toBe(1);
      expect(result.itemListElement[2]!.name).toBe("Clean Code");
    });

    it("should handle empty breadcrumbs", () => {
      const result = generateBreadcrumbJsonLd([]);

      expect(result.itemListElement).toHaveLength(0);
    });
  });

  describe("generateOrganizationJsonLd", () => {
    it("should generate organization data", () => {
      const result = generateOrganizationJsonLd();

      expect(result["@type"]).toBe("Organization");
      expect(result.name).toBe("BookWorm");
      expect(result.sameAs).toHaveLength(2);
      expect(result.contactPoint.contactType).toBe("Customer Service");
    });
  });

  describe("generateWebsiteJsonLd", () => {
    it("should generate website data with search action", () => {
      const result = generateWebsiteJsonLd();

      expect(result["@type"]).toBe("WebSite");
      expect(result.name).toBe("BookWorm");
      expect(result.url).toBe("https://bookworm.test");
      expect(result.potentialAction["@type"]).toBe("SearchAction");
    });
  });

  describe("generateItemListJsonLd", () => {
    it("should generate item list for books", () => {
      const books = [
        mockBook,
        { ...mockBook, id: "book-2", name: "Refactoring" },
      ];
      const result = generateItemListJsonLd(books, "Featured Books");

      expect(result["@type"]).toBe("ItemList");
      expect(result.name).toBe("Featured Books");
      expect(result.numberOfItems).toBe(2);
      expect(result.itemListElement[0]!.position).toBe(1);
      expect(result.itemListElement[1]!.item.name).toBe("Refactoring");
    });

    it("should handle empty book list", () => {
      const result = generateItemListJsonLd([], "Empty List");

      expect(result.numberOfItems).toBe(0);
      expect(result.itemListElement).toHaveLength(0);
    });
  });

  describe("generateFAQJsonLd", () => {
    it("should generate FAQ structured data", () => {
      const faqs = [
        { question: "What is BookWorm?", answer: "An online bookstore." },
        { question: "How to order?", answer: "Browse and add to cart." },
      ];

      const result = generateFAQJsonLd(faqs);

      expect(result["@type"]).toBe("FAQPage");
      expect(result.mainEntity).toHaveLength(2);
      expect(result.mainEntity[0]!["@type"]).toBe("Question");
      expect(result.mainEntity[0]!.acceptedAnswer.text).toBe(
        "An online bookstore.",
      );
    });
  });

  describe("generateCollectionPageJsonLd", () => {
    it("should generate collection page data", () => {
      const result = generateCollectionPageJsonLd(
        "Fiction",
        "Browse fiction books",
        42,
      );

      expect(result["@type"]).toBe("CollectionPage");
      expect(result.name).toBe("Fiction Books");
      expect(result.description).toBe("Browse fiction books");
      expect(result.numberOfItems).toBe(42);
    });
  });

  describe("truncateDescription", () => {
    it("should return text as-is when under limit", () => {
      expect(truncateDescription("Short text")).toBe("Short text");
    });

    it("should truncate long text with ellipsis", () => {
      const longText = "A".repeat(200);
      const result = truncateDescription(longText);

      expect(result.length).toBeLessThanOrEqual(160);
      expect(result).toMatch(/\.\.\.$/);
    });

    it("should respect custom maxLength", () => {
      const result = truncateDescription("Hello World, this is long", 10);

      expect(result.length).toBeLessThanOrEqual(10);
      expect(result).toMatch(/\.\.\.$/);
    });
  });

  describe("getCanonicalUrl", () => {
    it("should generate canonical URL with base", () => {
      expect(getCanonicalUrl("/shop")).toBe("https://bookworm.test/shop");
    });

    it("should handle root path", () => {
      expect(getCanonicalUrl("/")).toBe("https://bookworm.test/");
    });
  });

  describe("generateImageObject", () => {
    it("should generate image object with author names", () => {
      const result = generateImageObject(
        "https://example.com/img.jpg",
        "Clean Code",
        [{ name: "Robert Martin" }],
      );

      expect(result).toEqual({
        url: "https://example.com/img.jpg",
        alt: "Clean Code by Robert Martin book cover",
        width: 800,
        height: 1200,
      });
    });

    it("should generate image without authors", () => {
      const result = generateImageObject(
        "https://example.com/img.jpg",
        "My Book",
      );

      expect(result?.alt).toBe("My Book book cover");
    });

    it("should return undefined when imageUrl is null", () => {
      expect(generateImageObject(null, "No Image")).toBeUndefined();
    });

    it("should return undefined when imageUrl is undefined", () => {
      expect(generateImageObject(undefined, "No Image")).toBeUndefined();
    });

    it("should filter out null author names", () => {
      const result = generateImageObject(
        "https://example.com/img.jpg",
        "Book",
        [{ name: "Author" }, { name: null }],
      );

      expect(result?.alt).toBe("Book by Author book cover");
    });
  });

  describe("buildQueryString", () => {
    it("should build query string from params", () => {
      const result = buildQueryString({
        search: "react",
        category: "programming",
      });

      expect(result).toContain("search=react");
      expect(result).toContain("category=programming");
    });

    it("should exclude empty values", () => {
      const result = buildQueryString({
        search: "react",
        category: undefined,
      });

      expect(result).toBe("search=react");
    });

    it("should exclude page=1", () => {
      const result = buildQueryString({ search: "react", page: "1" });

      expect(result).toBe("search=react");
    });

    it("should include page when not 1", () => {
      const result = buildQueryString({ page: "3" });

      expect(result).toBe("page=3");
    });

    it("should return empty string for all-empty params", () => {
      const result = buildQueryString({ search: undefined, page: "1" });

      expect(result).toBe("");
    });
  });
});
