import type {
  CreateFeedbackRequest,
  Feedback,
  FeedbackSummary,
  ListFeedbacksQuery,
} from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";

import { HttpHeaders, apiClient, getHeaderValue } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class RatingApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(
    query?: ListFeedbacksQuery,
  ): Promise<PagedResult<Feedback>> {
    const response = await this.client.get<Feedback[]>(
      API_ENDPOINTS.rating.feedbacks.root,
      {
        params: query,
      },
    );

    return {
      items: response.data,
      totalCount: Number(
        getHeaderValue(response.headers, HttpHeaders.PaginationCount) || 0,
      ),
      link: getHeaderValue(response.headers, HttpHeaders.Link),
    };
  }

  public async create(request: CreateFeedbackRequest): Promise<Feedback> {
    const response = await this.client.post<Feedback>(
      API_ENDPOINTS.rating.feedbacks.root,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.rating.feedbacks.byId(id));
  }

  public async summary(bookId: string): Promise<FeedbackSummary> {
    const response = await this.client.get<FeedbackSummary>(
      API_ENDPOINTS.rating.feedbacks.summarize(bookId),
    );
    return response.data;
  }
}

export default new RatingApiClient();
