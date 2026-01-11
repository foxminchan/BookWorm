import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { SearchInput } from "@/features/books/filters/search-input";

describe("SearchInput", () => {
  it("renders search input with placeholder", () => {
    const onChange = vi.fn();
    render(<SearchInput value="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("Search by title or author...");
    expect(input).toBeInTheDocument();
  });

  it("displays current value", () => {
    const onChange = vi.fn();
    render(<SearchInput value="test book" onChange={onChange} />);

    const input = screen.getByDisplayValue("test book");
    expect(input).toBeInTheDocument();
  });

  it("calls onChange when typing", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<SearchInput value="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("Search by title or author...");
    await user.type(input, "t");

    // onChange is called with the new value
    expect(onChange).toHaveBeenCalled();
    expect(onChange).toHaveBeenCalledWith("t");
  });

  it("renders search icon", () => {
    const onChange = vi.fn();
    const { container } = render(<SearchInput value="" onChange={onChange} />);

    const icon = container.querySelector("svg");
    expect(icon).toBeInTheDocument();
    expect(icon).toHaveClass("lucide-search");
  });

  it("has screen reader label", () => {
    const onChange = vi.fn();
    render(<SearchInput value="" onChange={onChange} />);

    const label = screen.getByLabelText("Search books by title or author");
    expect(label).toBeInTheDocument();
  });

  it("has correct input styling", () => {
    const onChange = vi.fn();
    render(<SearchInput value="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("Search by title or author...");
    expect(input).toHaveClass("pl-9");
  });

  it("handles clearing input", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<SearchInput value="test" onChange={onChange} />);

    const input = screen.getByDisplayValue("test");
    await user.clear(input);

    expect(onChange).toHaveBeenCalledWith("");
  });

  it("updates when value prop changes", () => {
    const onChange = vi.fn();
    const { rerender } = render(<SearchInput value="" onChange={onChange} />);

    expect(screen.getByDisplayValue("")).toBeInTheDocument();

    rerender(<SearchInput value="new value" onChange={onChange} />);

    expect(screen.getByDisplayValue("new value")).toBeInTheDocument();
  });
});
