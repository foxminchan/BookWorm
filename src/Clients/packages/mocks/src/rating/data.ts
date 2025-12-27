import { v7 as uuidv7 } from "uuid";
import type { Feedback } from "@workspace/types/rating";
import { mockBooks } from "../catalog/books/data";

const generateFeedbackForBook = (bookId: string, count: number): Feedback[] => {
  const feedbacks: Feedback[] = [];
  const firstNames = ["John", "Jane", "Alice", "Bob", "Charlie", "Diana"];
  const lastNames = ["Smith", "Doe", "Johnson", "Williams", "Brown", "Jones"];
  const comments = [
    "Great book! Highly recommend it.",
    "Enjoyed reading this one.",
    "Not my favorite, but still good.",
    "Absolutely loved it!",
    "Could have been better.",
    null,
  ];

  for (let i = 0; i < count; i++) {
    feedbacks.push({
      id: uuidv7(),
      firstName: firstNames[i % firstNames.length],
      lastName: lastNames[i % lastNames.length],
      comment: comments[i % comments.length],
      rating: Math.floor(Math.random() * 6), // Rating between 0 and 5
      bookId,
    });
  }

  return feedbacks;
};

// Create mock feedbacks for the first 3 books
const mockFeedbacks: Feedback[] = [
  ...generateFeedbackForBook(mockBooks[0]!.id, 5), // Harry Potter
  ...generateFeedbackForBook(mockBooks[1]!.id, 3), // The Hobbit
  ...generateFeedbackForBook(mockBooks[2]!.id, 4), // 1984
];

let feedbacksStore = [...mockFeedbacks];

export const feedbacksStoreManager = {
  getAll: (): Feedback[] => {
    return feedbacksStore;
  },

  getById: (id: string): Feedback | undefined => {
    return feedbacksStore.find((f) => f.id === id);
  },

  getByBookId: (bookId: string): Feedback[] => {
    return feedbacksStore.filter((f) => f.bookId === bookId);
  },

  create: (data: {
    bookId: string;
    firstName?: string | null;
    lastName?: string | null;
    comment?: string | null;
    rating: number;
  }): Feedback => {
    const newFeedback: Feedback = {
      id: uuidv7(),
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
