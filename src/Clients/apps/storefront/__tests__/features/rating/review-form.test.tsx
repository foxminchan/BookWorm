import { act, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ReviewForm from "@/features/catalog/product/review-form";

import { renderWithProviders } from "../../utils/test-utils";

describe("ReviewForm", () => {
  const mockOnChange = vi.fn();
  const mockOnSubmit = vi.fn();

  const defaultProps = {
    firstName: "",
    lastName: "",
    comment: "",
    rating: 0,
    isSubmitting: false,
    onChange: mockOnChange,
    onSubmit: mockOnSubmit,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render form with all fields", () => {
    renderWithProviders(<ReviewForm {...defaultProps} />);

    expect(screen.getByText("Your Review")).toBeInTheDocument();
    expect(screen.getByText("Rating")).toBeInTheDocument();
    expect(screen.getByLabelText("First Name")).toBeInTheDocument();
    expect(screen.getByLabelText("Last Name")).toBeInTheDocument();
    expect(screen.getByLabelText("Comment")).toBeInTheDocument();
  });

  it("should render submit button", () => {
    renderWithProviders(<ReviewForm {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /submit review/i }),
    ).toBeInTheDocument();
  });

  it("should render 5 star rating buttons", () => {
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const starButtons = screen
      .getAllByRole("button")
      .filter((button) => button.querySelector("svg"));
    expect(starButtons).toHaveLength(6); // 5 stars + 1 submit button
  });

  it("should call onChange when star is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const starButtons = screen
      .getAllByRole("button")
      .filter((button) => button.querySelector("svg"));
    await user.click(starButtons[3]!); // Click 4th star

    expect(mockOnChange).toHaveBeenCalledWith("rating", 4);
  });

  it("should call onChange when first name is typed", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const firstNameInput = screen.getByPlaceholderText("First Name");
    await user.type(firstNameInput, "John");

    await waitFor(() => {
      expect(mockOnChange).toHaveBeenCalledWith("firstName", "John");
    });
  });

  it("should call onChange when last name is typed", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const lastNameInput = screen.getByPlaceholderText("Last Name");
    await user.type(lastNameInput, "Doe");

    await waitFor(() => {
      expect(mockOnChange).toHaveBeenCalledWith("lastName", "Doe");
    });
  });

  it("should call onChange when comment is typed", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const commentInput = screen.getByPlaceholderText("Share your thoughts...");
    await user.type(commentInput, "Great book!");

    await waitFor(() => {
      expect(mockOnChange).toHaveBeenCalledWith("comment", "Great book!");
    });
  });

  it("should display filled stars for selected rating", () => {
    const { container } = renderWithProviders(
      <ReviewForm {...defaultProps} rating={3} />,
    );

    const filledStars = container.querySelectorAll(".fill-primary");
    expect(filledStars.length).toBeGreaterThanOrEqual(3);
  });

  it("should pre-fill form with provided values", async () => {
    act(() => {
      renderWithProviders(
        <ReviewForm
          {...defaultProps}
          firstName="John"
          lastName="Doe"
          comment="Great book!"
          rating={4}
        />,
      );
    });

    await waitFor(() => {
      expect(screen.getByDisplayValue("John")).toBeInTheDocument();
      expect(screen.getByDisplayValue("Doe")).toBeInTheDocument();
      expect(screen.getByDisplayValue("Great book!")).toBeInTheDocument();
    });
  });

  it("should disable submit button when submitting", () => {
    renderWithProviders(<ReviewForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /submitting/i });
    expect(submitButton).toBeDisabled();
  });

  it("should show loading spinner when submitting", () => {
    renderWithProviders(<ReviewForm {...defaultProps} isSubmitting={true} />);

    expect(screen.getByText("Submitting...")).toBeInTheDocument();
  });

  it("should call onSubmit when form is submitted with valid data", async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <ReviewForm
        {...defaultProps}
        firstName="John"
        lastName="Doe"
        comment="Great book!"
        rating={4}
      />,
    );

    const submitButton = screen.getByRole("button", { name: /submit review/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalled();
    });
  });

  it("should have proper grid layout for name fields", () => {
    const { container } = renderWithProviders(<ReviewForm {...defaultProps} />);

    const grid = container.querySelector(".grid-cols-2");
    expect(grid).toBeInTheDocument();
  });

  it("should render textarea with proper rows", () => {
    renderWithProviders(<ReviewForm {...defaultProps} />);

    const textarea = screen.getByPlaceholderText("Share your thoughts...");
    expect(textarea).toHaveAttribute("rows", "4");
  });

  it("should have rounded corners on form container", () => {
    const { container } = renderWithProviders(<ReviewForm {...defaultProps} />);

    const formContainer = container.querySelector(".rounded-2xl");
    expect(formContainer).toBeInTheDocument();
  });
});
