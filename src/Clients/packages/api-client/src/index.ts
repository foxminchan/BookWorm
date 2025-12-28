export { ApiClient, type ApiClientConfig } from "./client";
export { default as axiosConfig } from "./config";
export { BasketApiClient, basketApiClient } from "./basket";
export {
  AuthorsApiClient,
  authorsApiClient,
  BooksApiClient,
  booksApiClient,
  CategoriesApiClient,
  categoriesApiClient,
  PublishersApiClient,
  publishersApiClient,
} from "./catalog";
export {
  BuyersApiClient,
  buyersApiClient,
  OrdersApiClient,
  ordersApiClient,
} from "./ordering";
export { RatingApiClient, ratingApiClient } from "./rating";
