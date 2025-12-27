import { http, HttpResponse } from "msw";
import type {
  AuthorDto,
  CreateAuthorRequest,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { authorsStore } from "./data";
import { generateTraceId } from "../../helpers/trace";
import { CATALOG_API_BASE_URL } from "../constants";

export const authorsHandlers = [
  http.get<never, never, AuthorDto[]>(
    `${CATALOG_API_BASE_URL}/api/v1/authors`,
    () => {
      const authors = authorsStore.getAll();
      return HttpResponse.json(authors, { status: 200 });
    },
  ),

  http.post<never, CreateAuthorRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/authors`,
    async ({ request }) => {
      const body = await request.json();

      if (!body.name || body.name.trim().length === 0) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Name: ["'Name' must not be empty."],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const newAuthorId = authorsStore.create(body.name);
      return HttpResponse.json(newAuthorId, { status: 200 });
    },
  ),

  http.put<never, UpdateAuthorRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/authors`,
    async ({ request }) => {
      const body = await request.json();

      const errors: Record<string, string[]> = {};

      if (!body.id) {
        errors.Id = ["'Id' must not be empty."];
      }

      if (!body.name || body.name.trim().length === 0) {
        errors.Name = ["'Name' must not be empty."];
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

      const updated = authorsStore.update(body.id, body.name);

      if (!updated) {
        return new HttpResponse("Author not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/authors/:id`,
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

      const deleted = authorsStore.delete(id);

      if (!deleted) {
        return new HttpResponse("Author not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
