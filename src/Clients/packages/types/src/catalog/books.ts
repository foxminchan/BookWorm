export type BookDto = {
  id: string;
  name: string | null;
  description: string | null;
  imageUrl: string | null;
  price: number;
  priceSale: number | null;
  status: "InStock" | "OutOfStock";
  category: {
    id: string;
    name: string | null;
  };
  publisher: {
    id: string;
    name: string | null;
  };
  authors: Array<{
    id: string;
    name: string | null;
  }>;
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

export type PagedResult<T> = {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};
