import type { Author } from "./authors";
import type { Category } from "./categories";
import type { Publisher } from "./publishers";

export type Book = {
  id: string;
  name: string | null;
  description: string | null;
  imageUrl: string | null;
  price: number;
  priceSale: number | null;
  status: "InStock" | "OutOfStock";
  category: Category | null;
  publisher: Publisher | null;
  authors: Author[];
  averageRating: number;
  totalReviews: number;
};

export type CreateBookRequest = {
  name: string;
  description: string;
  price: number;
  priceSale?: number | null;
  categoryId: string;
  publisherId: string;
  authorIds: string[];
  image?: File;
};

export type UpdateBookRequest = {
  id: string;
  name: string;
  description: string;
  price: number;
  priceSale?: number | null;
  categoryId: string;
  publisherId: string;
  authorIds: string[];
  image?: File;
  isRemoveImage?: boolean;
};

export type ListBooksQuery = {
  pageIndex?: number;
  pageSize?: number;
  orderBy?: string;
  isDescending?: boolean;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  categoryId?: string;
  publisherId?: string;
  authorId?: string;
};
