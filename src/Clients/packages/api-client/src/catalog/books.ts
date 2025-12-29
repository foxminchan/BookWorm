import ApiClient from "@/client";
import type {
  Book,
  CreateBookRequest,
  ListBooksQuery,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

class BooksApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(query?: ListBooksQuery): Promise<PagedResult<Book>> {
    const response = await this.client.get<Book[]>("/catalog/api/v1/books", {
      params: query,
    });

    return {
      items: response.data,
      totalCount: Number(response.headers["Pagination-Count"] || 0),
      link: response.headers["Link"],
    };
  }

  public async get(id: string): Promise<Book> {
    const response = await this.client.get<Book>(`/catalog/api/v1/books/${id}`);
    return response.data;
  }

  public async create(request: CreateBookRequest): Promise<Book> {
    const response = await this.client.post<Book>(
      "/catalog/api/v1/books",
      request,
    );
    return response.data;
  }

  public async update(request: UpdateBookRequest): Promise<Book> {
    const response = await this.client.put<Book>(
      `/catalog/api/v1/books`,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/catalog/api/v1/books/${id}`);
  }
}

export default new BooksApiClient();
