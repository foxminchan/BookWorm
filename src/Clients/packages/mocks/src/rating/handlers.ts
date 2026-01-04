import { HttpResponse, http } from "msw";

import type { Feedback } from "@workspace/types/rating";
import { buildPaginationLinks } from "@workspace/utils/link";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createFeedbackSchema,
  listFeedbacksSchema,
} from "@workspace/validations/rating";

import { RATING_API_BASE_URL } from "../rating/constants";
import { feedbacksStoreManager } from "./data";

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

    const allFeedbacks = feedbacksStoreManager.get(bookId);

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

    const hasPreviousPage = pageIndex > 1;
    const hasNextPage = pageIndex < totalPages;

    const links = buildPaginationLinks(
      request.url,
      pageIndex,
      pageSize,
      totalPages,
      hasPreviousPage,
      hasNextPage,
    );

    const headers = new Headers();
    headers.set("Link", links.join(","));
    headers.set("Pagination-Count", totalCount.toString());

    return HttpResponse.json(items, { status: 200, headers });
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

  http.get(
    `${RATING_API_BASE_URL}/api/v1/feedbacks/:bookId/summarize`,
    ({ params }) => {
      const { bookId } = params;

      if (!bookId || typeof bookId !== "string") {
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

      const feedbacks = feedbacksStoreManager.get(bookId);

      if (feedbacks.length === 0) {
        return HttpResponse.json({
          summary: "No reviews available for this book yet.",
        });
      }

      // Generate a mock summary based on ratings
      const avgRating =
        feedbacks.reduce((sum, f) => sum + f.rating, 0) / feedbacks.length;
      const positiveCount = feedbacks.filter((f) => f.rating >= 4).length;
      const negativeCount = feedbacks.filter((f) => f.rating <= 2).length;

      let summary = "";
      if (avgRating >= 4) {
        summary = `Readers highly praise this book! ${positiveCount} out of ${feedbacks.length} reviewers gave it 4 or 5 stars. Common themes include engaging storytelling, well-developed characters, and compelling plot twists.`;
      } else if (avgRating >= 3) {
        summary = `This book receives mixed reviews from readers. While some appreciate its unique approach and interesting concepts, others find it somewhat lacking in execution. ${positiveCount} readers rated it positively, while ${negativeCount} had reservations.`;
      } else {
        summary = `Reviews for this book are generally critical. ${negativeCount} out of ${feedbacks.length} reviewers gave it 2 stars or lower. Common criticisms include pacing issues, character development concerns, and plot inconsistencies.`;
      }

      return HttpResponse.json({ summary }, { status: 200 });
    },
  ),
];
