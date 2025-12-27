import { v7 as uuidv7 } from "uuid";
import type { BookDto } from "@workspace/types/catalog/books";

const getMockCategoryId = () => uuidv7();
const getMockPublisherId = () => uuidv7();
const getMockAuthorId = () => uuidv7();

export const mockBooks: BookDto[] = [
  {
    id: uuidv7(),
    name: "Harry Potter and the Philosopher's Stone",
    description:
      "The first novel in the Harry Potter series and Rowling's debut novel.",
    imageUrl: "https://placehold.co/400x600/png?text=Harry+Potter",
    price: 25.99,
    priceSale: 19.99,
    status: "InStock",
    category: { id: getMockCategoryId(), name: "Fantasy" },
    publisher: { id: getMockPublisherId(), name: "Bloomsbury Publishing" },
    authors: [{ id: getMockAuthorId(), name: "J.K. Rowling" }],
    averageRating: 4.8,
    totalReviews: 1250,
  },
  {
    id: uuidv7(),
    name: "The Hobbit",
    description: "A fantasy novel and children's book by J.R.R. Tolkien.",
    imageUrl: "https://placehold.co/400x600/png?text=The+Hobbit",
    price: 22.5,
    priceSale: null,
    status: "InStock",
    category: { id: getMockCategoryId(), name: "Fantasy" },
    publisher: { id: getMockPublisherId(), name: "Oxford University Press" },
    authors: [{ id: getMockAuthorId(), name: "J.R.R. Tolkien" }],
    averageRating: 4.7,
    totalReviews: 980,
  },
  {
    id: uuidv7(),
    name: "1984",
    description:
      "A dystopian social science fiction novel by English novelist George Orwell.",
    imageUrl: "https://placehold.co/400x600/png?text=1984",
    price: 18.99,
    priceSale: 14.99,
    status: "InStock",
    category: { id: getMockCategoryId(), name: "Fiction" },
    publisher: { id: getMockPublisherId(), name: "Penguin Random House" },
    authors: [{ id: getMockAuthorId(), name: "George Orwell" }],
    averageRating: 4.6,
    totalReviews: 2100,
  },
  {
    id: uuidv7(),
    name: "The Shining",
    description: "A horror novel by American author Stephen King.",
    imageUrl: "https://placehold.co/400x600/png?text=The+Shining",
    price: 21.99,
    priceSale: null,
    status: "OutOfStock",
    category: { id: getMockCategoryId(), name: "Thriller" },
    publisher: { id: getMockPublisherId(), name: "Simon & Schuster" },
    authors: [{ id: getMockAuthorId(), name: "Stephen King" }],
    averageRating: 4.5,
    totalReviews: 850,
  },
  {
    id: uuidv7(),
    name: "And Then There Were None",
    description: "A mystery novel by Agatha Christie.",
    imageUrl: "https://placehold.co/400x600/png?text=And+Then+There+Were+None",
    price: 16.99,
    priceSale: 12.99,
    status: "InStock",
    category: { id: getMockCategoryId(), name: "Mystery" },
    publisher: { id: getMockPublisherId(), name: "HarperCollins" },
    authors: [{ id: getMockAuthorId(), name: "Agatha Christie" }],
    averageRating: 4.7,
    totalReviews: 1500,
  },
];

export const booksStore = {
  books: [...mockBooks],

  getAll(query?: {
    pageIndex?: number;
    pageSize?: number;
    searchTerm?: string;
  }): {
    items: BookDto[];
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

  getById(id: string): BookDto | undefined {
    return this.books.find((book) => book.id === id);
  },

  create(data: {
    name: string;
    description: string;
    price: number;
    priceSale?: number | null;
    categoryId: string;
    publisherId: string;
    authorIds: string[];
  }): string {
    const newId = uuidv7();
    this.books.push({
      id: newId,
      name: data.name,
      description: data.description,
      imageUrl: null,
      price: data.price,
      priceSale: data.priceSale ?? null,
      status: "InStock",
      category: { id: data.categoryId, name: "Mock Category" },
      publisher: { id: data.publisherId, name: "Mock Publisher" },
      authors: data.authorIds.map((id) => ({ id, name: "Mock Author" })),
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
    this.books[index] = {
      id: data.id,
      name: data.name,
      description: data.description,
      imageUrl: existingBook.imageUrl,
      price: data.price,
      priceSale: data.priceSale ?? null,
      status: existingBook.status,
      category: { id: data.categoryId, name: "Mock Category" },
      publisher: { id: data.publisherId, name: "Mock Publisher" },
      authors: data.authorIds.map((id) => ({ id, name: "Mock Author" })),
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
