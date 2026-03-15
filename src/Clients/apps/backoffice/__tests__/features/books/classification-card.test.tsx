import { zodResolver } from "@hookform/resolvers/zod";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useForm } from "react-hook-form";
import { describe, expect, it, vi } from "vitest";

import { Form } from "@workspace/ui/components/form";
import {
  type CreateBookInput,
  createBookSchema,
} from "@workspace/validations/catalog/books";

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
  selectedAuthors?: string[];
  isLoading?: boolean;
  onToggleAuthor?: (authorId: string) => void;
};

function ClassificationCardWrapper({
  selectedAuthors = [],
  isLoading = false,
  onToggleAuthor = vi.fn(),
}: WrapperProps) {
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
      <ClassificationCard
        form={form}
        categories={mockCategories}
        publishers={mockPublishers}
        authors={mockAuthors}
        selectedAuthors={selectedAuthors}
        isLoading={isLoading}
        onToggleAuthor={onToggleAuthor}
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

  it("shows validation message when no authors selected", () => {
    render(<ClassificationCardWrapper selectedAuthors={[]} />);

    expect(
      screen.getByText("At least one author is required"),
    ).toBeInTheDocument();
  });

  it("hides validation message when authors are selected", () => {
    render(
      <ClassificationCardWrapper selectedAuthors={[mockAuthors[0]!.id]} />,
    );

    expect(
      screen.queryByText("At least one author is required"),
    ).not.toBeInTheDocument();
  });

  it("marks selected author checkboxes as checked", () => {
    render(
      <ClassificationCardWrapper
        selectedAuthors={[mockAuthors[0]!.id, mockAuthors[2]!.id]}
      />,
    );

    expect(screen.getByLabelText("Author One")).toBeChecked();
    expect(screen.getByLabelText("Author Two")).not.toBeChecked();
    expect(screen.getByLabelText("Author Three")).toBeChecked();
  });

  it("calls onToggleAuthor when clicking an author checkbox", async () => {
    const onToggleAuthor = vi.fn();
    const user = userEvent.setup();
    render(<ClassificationCardWrapper onToggleAuthor={onToggleAuthor} />);

    await user.click(screen.getByLabelText("Author One"));

    expect(onToggleAuthor).toHaveBeenCalledWith(mockAuthors[0]!.id);
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
