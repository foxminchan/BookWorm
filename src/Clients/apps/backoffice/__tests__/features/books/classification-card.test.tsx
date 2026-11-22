import { Form, useForm } from "@formisch/react";
import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { createBookSchema } from "@workspace/validations/catalog/books";

import {
  createMockAuthor,
  createMockCategory,
  createMockPublisher,
} from "@/__tests__/factories";
import { ClassificationCard } from "@/features/books/classification-card";

const mockCategories = [
  createMockCategory({ name: "Fiction" }),
  createMockCategory({ name: "Non-Fiction" }),
];

const mockPublishers = [
  createMockPublisher({ name: "Publisher A" }),
  createMockPublisher({ name: "Publisher B" }),
];

const mockAuthors = [
  createMockAuthor({ name: "Author One" }),
  createMockAuthor({ name: "Author Two" }),
  createMockAuthor({ name: "Author Three" }),
];

type WrapperProps = {
  initialAuthors?: string[];
  isLoading?: boolean;
};

function ClassificationCardWrapper({
  initialAuthors = [],
  isLoading = false,
}: WrapperProps) {
  const form = useForm({
    schema: createBookSchema,
    initialInput: {
      name: "",
      description: "",
      price: undefined,
      priceSale: null,
      categoryId: "",
      publisherId: "",
      authorIds: initialAuthors,
    },
  });

  return (
    <Form of={form} onSubmit={vi.fn()}>
      <ClassificationCard
        form={form}
        categories={mockCategories}
        publishers={mockPublishers}
        authors={mockAuthors}
        isLoading={isLoading}
      />
    </Form>
  );
}

describe("ClassificationCard", () => {
  it("renders the card title and description", () => {
    render(<ClassificationCardWrapper />);

    expect(screen.getByText("Classification")).toBeInTheDocument();
    expect(
      screen.getByText("Select category, publisher, and authors"),
    ).toBeInTheDocument();
  });

  it("renders category select with options", () => {
    render(<ClassificationCardWrapper />);

    expect(screen.getByText("Category")).toBeInTheDocument();
    expect(
      screen.getByRole("combobox", { name: /category/i }),
    ).toBeInTheDocument();
  });

  it("renders publisher select with options", () => {
    render(<ClassificationCardWrapper />);

    expect(screen.getByText("Publisher")).toBeInTheDocument();
    expect(
      screen.getByRole("combobox", { name: /publisher/i }),
    ).toBeInTheDocument();
  });

  it("renders authors checkboxes", () => {
    render(<ClassificationCardWrapper />);

    expect(screen.getByText("Authors")).toBeInTheDocument();
    expect(screen.getByLabelText("Author One")).toBeInTheDocument();
    expect(screen.getByLabelText("Author Two")).toBeInTheDocument();
    expect(screen.getByLabelText("Author Three")).toBeInTheDocument();
  });

  it("shows validation message when no authors selected", async () => {
    const { container } = render(
      <ClassificationCardWrapper initialAuthors={[]} />,
    );

    fireEvent.submit(container.querySelector("form")!);

    expect(
      await screen.findByText("At least one author is required"),
    ).toBeInTheDocument();
  });

  it("hides validation message when authors are selected", () => {
    render(<ClassificationCardWrapper initialAuthors={[mockAuthors[0]!.id]} />);

    expect(
      screen.queryByText("At least one author is required"),
    ).not.toBeInTheDocument();
  });

  it("marks selected author checkboxes as checked", () => {
    render(
      <ClassificationCardWrapper
        initialAuthors={[mockAuthors[0]!.id, mockAuthors[2]!.id]}
      />,
    );

    expect(screen.getByLabelText("Author One")).toBeChecked();
    expect(screen.getByLabelText("Author Two")).not.toBeChecked();
    expect(screen.getByLabelText("Author Three")).toBeChecked();
  });

  it("toggles author selection in form state when clicking an author checkbox", async () => {
    const user = userEvent.setup();
    render(<ClassificationCardWrapper />);

    const authorOneCheckbox = screen.getByLabelText("Author One");
    expect(authorOneCheckbox).not.toBeChecked();

    await user.click(authorOneCheckbox);

    expect(authorOneCheckbox).toBeChecked();
    expect(
      screen.queryByText("At least one author is required"),
    ).not.toBeInTheDocument();
  });

  it("renders skeleton when loading", () => {
    render(<ClassificationCardWrapper isLoading={true} />);

    expect(screen.getByText("Classification")).toBeInTheDocument();
    expect(screen.queryByText("Category")).not.toBeInTheDocument();
    expect(screen.queryByText("Publisher")).not.toBeInTheDocument();
    expect(screen.queryByText("Authors")).not.toBeInTheDocument();
  });

  it("renders form fields when not loading", () => {
    render(<ClassificationCardWrapper isLoading={false} />);

    expect(screen.getByText("Category")).toBeInTheDocument();
    expect(screen.getByText("Publisher")).toBeInTheDocument();
    expect(screen.getByText("Authors")).toBeInTheDocument();
  });
});
