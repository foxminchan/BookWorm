import { http, HttpResponse } from "msw";
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import {
  createCategorySchema,
  updateCategorySchema,
} from "@workspace/validations/catalog/categories";
import { categoriesStore } from "./data";
import { formatValidationErrors } from "@workspace/utils/validation";
import { generateTraceId } from "@workspace/utils/trace";
import { BASE_URL } from "../../constants";

export const categoriesHandlers = [
  http.get<never, never, Category[]>(`${BASE_URL}/api/v1/categories`, () => {
    const categories = categoriesStore.getAll();
    return HttpResponse.json(categories, { status: 200 });
  }),

  http.post<never, CreateCategoryRequest>(
    `${BASE_URL}/api/v1/categories`,
    async ({ request }) => {
      const body = await request.json();
      const result = createCategorySchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newCategoryId = categoriesStore.create(result.data.name);
      return HttpResponse.json(newCategoryId, { status: 200 });
    },
  ),

  http.put<never, UpdateCategoryRequest>(
    `${BASE_URL}/api/v1/categories`,
    async ({ request }) => {
      const body = await request.json();
      const result = updateCategorySchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = categoriesStore.update(result.data.id, result.data.name);

      if (!updated) {
        return new HttpResponse("Category not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${BASE_URL}/api/v1/categories/:id`,
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
