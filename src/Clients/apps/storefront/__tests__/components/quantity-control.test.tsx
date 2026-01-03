import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { QuantityControl } from "@/components/quantity-control";

import { renderWithProviders } from "../utils/test-utils";

describe("QuantityControl", () => {
  describe("simple variant", () => {
    it("should render quantity value", () => {
      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
        />,
      );

      expect(screen.getByText("5")).toBeInTheDocument();
    });

    it("should call onIncrease when plus button is clicked", async () => {
      const user = userEvent.setup();
      const handleIncrease = vi.fn();

      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={handleIncrease}
          onDecrease={vi.fn()}
        />,
      );

      const increaseButton = screen.getByRole("button", { name: /increase/i });
      await user.click(increaseButton);

      expect(handleIncrease).toHaveBeenCalledTimes(1);
    });

    it("should call onDecrease when minus button is clicked", async () => {
      const user = userEvent.setup();
      const handleDecrease = vi.fn();

      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={handleDecrease}
        />,
      );

      const decreaseButton = screen.getByRole("button", { name: /decrease/i });
      await user.click(decreaseButton);

      expect(handleDecrease).toHaveBeenCalledTimes(1);
    });

    it("should render with small size", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          size="sm"
        />,
      );

      const wrapper = container.querySelector(".h-8");
      expect(wrapper).toBeInTheDocument();
    });

    it("should render with medium size by default", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
        />,
      );

      const wrapper = container.querySelector(".h-10");
      expect(wrapper).toBeInTheDocument();
    });

    it("should render with large size", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          size="lg"
        />,
      );

      const wrapper = container.querySelector(".h-12");
      expect(wrapper).toBeInTheDocument();
    });

    it("should apply custom className", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          className="custom-class"
        />,
      );

      const wrapper = container.querySelector(".custom-class");
      expect(wrapper).toBeInTheDocument();
    });
  });

  describe("input variant", () => {
    it("should render input field with quantity value", () => {
      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          variant="input"
          onQuantityChange={vi.fn()}
        />,
      );

      const input = screen.getByRole("textbox", { name: "Book quantity" });
      expect(input).toHaveValue("5");
    });

    it("should call onQuantityChange when input value changes", async () => {
      const user = userEvent.setup();
      const handleQuantityChange = vi.fn();

      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          variant="input"
          onQuantityChange={handleQuantityChange}
        />,
      );

      const input = screen.getByRole("textbox", { name: "Book quantity" });
      await user.clear(input);
      await user.type(input, "10");

      expect(handleQuantityChange).toHaveBeenCalled();
    });

    it("should call onIncrease when plus button is clicked in input variant", async () => {
      const user = userEvent.setup();
      const handleIncrease = vi.fn();

      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={handleIncrease}
          onDecrease={vi.fn()}
          variant="input"
          onQuantityChange={vi.fn()}
        />,
      );

      const buttons = screen.getAllByRole("button");
      const increaseButton = buttons[1]!; // Second button is increase
      await user.click(increaseButton);

      expect(handleIncrease).toHaveBeenCalledTimes(1);
    });

    it("should call onDecrease when minus button is clicked in input variant", async () => {
      const user = userEvent.setup();
      const handleDecrease = vi.fn();

      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={handleDecrease}
          variant="input"
          onQuantityChange={vi.fn()}
        />,
      );

      const buttons = screen.getAllByRole("button");
      const decreaseButton = buttons[0]!; // First button is decrease
      await user.click(decreaseButton);

      expect(handleDecrease).toHaveBeenCalledTimes(1);
    });

    it("should have min value of 1 for input", () => {
      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          variant="input"
          onQuantityChange={vi.fn()}
        />,
      );

      const input = screen.getByRole("textbox", { name: "Book quantity" });
      expect(input).toHaveAttribute("min", "1");
    });

    it("should have max value of 99 for input", () => {
      renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          variant="input"
          onQuantityChange={vi.fn()}
        />,
      );

      const input = screen.getByRole("textbox", { name: "Book quantity" });
      expect(input).toHaveAttribute("max", "99");
    });
  });

  describe("showBorder prop", () => {
    it("should show border by default", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
        />,
      );

      const wrapper = container.querySelector(".rounded-full");
      expect(wrapper).toBeInTheDocument();
    });

    it("should not show border when showBorder is false", () => {
      const { container } = renderWithProviders(
        <QuantityControl
          quantity={5}
          onIncrease={vi.fn()}
          onDecrease={vi.fn()}
          showBorder={false}
        />,
      );

      // When showBorder is false, border classes should not be applied
      const wrapper = container.firstChild;
      expect(wrapper).toBeInTheDocument();
    });
  });
});
