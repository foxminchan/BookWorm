import { http, HttpResponse } from "msw";
import type { Feedback, ListFeedbacksQuery } from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";
import { feedbacksStoreManager } from "./data";
import {
  createFeedbackSchema,
  listFeedbacksSchema,
} from "@workspace/validations/rating";
import { formatValidationErrors } from "@workspace/utils/validation";
import { buildPaginationLinks } from "@workspace/utils/link";
import { RATING_API_BASE_URL } from "./constants";

export const feedbacksHandlers = [
  http.get(`${RATING_API_BASE_URL}/api/v1/feedbacks`, ({ request }) => {
    const url = new URL(request.url);
    const bookId = url.searchParams.get("bookId");
    const pageIndex = Number.parseInt(url.searchParams.get("pageIndex") || "1");
    const pageSize = Number.parseInt(url.searchParams.get("pageSize") || "10");
    const orderBy = url.searchParams.get("orderBy") || "rating";
    const isDescending = url.searchParams.get("isDescending") === "true";

    if (!bookId) {
      return HttpResponse.json(
        {
          type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
          title: "One or more validation errors occurred.",
          status: 400,
          errors: { BookId: ["Book ID is required"] },
        },
        { status: 400 },
      );
    }

    const validation = listFeedbacksSchema.safeParse({
      bookId,
      pageIndex,
      pageSize,
      orderBy,
      isDescending,
    });

    if (!validation.success) {
      return HttpResponse.json(formatValidationErrors(validation.error), {
        status: 400,
      });
    }

    let allFeedbacks = feedbacksStoreManager.getByBookId(bookId);

    allFeedbacks.sort((a, b) => {
      const aValue = a[orderBy as keyof Feedback];
      const bValue = b[orderBy as keyof Feedback];

      if (aValue === bValue) return 0;
      if (aValue == null) return 1;
      if (bValue == null) return -1;
      const comparison = aValue > bValue ? 1 : -1;
      return isDescending ? -comparison : comparison;
    });

    const totalCount = allFeedbacks.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageIndex - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const items = allFeedbacks.slice(startIndex, endIndex);

    const result: PagedResult<Feedback> = {
      items,
      pageIndex,
      pageSize,
      totalCount,
      totalPages,
      hasPreviousPage: pageIndex > 1,
      hasNextPage: pageIndex < totalPages,
    };

    const links = buildPaginationLinks(
      request.url,
      pageIndex,
      pageSize,
      totalPages,
      result.hasPreviousPage,
      result.hasNextPage,
    );

    const headers = new Headers();
    headers.set("Link", links.join(","));
    headers.set("Pagination-Count", totalCount.toString());

    return HttpResponse.json(result, { status: 200, headers });
  }),

  http.post(`${RATING_API_BASE_URL}/api/v1/feedbacks`, async ({ request }) => {
    const body = await request.json();

    const validation = createFeedbackSchema.safeParse(body);

    if (!validation.success) {
      return HttpResponse.json(formatValidationErrors(validation.error), {
        status: 400,
      });
    }

    const feedback = feedbacksStoreManager.create(validation.data);

    return HttpResponse.json(feedback.id, { status: 200 });
  }),

  http.delete(`${RATING_API_BASE_URL}/api/v1/feedbacks/:id`, ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const deleted = feedbacksStoreManager.delete(id);

    if (!deleted) {
      return new HttpResponse(null, { status: 404 });
    }

    return new HttpResponse(null, { status: 204 });
  }),
];
