import { http, HttpResponse } from "msw";
import type {
  Book,
  CreateBookRequest,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import {
  createBookSchema,
  updateBookSchema,
} from "@workspace/validations/catalog/books";
import { booksStore } from "./data";
import { formatValidationErrors } from "@workspace/utils/validation";
import { generateTraceId } from "@workspace/utils/trace";
import { buildPaginationLinks } from "@workspace/utils/link";
import { CATALOG_API_BASE_URL } from "@/catalog/constants";
import { PagedResult } from "@workspace/types/shared";

export const booksHandlers = [
  http.get<never, never, PagedResult<Book>>(
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

      const result = booksStore.list({ pageIndex, pageSize, searchTerm });

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

      const book = booksStore.get(id);

      if (!book) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return HttpResponse.json(book, { status: 200 });
    },
  ),

  http.post<never, CreateBookRequest>(
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
                "Content-Type must be multipart/form-data for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const body = await request.json();
      const result = createBookSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newBookId = booksStore.create(result.data);

      const headers = new Headers();
      headers.set("Location", `/api/v1/books/${newBookId}`);

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
                "Content-Type must be multipart/form-data for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const body = await request.json();
      const result = updateBookSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = booksStore.update(result.data);

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
