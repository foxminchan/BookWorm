export type Feedback = {
  id: string;
  firstName?: string | null;
  lastName?: string | null;
  comment?: string | null;
  rating: number;
  bookId: string;
};

export type CreateFeedbackRequest = {
  bookId: string;
  firstName?: string | null;
  lastName?: string | null;
  comment?: string | null;
  rating: number;
};

export type ListFeedbacksQuery = {
  bookId: string;
  pageIndex?: number;
  pageSize?: number;
  orderBy?: string;
  isDescending?: boolean;
};
