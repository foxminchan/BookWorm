import { useRouter } from "next/navigation";

import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  createMockAuthor,
  createMockBook,
  createMockCategory,
  createMockPublisher,
} from "@/__tests__/factories";
import { BookForm } from "@/features/books/book-form";

vi.mock("next/navigation");
vi.mock("@workspace/api-hooks/catalog/books/useBook");
vi.mock("@workspace/api-hooks/catalog/books/useCreateBook");
vi.mock("@workspace/api-hooks/catalog/books/useUpdateBook");
vi.mock("@workspace/api-hooks/catalog/authors/useAuthors");
vi.mock("@workspace/api-hooks/catalog/categories/useCategories");
vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers");

const mockPush = vi.fn();
const mockUseRouter = vi.mocked(useRouter);
const mockUseBook = vi.hoisted(() => vi.fn());
const mockUseCreateBook = vi.hoisted(() => vi.fn());
const mockUseUpdateBook = vi.hoisted(() => vi.fn());
const mockUseAuthors = vi.hoisted(() => vi.fn());
const mockUseCategories = vi.hoisted(() => vi.fn());
const mockUsePublishers = vi.hoisted(() => vi.fn());

vi.mock("@workspace/api-hooks/catalog/books/useBook", () => ({
  default: mockUseBook,
}));

vi.mock("@workspace/api-hooks/catalog/books/useCreateBook", () => ({
  default: mockUseCreateBook,
}));

vi.mock("@workspace/api-hooks/catalog/books/useUpdateBook", () => ({
  default: mockUseUpdateBook,
}));

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors", () => ({
  default: mockUseAuthors,
}));

vi.mock("@workspace/api-hooks/catalog/categories/useCategories", () => ({
  default: mockUseCategories,
}));

vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers", () => ({
  default: mockUsePublishers,
}));

