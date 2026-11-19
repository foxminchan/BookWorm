import type { CreateFeedbackRequest } from "@workspace/types/rating";

export const INITIAL_REVIEW: Omit<CreateFeedbackRequest, "bookId"> = {
  firstName: "",
  lastName: "",
  comment: "",
  rating: 0,
};

export type BookDetailState = {
  showReviewForm: boolean;
  currentPage: number;
  showRemoveDialog: boolean;
  sortBy: "newest" | "highest" | "lowest";
  newReview: Omit<CreateFeedbackRequest, "bookId">;
};

export type BookDetailAction =
  | { type: "TOGGLE_REVIEW_FORM" }
  | { type: "HIDE_REVIEW_FORM" }
  | { type: "SET_PAGE"; page: number }
  | { type: "SHOW_REMOVE_DIALOG" }
  | { type: "HIDE_REMOVE_DIALOG" }
  | { type: "SET_SORT"; sortBy: "newest" | "highest" | "lowest" }
  | { type: "UPDATE_REVIEW"; field: string; value: string | number }
  | { type: "RESET_REVIEW" };

export const initialBookDetailState: BookDetailState = {
  showReviewForm: false,
  currentPage: 1,
  showRemoveDialog: false,
  sortBy: "newest",
  newReview: INITIAL_REVIEW,
};

export function bookDetailReducer(
  state: BookDetailState,
  action: BookDetailAction,
): BookDetailState {
  switch (action.type) {
    case "TOGGLE_REVIEW_FORM":
      return { ...state, showReviewForm: !state.showReviewForm };
    case "HIDE_REVIEW_FORM":
      return { ...state, showReviewForm: false };
    case "SET_PAGE":
      return { ...state, currentPage: action.page };
    case "SHOW_REMOVE_DIALOG":
      return { ...state, showRemoveDialog: true };
    case "HIDE_REMOVE_DIALOG":
      return { ...state, showRemoveDialog: false };
    case "SET_SORT":
      return { ...state, sortBy: action.sortBy, currentPage: 1 };
    case "UPDATE_REVIEW":
      return {
        ...state,
        newReview: { ...state.newReview, [action.field]: action.value },
      };
    case "RESET_REVIEW":
      return {
        ...state,
        newReview: INITIAL_REVIEW,
        showReviewForm: false,
      };
    default:
      return state;
  }
}
