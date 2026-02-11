export const basketKeys = {
  all: ["basket"] as const,
  detail: () => [...basketKeys.all, "detail"] as const,
};

const catalogBase = ["catalog"] as const;

export const catalogKeys = {
  all: catalogBase,

  books: {
    all: [...catalogBase, "books"] as const,
    lists: () => [...catalogBase, "books", "list"] as const,
    list: (query?: unknown) =>
      [...catalogBase, "books", "list", query] as const,
    details: () => [...catalogBase, "books", "detail"] as const,
    detail: (id: string) => [...catalogBase, "books", "detail", id] as const,
  },

  authors: {
    all: [...catalogBase, "authors"] as const,
    lists: () => [...catalogBase, "authors", "list"] as const,
    list: () => [...catalogBase, "authors", "list"] as const,
    details: () => [...catalogBase, "authors", "detail"] as const,
    detail: (id: string) => [...catalogBase, "authors", "detail", id] as const,
  },

  categories: {
    all: [...catalogBase, "categories"] as const,
    lists: () => [...catalogBase, "categories", "list"] as const,
    list: () => [...catalogBase, "categories", "list"] as const,
    details: () => [...catalogBase, "categories", "detail"] as const,
    detail: (id: string) =>
      [...catalogBase, "categories", "detail", id] as const,
  },

  publishers: {
    all: [...catalogBase, "publishers"] as const,
    lists: () => [...catalogBase, "publishers", "list"] as const,
    list: () => [...catalogBase, "publishers", "list"] as const,
    details: () => [...catalogBase, "publishers", "detail"] as const,
    detail: (id: string) =>
      [...catalogBase, "publishers", "detail", id] as const,
  },
};

const orderingBase = ["ordering"] as const;

export const orderingKeys = {
  all: orderingBase,

  orders: {
    all: [...orderingBase, "orders"] as const,
    lists: () => [...orderingBase, "orders", "list"] as const,
    list: (query?: unknown) =>
      [...orderingBase, "orders", "list", query] as const,
    details: () => [...orderingBase, "orders", "detail"] as const,
    detail: (id: string) => [...orderingBase, "orders", "detail", id] as const,
  },

  buyers: {
    all: [...orderingBase, "buyers"] as const,
    lists: () => [...orderingBase, "buyers", "list"] as const,
    list: (query?: unknown) =>
      [...orderingBase, "buyers", "list", query] as const,
    details: () => [...orderingBase, "buyers", "detail"] as const,
    detail: (id: string) => [...orderingBase, "buyers", "detail", id] as const,
    current: () => [...orderingBase, "buyers", "current"] as const,
  },
};

const ratingBase = ["rating"] as const;

export const ratingKeys = {
  all: ratingBase,
  feedbacks: {
    all: [...ratingBase, "feedbacks"] as const,
    lists: () => [...ratingBase, "feedbacks", "list"] as const,
    list: (query?: unknown) =>
      [...ratingBase, "feedbacks", "list", query] as const,
    details: () => [...ratingBase, "feedbacks", "detail"] as const,
    detail: (id: string) => [...ratingBase, "feedbacks", "detail", id] as const,
    byBook: (bookId: string, params?: unknown) =>
      [...ratingBase, "feedbacks", "book", bookId, params] as const,
    summary: (bookId: string) =>
      [...ratingBase, "feedbacks", "summary", bookId] as const,
  },
};
