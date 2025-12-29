import { v7 as uuidv7 } from "uuid";
import type { Feedback } from "@workspace/types/rating";
import feedbacksData from "@/data/feedbacks.json";

const mockFeedbacks: Feedback[] = feedbacksData;

let feedbacksStore = [...mockFeedbacks];

export const feedbacksStoreManager = {
  get: (bookId: string): Feedback[] => {
    return feedbacksStore.filter((f) => f.bookId === bookId);
  },

  create: (data: {
    bookId: string;
    firstName?: string | null;
    lastName?: string | null;
    comment?: string | null;
    rating: number;
    id?: string;
  }): Feedback => {
    const newFeedback: Feedback = {
      id: data.id || uuidv7(),
      bookId: data.bookId,
      firstName: data.firstName ?? null,
      lastName: data.lastName ?? null,
      comment: data.comment ?? null,
      rating: data.rating,
    };
    feedbacksStore.push(newFeedback);
    return newFeedback;
  },

  delete: (id: string): boolean => {
    const index = feedbacksStore.findIndex((f) => f.id === id);
    if (index === -1) return false;

    feedbacksStore.splice(index, 1);
    return true;
  },

  reset: () => {
    feedbacksStore = [...mockFeedbacks];
  },
};
