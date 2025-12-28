import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Feedback,
  CreateFeedbackRequest,
  ListFeedbacksQuery,
} from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";

export class RatingApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listFeedbacks(
    query?: ListFeedbacksQuery,
  ): Promise<PagedResult<Feedback>> {
    return this.client.get<PagedResult<Feedback>>("/rating/api/v1/feedbacks", {
      params: query,
    });
  }

  async getFeedback(id: string): Promise<Feedback> {
    return this.client.get<Feedback>(`/rating/api/v1/feedbacks/${id}`);
  }

  async createFeedback(request: CreateFeedbackRequest): Promise<Feedback> {
    return this.client.post<Feedback>("/rating/api/v1/feedbacks", request);
  }

  async updateFeedback(
    id: string,
    request: CreateFeedbackRequest,
  ): Promise<Feedback> {
    return this.client.put<Feedback>(`/rating/api/v1/feedbacks/${id}`, request);
  }

  async deleteFeedback(id: string): Promise<void> {
    return this.client.delete<void>(`/rating/api/v1/feedbacks/${id}`);
  }

  async getBookFeedbacks(
    bookId: string,
    params?: { pageIndex?: number; pageSize?: number },
  ): Promise<PagedResult<Feedback>> {
    return this.client.get<PagedResult<Feedback>>(
      `/rating/api/v1/books/${bookId}/feedbacks`,
      { params },
    );
  }
}

export const ratingApiClient = new RatingApiClient();
