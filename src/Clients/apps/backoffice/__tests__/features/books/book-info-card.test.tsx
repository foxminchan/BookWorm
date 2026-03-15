import { zodResolver } from "@hookform/resolvers/zod";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useForm } from "react-hook-form";
import { describe, expect, it } from "vitest";

import { Form } from "@workspace/ui/components/form";
import {
  type CreateBookInput,
  createBookSchema,
} from "@workspace/validations/catalog/books";

import { BookInfoCard } from "@/features/books/book-info-card";

function BookInfoCardWrapper() {
  const form = useForm<CreateBookInput>({
    resolver: zodResolver(createBookSchema),
    defaultValues: {
      name: "",
      description: "",
      price: undefined,
      priceSale: null,
      categoryId: "",
      publisherId: "",
      authorIds: [],
    },
  });

  return (
    <Form {...form}>
      <BookInfoCard form={form} />
    </Form>
  );
}

describe("BookInfoCard", () => {
  it("renders the card title and description", () => {
    render(<BookInfoCardWrapper />);

    expect(screen.getByText("Book Information")).toBeInTheDocument();
    expect(
      screen.getByText("Enter the basic details about the book"),
    ).toBeInTheDocument();
  });

  it("renders the book name input field", () => {
    render(<BookInfoCardWrapper />);

    const nameInput = screen.getByPlaceholderText("Enter book name");
    expect(nameInput).toBeInTheDocument();
    expect(nameInput.tagName).toBe("INPUT");
  });

  it("renders the description textarea", () => {
    render(<BookInfoCardWrapper />);

    const descriptionField = screen.getByPlaceholderText(
      "Enter book description",
    );
    expect(descriptionField).toBeInTheDocument();
    expect(descriptionField.tagName).toBe("TEXTAREA");
  });

  it("renders price and sale price inputs with number type", () => {
    render(<BookInfoCardWrapper />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    expect(priceInputs).toHaveLength(2);

    for (const input of priceInputs) {
      expect(input).toHaveAttribute("type", "number");
      expect(input).toHaveAttribute("step", "0.01");
      expect(input).toHaveAttribute("min", "0");
    }
  });

  it("renders the sale price label as optional", () => {
    render(<BookInfoCardWrapper />);

    expect(screen.getByText("Sale Price (Optional)")).toBeInTheDocument();
  });

  it("handles book name input changes", async () => {
    const user = userEvent.setup();
    render(<BookInfoCardWrapper />);

    const nameInput = screen.getByPlaceholderText("Enter book name");
    await user.type(nameInput, "Test Book");

    expect(nameInput).toHaveValue("Test Book");
  });

  it("handles description input changes", async () => {
    const user = userEvent.setup();
    render(<BookInfoCardWrapper />);

    const descriptionField = screen.getByPlaceholderText(
      "Enter book description",
    );
    await user.type(descriptionField, "A test description");

    expect(descriptionField).toHaveValue("A test description");
  });

  it("handles price input with decimal values", async () => {
    const user = userEvent.setup();
    render(<BookInfoCardWrapper />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    const priceInput = priceInputs[0] as HTMLInputElement;

    await user.type(priceInput, "29.99");

    expect(priceInput).toHaveValue(29.99);
  });

  it("handles sale price input clearing to null", async () => {
    const user = userEvent.setup();
    render(<BookInfoCardWrapper />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    const salePriceInput = priceInputs[1] as HTMLInputElement;

    await user.type(salePriceInput, "19.99");
    await user.clear(salePriceInput);

    expect(salePriceInput).toHaveValue(null);
  });

  it("renders form labels for all fields", () => {
    render(<BookInfoCardWrapper />);

    expect(screen.getByText("Book Name")).toBeInTheDocument();
    expect(screen.getByText("Description")).toBeInTheDocument();
    expect(screen.getByText("Price")).toBeInTheDocument();
    expect(screen.getByText("Sale Price (Optional)")).toBeInTheDocument();
  });
});