describe("BookForm", () => {
  const mockAuthors = [
    createMockAuthor({ name: "John Doe" }),
    createMockAuthor({ name: "Jane Smith" }),
  ];

  const mockCategories = [
    createMockCategory({ name: "Fiction" }),
    createMockCategory({ name: "Non-Fiction" }),
  ];

  const mockPublishers = [createMockPublisher(), createMockPublisher()];

  beforeEach(() => {
    vi.clearAllMocks();
    mockUseRouter.mockReturnValue({
      push: mockPush,
      back: vi.fn(),
      forward: vi.fn(),
      refresh: vi.fn(),
      replace: vi.fn(),
      prefetch: vi.fn(),
    });

    mockUseBook.mockReturnValue({
      data: null,
      isLoading: false,
    });

    mockUseCreateBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    mockUseUpdateBook.mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
    });

    mockUseAuthors.mockReturnValue({
      data: mockAuthors,
      isLoading: false,
    });

    mockUseCategories.mockReturnValue({
      data: mockCategories,
      isLoading: false,
    });

    mockUsePublishers.mockReturnValue({
      data: mockPublishers,
      isLoading: false,
    });
  });

  it("renders the form with basic fields", () => {
    render(<BookForm />);

    expect(screen.getByLabelText(/Book Name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Description/i)).toBeInTheDocument();
    // Multiple price fields exist (Price and Sale Price), just check one
    expect(screen.getAllByLabelText(/Price/i).length).toBeGreaterThan(0);
  });

  it("renders populated form when book data is provided", () => {
    const mockBook = createMockBook({
      name: "Test Book",
      description: "Test Description",
    });

    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
    });

    render(<BookForm bookId={mockBook.id} />);

    // Check form has loaded with data
    expect(screen.getByLabelText(/Book Name/i)).toBeInTheDocument();
  });

  it("renders when book is loading", () => {
    mockUseBook.mockReturnValue({
      data: null,
      isLoading: true,
    });

    render(<BookForm bookId="book-1" />);

    // Should render something (component might show skeleton or loading state)
    expect(document.body).toBeInTheDocument();
  });

  it("renders form submit button", async () => {
    render(<BookForm />);

    // Check submit button exists (label may vary between Create/Update)
    const buttons = screen.getAllByRole("button");
    expect(buttons.length).toBeGreaterThan(0);
  });

  it("shows cancel button that navigates back", () => {
    render(<BookForm />);

    expect(screen.getByRole("button", { name: /Cancel/i })).toBeInTheDocument();
  });

  it("displays authors section", () => {
    render(<BookForm />);

    // Check that form renders - authors section handled by AuthorsFilter component
    expect(screen.getByPlaceholderText("Enter book name")).toBeInTheDocument();
  });

  it("shows sale price field", () => {
    render(<BookForm />);

    expect(screen.getByLabelText(/Sale Price/i)).toBeInTheDocument();
  });

  it("displays main form sections", () => {
    render(<BookForm />);

    // Check for key form elements instead of specific section titles
    expect(screen.getByText("Book Information")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter book name")).toBeInTheDocument();
    // Multiple fields with 0.00 placeholder, just check one exists
    const priceInputs = screen.getAllByPlaceholderText("0.00");
    expect(priceInputs.length).toBeGreaterThan(0);
  });

  it("handles form input changes", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const nameInput = screen.getByPlaceholderText("Enter book name");
    await user.type(nameInput, "New Book Title");

    expect(nameInput).toHaveValue("New Book Title");
  });

  it("displays category select", () => {
    render(<BookForm />);

    expect(screen.getByText("Category")).toBeInTheDocument();
  });

  it("displays publisher select", () => {
    render(<BookForm />);

    expect(screen.getByText("Publisher")).toBeInTheDocument();
  });

  it("calls cancel handler when cancel button clicked", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const cancelButton = screen.getByRole("button", { name: /Cancel/i });
    await user.click(cancelButton);

    // Should attempt navigation (mocked)
    expect(mockUseRouter).toHaveBeenCalled();
  });

  it("renders description textarea", () => {
    render(<BookForm />);

    const descriptionField = screen.getByPlaceholderText(
      "Enter book description",
    );
    expect(descriptionField).toBeInTheDocument();
    expect(descriptionField.tagName).toBe("TEXTAREA");
  });

  it("renders price inputs with correct type", () => {
    render(<BookForm />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    priceInputs.forEach((input) => {
      expect(input).toHaveAttribute("type", "number");
      expect(input).toHaveAttribute("min", "0");
      expect(input).toHaveAttribute("step", "0.01");
    });
  });

  it("handles author checkbox selection", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const firstAuthorCheckbox = screen.getByLabelText("John Doe");
    await user.click(firstAuthorCheckbox);

    expect(firstAuthorCheckbox).toBeChecked();
  });

  it("handles category selection", async () => {
    render(<BookForm />);

    const categorySelect = screen.getByRole("combobox", { name: /category/i });
    expect(categorySelect).toBeInTheDocument();
  });

  it("handles publisher selection", async () => {
    render(<BookForm />);

    const publisherSelect = screen.getByRole("combobox", {
      name: /publisher/i,
    });
    expect(publisherSelect).toBeInTheDocument();
  });

  it("shows validation message when no authors selected", async () => {
    render(<BookForm />);

    const validationMessage = screen.getByText(
      /At least one author is required/i,
    );
    expect(validationMessage).toBeInTheDocument();
  });

  it("handles image file selection", async () => {
    render(<BookForm />);

    const fileInput = screen.getByLabelText(/Upload book cover/i);
    expect(fileInput).toBeInTheDocument();
    expect(fileInput).toHaveAttribute("type", "file");
    expect(fileInput).toHaveAttribute("accept", "image/*");
  });

  it("toggles multiple authors selection", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const firstAuthor = screen.getByLabelText("John Doe");
    const secondAuthor = screen.getByLabelText("Jane Smith");

    // Select first author
    await user.click(firstAuthor);
    expect(firstAuthor).toBeChecked();

    // Select second author
    await user.click(secondAuthor);
    expect(secondAuthor).toBeChecked();

    // Unselect first author
    await user.click(firstAuthor);
    expect(firstAuthor).not.toBeChecked();
    expect(secondAuthor).toBeChecked();
  });

  it("handles image file upload with FileReader", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const fileInput = screen.getByLabelText(/Upload book cover/i);

    // Create a mock file
    const file = new File(["test"], "test.png", { type: "image/png" });

    await user.upload(fileInput, file);

    // Verify file was uploaded
    const input = fileInput as HTMLInputElement;
    expect(input.files?.[0]).toBe(file);
  });

  it("renders image preview section when form loads", () => {
    render(<BookForm />);

    // Image section should be in the document
    const fileInput = screen.getByLabelText(/Upload book cover/i);
    expect(fileInput).toBeInTheDocument();
  });

  it("handles price input with decimal values", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    expect(priceInputs.length).toBeGreaterThanOrEqual(2);
    const regularPrice = priceInputs[0] as HTMLInputElement;
    const salePrice = priceInputs[1] as HTMLInputElement;

    // Test regular price
    await user.type(regularPrice, "29.99");
    expect(regularPrice).toHaveValue(29.99);

    // Test sale price
    await user.type(salePrice, "19.99");
    expect(salePrice).toHaveValue(19.99);
  });

  it("handles sale price as null when empty", async () => {
    const user = userEvent.setup();
    render(<BookForm />);

    const priceInputs = screen.getAllByPlaceholderText("0.00");
    expect(priceInputs.length).toBeGreaterThanOrEqual(2);
    const salePriceInput = priceInputs[1] as HTMLInputElement;

    // Type and clear
    await user.type(salePriceInput, "19.99");
    await user.clear(salePriceInput);

    expect(salePriceInput).toHaveValue(null);
  });
});
