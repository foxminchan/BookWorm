import { http, HttpResponse } from "msw";
import type {
  BookDto,
  CreateBookRequest,
  UpdateBookRequest,
  ListBooksQuery,
  PagedResult,
} from "@workspace/types/catalog/books";
import { booksStore } from "./data.js";
import { generateTraceId, buildPaginationLinks } from "../../helpers/index.js";
import { CATALOG_API_BASE_URL } from "../constants.js";

export const booksHandlers = [
  http.get<never, never, PagedResult<BookDto>>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    ({ request }) => {
      const url = new URL(request.url);
      const pageIndex = Number.parseInt(
        url.searchParams.get("pageIndex") || "1",
      );
      const pageSize = Number.parseInt(
        url.searchParams.get("pageSize") || "10",
      );
      const searchTerm = url.searchParams.get("search") || undefined;

      const result = booksStore.getAll({ pageIndex, pageSize, searchTerm });

      const links = buildPaginationLinks(
        request.url,
        result.pageIndex,
        result.pageSize,
        result.totalPages,
        result.hasPreviousPage,
        result.hasNextPage,
      );

      const headers = new Headers();
      headers.set("Link", links.join(","));
      headers.set("Pagination-Count", result.totalCount.toString());

      return HttpResponse.json(result, { status: 200, headers });
    },
  ),

  http.get<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/books/:id`,
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Id: ["'Id' must not be empty."],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const book = booksStore.getById(id);

      if (!book) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return HttpResponse.json(book, { status: 200 });
    },
  ),

  http.post<never, CreateBookRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    async ({ request }) => {
      // Check Content-Type
      const contentType = request.headers.get("Content-Type");
      if (!contentType?.includes("multipart/form-data")) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              ContentType: [
                "'Content-Type' must be 'multipart/form-data' for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const body = await request.json();
      const errors: Record<string, string[]> = {};

      if (!body.name || body.name.trim().length === 0) {
        errors.Name = ["'Name' must not be empty."];
      }

      if (!body.description || body.description.trim().length === 0) {
        errors.Description = ["'Description' must not be empty."];
      }

      if (body.price <= 0) {
        errors.Price = ["'Price' must be greater than 0."];
      }

      if (!body.categoryId) {
        errors.CategoryId = ["'Category Id' must not be empty."];
      }

      if (!body.publisherId) {
        errors.PublisherId = ["'Publisher Id' must not be empty."];
      }

      if (!body.authorIds || body.authorIds.length === 0) {
        errors.AuthorIds = ["'Author Ids' must contain at least one item."];
      }

      if (Object.keys(errors).length > 0) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors,
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const newBookId = booksStore.create(body);

      const headers = new Headers();
      headers.set(
        "Location",
        `${CATALOG_API_BASE_URL}/api/v1/books/${newBookId}`,
      );

      return HttpResponse.json(newBookId, { status: 201, headers });
    },
  ),

  http.put<never, UpdateBookRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    async ({ request }) => {
      const contentType = request.headers.get("Content-Type");
      if (!contentType?.includes("multipart/form-data")) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              ContentType: [
                "'Content-Type' must be 'multipart/form-data' for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const body = await request.json();
      const errors: Record<string, string[]> = {};

      if (!body.id) {
        errors.Id = ["'Id' must not be empty."];
      }

      if (!body.name || body.name.trim().length === 0) {
        errors.Name = ["'Name' must not be empty."];
      }

      if (!body.description || body.description.trim().length === 0) {
        errors.Description = ["'Description' must not be empty."];
      }

      if (body.price <= 0) {
        errors.Price = ["'Price' must be greater than 0."];
      }

      if (!body.categoryId) {
        errors.CategoryId = ["'Category Id' must not be empty."];
      }

      if (!body.publisherId) {
        errors.PublisherId = ["'Publisher Id' must not be empty."];
      }

      if (!body.authorIds || body.authorIds.length === 0) {
        errors.AuthorIds = ["'Author Ids' must contain at least one item."];
      }

      if (Object.keys(errors).length > 0) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors,
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const updated = booksStore.update(body);

      if (!updated) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/books/:id`,
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Id: ["'Id' must not be empty."],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const deleted = booksStore.delete(id);

      if (!deleted) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
