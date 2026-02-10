import type {
  CreateFeedbackRequest,
  Feedback,
  FeedbackSummary,
  ListFeedbacksQuery,
} from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";

import { apiClient } from "../client";
import type ApiClient from "../client";

class RatingApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(
    query?: ListFeedbacksQuery,
  ): Promise<PagedResult<Feedback>> {
    const response = await this.client.get<Feedback[]>(
      "/rating/api/v1/feedbacks",
      {
        params: query,
      },
    );

    return {
      items: response.data,
      totalCount: Number(response.headers["pagination-count"] || 0),
      link: response.headers["link"],
    };
  }

  public async create(request: CreateFeedbackRequest): Promise<Feedback> {
    const response = await this.client.post<Feedback>(
      "/rating/api/v1/feedbacks",
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/rating/api/v1/feedbacks/${id}`);
  }

  public async summary(bookId: string): Promise<FeedbackSummary> {
    const response = await this.client.get<FeedbackSummary>(
      `/rating/api/v1/feedbacks/${bookId}/summarize`,
    );
    return response.data;
  }
}

export default new RatingApiClient();
