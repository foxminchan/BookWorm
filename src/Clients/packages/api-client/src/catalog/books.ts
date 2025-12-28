import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Book,
  CreateBookRequest,
  ListBooksQuery,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

export class BooksApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listBooks(query?: ListBooksQuery): Promise<PagedResult<Book>> {
    return this.client.get<PagedResult<Book>>("/catalog/api/v1/books", {
      params: query,
    });
  }

  async getBook(id: string): Promise<Book> {
    return this.client.get<Book>(`/catalog/api/v1/books/${id}`);
  }

  async createBook(request: CreateBookRequest): Promise<Book> {
    return this.client.post<Book>("/catalog/api/v1/books", request);
  }

  async updateBook(id: string, request: UpdateBookRequest): Promise<Book> {
    return this.client.put<Book>(`/catalog/api/v1/books/${id}`, request);
  }

  async deleteBook(id: string): Promise<void> {
    return this.client.delete<void>(`/catalog/api/v1/books/${id}`);
  }
}

export const booksApiClient = new BooksApiClient();
