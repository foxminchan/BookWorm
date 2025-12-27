import { http, HttpResponse } from "msw";
import type {
  CategoryDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import { categoriesStore } from "./data.js";
import { generateTraceId } from "../../helpers/trace";
import { CATALOG_API_BASE_URL } from "../constants";

export const categoriesHandlers = [
  http.get<never, never, CategoryDto[]>(
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
    () => {
      const categories = categoriesStore.getAll();
      return HttpResponse.json(categories, { status: 200 });
    },
  ),

  http.post<never, CreateCategoryRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
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

      const newCategoryId = categoriesStore.create(body.name);
      return HttpResponse.json(newCategoryId, { status: 200 });
    },
  ),

  http.put<never, UpdateCategoryRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
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

      const updated = categoriesStore.update(body.id, body.name);

      if (!updated) {
        return new HttpResponse("Category not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/categories/:id`,
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

      const deleted = categoriesStore.delete(id);

      if (!deleted) {
        return new HttpResponse("Category not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
