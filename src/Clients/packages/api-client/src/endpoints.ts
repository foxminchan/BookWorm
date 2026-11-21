/**
 * Shared API endpoint path definitions for BookWorm services.
 * Dynamic path builders default to route parameter tokens (e.g. `:id`)
 * so MSW handlers and API clients share a single source of truth.
 */
export const API_ENDPOINTS = {
  basket: {
    root: "/basket/api/v1/baskets",
  },
  catalog: {
    authors: {
      root: "/catalog/api/v1/authors",
      byId: (id = ":id") => `/catalog/api/v1/authors/${id}`,
    },
    books: {
      root: "/catalog/api/v1/books",
      byId: (id = ":id") => `/catalog/api/v1/books/${id}`,
    },
    categories: {
      root: "/catalog/api/v1/categories",
      byId: (id = ":id") => `/catalog/api/v1/categories/${id}`,
    },
    publishers: {
      root: "/catalog/api/v1/publishers",
      byId: (id = ":id") => `/catalog/api/v1/publishers/${id}`,
    },
  },
  chatting: {
    agUi: "/chatting/ag-ui",
  },
  ordering: {
    buyers: {
      root: "/ordering/api/v1/buyers",
      me: "/ordering/api/v1/buyers/me",
      address: "/ordering/api/v1/buyers/address",
      byId: (id = ":id") => `/ordering/api/v1/buyers/${id}`,
    },
    dashboard: {
      root: "/ordering/api/v1/dashboard",
      stream: "/ordering/api/v1/dashboard/stream",
    },
    orders: {
      root: "/ordering/api/v1/orders",
      byId: (id = ":id") => `/ordering/api/v1/orders/${id}`,
      history: (id = ":id") => `/ordering/api/v1/orders/${id}/history`,
      summary: (id = ":id") => `/ordering/api/v1/orders/${id}/summary`,
      cancel: (id = ":id") => `/ordering/api/v1/orders/${id}/cancel`,
      complete: (id = ":id") => `/ordering/api/v1/orders/${id}/complete`,
    },
  },
  rating: {
    feedbacks: {
      root: "/rating/api/v1/feedbacks",
      byId: (id = ":id") => `/rating/api/v1/feedbacks/${id}`,
      summarize: (bookId = ":bookId") =>
        `/rating/api/v1/feedbacks/${bookId}/summarize`,
    },
  },
} as const;
