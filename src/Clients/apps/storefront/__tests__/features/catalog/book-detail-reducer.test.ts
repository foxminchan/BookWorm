import { describe, expect, it } from "vitest";

import {
  type BookDetailAction,
  INITIAL_REVIEW,
  bookDetailReducer,
  initialBookDetailState,
} from "@/features/catalog/product/book-detail-reducer";

describe("bookDetailReducer", () => {
  it("toggles showReviewForm on TOGGLE_REVIEW_FORM", () => {
    const opened = bookDetailReducer(initialBookDetailState, {
      type: "TOGGLE_REVIEW_FORM",
    });
    expect(opened.showReviewForm).toBe(true);

    const closed = bookDetailReducer(opened, {
      type: "TOGGLE_REVIEW_FORM",
    });
    expect(closed.showReviewForm).toBe(false);
  });

  it("hides review form on HIDE_REVIEW_FORM", () => {
    const state = { ...initialBookDetailState, showReviewForm: true };
    const next = bookDetailReducer(state, { type: "HIDE_REVIEW_FORM" });
    expect(next.showReviewForm).toBe(false);
  });

  it("updates currentPage on SET_PAGE", () => {
    const next = bookDetailReducer(initialBookDetailState, {
      type: "SET_PAGE",
      page: 3,
    });
    expect(next.currentPage).toBe(3);
  });

  it("shows and hides remove dialog", () => {
    const shown = bookDetailReducer(initialBookDetailState, {
      type: "SHOW_REMOVE_DIALOG",
    });
    expect(shown.showRemoveDialog).toBe(true);

    const hidden = bookDetailReducer(shown, { type: "HIDE_REMOVE_DIALOG" });
    expect(hidden.showRemoveDialog).toBe(false);
  });

  it("updates sortBy and resets currentPage to 1 on SET_SORT", () => {
    const state = { ...initialBookDetailState, currentPage: 4 };
    const next = bookDetailReducer(state, {
      type: "SET_SORT",
      sortBy: "highest",
    });
    expect(next.sortBy).toBe("highest");
    expect(next.currentPage).toBe(1);
  });

  it("updates newReview field on UPDATE_REVIEW", () => {
    const withComment = bookDetailReducer(initialBookDetailState, {
      type: "UPDATE_REVIEW",
      field: "comment",
      value: "Great book!",
    });
    expect(withComment.newReview.comment).toBe("Great book!");

    const withRating = bookDetailReducer(withComment, {
      type: "UPDATE_REVIEW",
      field: "rating",
      value: 5,
    });
    expect(withRating.newReview.rating).toBe(5);
    expect(withRating.newReview.comment).toBe("Great book!");
  });

  it("resets newReview and hides review form on RESET_REVIEW", () => {
    const state = {
      ...initialBookDetailState,
      showReviewForm: true,
      newReview: {
        firstName: "Ada",
        lastName: "Lovelace",
        comment: "Wonderful",
        rating: 5,
      },
    };
    const next = bookDetailReducer(state, { type: "RESET_REVIEW" });
    expect(next.newReview).toEqual(INITIAL_REVIEW);
    expect(next.showReviewForm).toBe(false);
  });

  it("returns current state for unknown action", () => {
    const next = bookDetailReducer(initialBookDetailState, {
      type: "UNKNOWN_ACTION",
    } as unknown as BookDetailAction);
    expect(next).toBe(initialBookDetailState);
  });
});
