import { http, HttpResponse } from "msw";
import type {
  Publisher,
  CreatePublisherRequest,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import {
  createPublisherSchema,
  updatePublisherSchema,
} from "@workspace/validations/catalog/publishers";
import { publishersStore } from "./data";
import { formatValidationErrors } from "@workspace/utils/validation";
import { generateTraceId } from "@workspace/utils/trace";
import { BASE_URL } from "../../constants";

export const publishersHandlers = [
  http.get<never, never, Publisher[]>(`${BASE_URL}/api/v1/publishers`, () => {
    const publishers = publishersStore.getAll();
    return HttpResponse.json(publishers, { status: 200 });
  }),

  http.post<never, CreatePublisherRequest>(
    `${BASE_URL}/api/v1/publishers`,
    async ({ request }) => {
      const body = await request.json();
      const result = createPublisherSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newPublisherId = publishersStore.create(result.data.name);
      return HttpResponse.json(newPublisherId, { status: 200 });
    },
  ),

  http.put<never, UpdatePublisherRequest>(
    `${BASE_URL}/api/v1/publishers`,
    async ({ request }) => {
      const body = await request.json();
      const result = updatePublisherSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = publishersStore.update(result.data.id, result.data.name);

      if (!updated) {
        return new HttpResponse("Publisher not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${BASE_URL}/api/v1/publishers/:id`,
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

      const deleted = publishersStore.delete(id);

      if (!deleted) {
        return new HttpResponse("Publisher not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
