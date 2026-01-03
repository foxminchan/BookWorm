import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ProductActions from "@/features/catalog/product/product-actions";

import { renderWithProviders } from "../../utils/test-utils";

// Mock next/navigation
vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}));

// Mock auth-client
vi.mock("@/lib/auth-client", () => ({
  useSession: () => ({
    data: { user: { id: "1", name: "Test User" } },
  }),
}));

describe("ProductActions", () => {
  const mockOnAddToBasket = vi.fn();
  const mockOnQuantityChange = vi.fn();
  const mockOnDecrease = vi.fn();
  const mockOnIncrease = vi.fn();

  const defaultProps = {
    quantity: 0,
    status: "InStock",
    isAddingToBasket: false,
    onAddToBasket: mockOnAddToBasket,
    onQuantityChange: mockOnQuantityChange,
    onDecrease: mockOnDecrease,
    onIncrease: mockOnIncrease,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should display add to basket button when quantity is 0", () => {
    renderWithProviders(<ProductActions {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: /add to basket/i }),
    ).toBeInTheDocument();
  });

  it("should call onAddToBasket when clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ProductActions {...defaultProps} />);

    const addButton = screen.getByRole("button", { name: /add to basket/i });
    await user.click(addButton);

    expect(mockOnAddToBasket).toHaveBeenCalledTimes(1);
  });

  it("should disable button when out of stock", () => {
    renderWithProviders(
      <ProductActions {...defaultProps} status="OutOfStock" />,
    );

    const addButton = screen.getByRole("button", { name: /add to basket/i });
    expect(addButton).toBeDisabled();
  });

  it("should show loading state", () => {
    renderWithProviders(
      <ProductActions {...defaultProps} isAddingToBasket={true} />,
    );

    expect(screen.getByText("Adding...")).toBeInTheDocument();
  });

  it("should disable during loading", () => {
    renderWithProviders(
      <ProductActions {...defaultProps} isAddingToBasket={true} />,
    );

    const addButton = screen.getByRole("button", { name: /adding/i });
    expect(addButton).toBeDisabled();
  });

  it("should display quantity control when quantity > 0", () => {
    renderWithProviders(<ProductActions {...defaultProps} quantity={5} />);

    expect(
      screen.queryByRole("button", { name: /add to basket/i }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("textbox")).toBeInTheDocument();
  });

  it("should have shopping basket icon", () => {
    const { container } = renderWithProviders(
      <ProductActions {...defaultProps} />,
    );

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
  });

  it("should have loader icon when adding", () => {
    const { container } = renderWithProviders(
      <ProductActions {...defaultProps} isAddingToBasket={true} />,
    );

    const loader = container.querySelector(".animate-spin");
    expect(loader).toBeInTheDocument();
  });

  it("should pass quantity to control component", () => {
    renderWithProviders(<ProductActions {...defaultProps} quantity={3} />);

    const input = screen.getByRole("textbox") as HTMLInputElement;
    expect(input.value).toBe("3");
  });

  it("should have correct button styling", () => {
    renderWithProviders(<ProductActions {...defaultProps} />);

    const button = screen.getByRole("button", { name: /add to basket/i });
    expect(button).toHaveClass("rounded-full");
  });

  it("should display out of stock correctly", () => {
    renderWithProviders(
      <ProductActions {...defaultProps} status="OutOfStock" />,
    );

    const button = screen.getByRole("button", { name: /add to basket/i });
    expect(button).toBeDisabled();
    expect(screen.getByText(/add to basket/i)).toBeInTheDocument();
  });
});
