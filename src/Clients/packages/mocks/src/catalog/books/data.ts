import { v7 as uuidv7 } from "uuid";
import type { Author } from "@workspace/types/catalog/authors";
import type { Book } from "@workspace/types/catalog/books";
import booksData from "../../data/books.json";
import { mockAuthors } from "../../catalog/authors/data";
import { mockCategories } from "../../catalog/categories/data";
import { mockPublishers } from "../../catalog/publishers/data";

export const mockBooks: Book[] = booksData.map((book) => {
  const { categoryId, publisherId, authorIds, ...bookData } = book;

  const category = mockCategories.find((c) => c.id === categoryId) ?? null;
  const publisher = mockPublishers.find((p) => p.id === publisherId) ?? null;
  const authors = authorIds
    .map((authorId) => mockAuthors.find((a) => a.id === authorId))
    .filter((author): author is Author => author !== undefined);

  return {
    ...bookData,
    status: bookData.status as "InStock" | "OutOfStock",
    category,
    publisher,
    authors,
    averageRating: Math.random() * 5,
    totalReviews: Math.floor(Math.random() * 2000),
  };
});

export const booksStore = {
  books: [...mockBooks],

  list(query?: {
    pageIndex?: number;
    pageSize?: number;
    searchTerm?: string;
  }): {
    items: Book[];
    pageIndex: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
  } {
    let filtered = [...this.books];

    if (query?.searchTerm) {
      const term = query.searchTerm.toLowerCase();
      filtered = filtered.filter(
        (book) =>
          book.name?.toLowerCase().includes(term) ||
          book.description?.toLowerCase().includes(term),
      );
    }

    const pageIndex = query?.pageIndex ?? 1;
    const pageSize = query?.pageSize ?? 10;
    const totalCount = filtered.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageIndex - 1) * pageSize;
    const items = filtered.slice(startIndex, startIndex + pageSize);

    return {
      items,
      pageIndex,
      pageSize,
      totalCount,
      totalPages,
      hasPreviousPage: pageIndex > 1,
      hasNextPage: pageIndex < totalPages,
    };
  },

  get(id: string): Book | undefined {
    return this.books.find((book) => book.id === id);
  },

  create(data: {
    id?: string;
    name: string;
    description: string;
    price: number;
    priceSale?: number | null;
    categoryId: string;
    publisherId: string;
    authorIds: string[];
  }): string {
    const newId = data.id || uuidv7();
    const category =
      mockCategories.find((c) => c.id === data.categoryId) ?? null;
    const publisher =
      mockPublishers.find((p) => p.id === data.publisherId) ?? null;
    const authors = data.authorIds
      .map((authorId) => mockAuthors.find((a) => a.id === authorId))
      .filter((author): author is Author => author !== undefined);

    this.books.push({
      id: newId,
      name: data.name,
      description: data.description,
      imageUrl: null,
      price: data.price,
      priceSale: data.priceSale ?? null,
      status: "InStock",
      category,
      publisher,
      authors,
      averageRating: 0,
      totalReviews: 0,
    });
    return newId;
  },

  update(data: {
    id: string;
    name: string;
    description: string;
    price: number;
    priceSale?: number | null;
    categoryId: string;
    publisherId: string;
    authorIds: string[];
  }): boolean {
    const index = this.books.findIndex((book) => book.id === data.id);
    if (index === -1) return false;

    const existingBook = this.books[index]!;
    const category =
      mockCategories.find((c) => c.id === data.categoryId) ?? null;
    const publisher =
      mockPublishers.find((p) => p.id === data.publisherId) ?? null;
    const authors = data.authorIds
      .map((authorId) => mockAuthors.find((a) => a.id === authorId))
      .filter((author): author is Author => author !== undefined);

    this.books[index] = {
      id: data.id,
      name: data.name,
      description: data.description,
      imageUrl: existingBook.imageUrl,
      price: data.price,
      priceSale: data.priceSale ?? null,
      status: existingBook.status,
      category,
      publisher,
      authors,
      averageRating: existingBook.averageRating,
      totalReviews: existingBook.totalReviews,
    };
    return true;
  },

  delete(id: string): boolean {
    const index = this.books.findIndex((book) => book.id === id);
    if (index === -1) return false;

    this.books.splice(index, 1);
    return true;
  },

  reset(): void {
    this.books = [...mockBooks];
  },
};
