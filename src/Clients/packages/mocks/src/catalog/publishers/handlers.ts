import { http, HttpResponse } from "msw";
import type {
  PublisherDto,
  CreatePublisherRequest,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import { publishersStore } from "./data";
import { generateTraceId } from "../../helpers/trace";
import { CATALOG_API_BASE_URL } from "../constants";

export const publishersHandlers = [
  http.get<never, never, PublisherDto[]>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
    () => {
      const publishers = publishersStore.getAll();
      return HttpResponse.json(publishers, { status: 200 });
    },
  ),

  http.post<never, CreatePublisherRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
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

      const newPublisherId = publishersStore.create(body.name);
      return HttpResponse.json(newPublisherId, { status: 200 });
    },
  ),

  http.put<never, UpdatePublisherRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
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

      const updated = publishersStore.update(body.id, body.name);

      if (!updated) {
        return new HttpResponse("Publisher not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers/:id`,
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
