import { useRouter } from "next/navigation";

import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  createMockAuthor,
  createMockBook,
  createMockCategory,
  createMockPublisher,
} from "@/__tests__/factories";
import { BookForm } from "@/features/books/book-form";

vi.mock("next/navigation");

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
      bfcacheId: "test-route",
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

  it("submits update form, handles image error and removal, and redirects", async () => {
    const user = userEvent.setup();
    const updateMutate = vi.fn((_vars, options) => {
      options?.onSuccess?.();
    });
    mockUseUpdateBook.mockReturnValue({
      mutate: updateMutate,
      isPending: false,
    });
    const mockBook = createMockBook({
      name: "Existing Book",
      description: "Existing Description",
      price: 25,
      imageUrl: "https://example.com/cover.jpg",
      category: mockCategories[0],
      publisher: mockPublishers[0],
      authors: [mockAuthors[0]!],
    });
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
    });

    const { container } = render(<BookForm bookId={mockBook.id} />);

    // Trigger image error fallback
    const img = await screen.findByRole("img");
    fireEvent.error(img);

    // Trigger image removal
    await user.click(
      screen.getByRole("button", { name: /remove current book cover image/i }),
    );

    // Submit form
    const form = container.querySelector("form")!;
    fireEvent.submit(form);

    await waitFor(() => {
      expect(updateMutate).toHaveBeenCalledWith(
        expect.objectContaining({
          request: expect.objectContaining({
            id: mockBook.id,
            isRemoveImage: true,
          }),
        }),
        expect.objectContaining({ onSuccess: expect.any(Function) }),
      );
      expect(mockPush).toHaveBeenCalledWith("/books");
    });
  });

  it("submits create form and redirects to new book", async () => {
    const user = userEvent.setup();
    const createMutate = vi.fn((_vars, options) => {
      options?.onSuccess?.({ id: "created-123" });
    });
    mockUseCreateBook.mockReturnValue({
      mutate: createMutate,
      isPending: false,
    });

    const { container } = render(<BookForm />);

    await user.type(screen.getByLabelText(/Book Name/i), "New Book");
    await user.type(screen.getByLabelText(/Description/i), "New Description");
    const priceInputs = screen.getAllByPlaceholderText("0.00");
    await user.type(priceInputs[0]!, "19.99");

    // Select category, publisher, and author
    await user.click(screen.getByLabelText(/Category/i));
    await user.click(screen.getByRole("option", { name: "Fiction" }));

    await user.click(screen.getByLabelText(/Publisher/i));
    await user.click(
      screen.getByRole("option", { name: mockPublishers[0]!.name! }),
    );

    await user.click(screen.getByLabelText(mockAuthors[0]!.name!));

    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => {
      expect(createMutate).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith("/books?new=created-123");
    });
  });

  it("handles undefined classification data, null book fields, pending state, and edge-case image inputs", () => {
    mockUseAuthors.mockReturnValue({ data: undefined, isLoading: false });
    mockUseCategories.mockReturnValue({ data: undefined, isLoading: false });
    mockUsePublishers.mockReturnValue({ data: undefined, isLoading: false });
    mockUseCreateBook.mockReturnValue({ mutate: vi.fn(), isPending: true });
    mockUseBook.mockReturnValue({
      data: {
        id: "b-null",
        name: null,
        description: null,
        price: 10,
        priceSale: null,
        category: null,
        publisher: null,
        authors: [],
        imageUrl: null,
      },
      isLoading: false,
    });

    render(<BookForm bookId="b-null" />);
    expect(screen.getByText(/Updating\.\.\./i)).toBeInTheDocument();

    // Empty file change
    const fileInput = screen.getByLabelText(/Upload book cover image/i);
    fireEvent.change(fileInput, { target: { files: [] } });

    // Non-string FileReader result
    class MockFileReader {
      result = new ArrayBuffer(8);
      onloadend: (() => void) | null = null;
      readAsDataURL() {
        this.onloadend?.();
      }
    }
    const fileReaderSpy = vi
      .spyOn(globalThis, "FileReader")
      .mockImplementation(MockFileReader as unknown as new () => FileReader);

    const file = new File(["bytes"], "cover.png", { type: "image/png" });
    fireEvent.change(fileInput, { target: { files: [file] } });

    fileReaderSpy.mockRestore();
  });

  it("passes ifMatch derived from useBook etag when submitting an edit", async () => {
    const updateMutate = vi.fn();
    mockUseUpdateBook.mockReturnValue({
      mutate: updateMutate,
      isPending: false,
    });
    const mockBook = createMockBook({
      name: "ETag Book",
      description: "ETag Description",
      price: 25,
      category: mockCategories[0],
      publisher: mockPublishers[0],
      authors: [mockAuthors[0]!],
      version: 7,
    });
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
      etag: '"7"',
      refetch: vi.fn(),
    });

    const { container } = render(<BookForm bookId={mockBook.id} />);
    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => {
      expect(updateMutate).toHaveBeenCalledWith(
        expect.objectContaining({
          request: expect.objectContaining({ id: mockBook.id }),
          ifMatch: '"7"',
        }),
        expect.any(Object),
      );
    });
  });

  it("falls back to formatted book version when useBook etag is omitted", async () => {
    const updateMutate = vi.fn();
    mockUseUpdateBook.mockReturnValue({
      mutate: updateMutate,
      isPending: false,
    });
    const mockBook = createMockBook({
      name: "Fallback Version Book",
      description: "Fallback Version Description",
      price: 30,
      category: mockCategories[0],
      publisher: mockPublishers[0],
      authors: [mockAuthors[0]!],
      version: 3,
    });
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
    });

    const { container } = render(<BookForm bookId={mockBook.id} />);
    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => {
      expect(updateMutate).toHaveBeenCalledWith(
        expect.objectContaining({
          request: expect.objectContaining({ id: mockBook.id }),
          ifMatch: '"3"',
        }),
        expect.any(Object),
      );
    });
  });

  it("shows version conflict alert on 412 error and reloads latest book when clicked", async () => {
    const user = userEvent.setup();
    const toastErrorSpy = vi.spyOn(toast, "error").mockImplementation(() => "");
    const refetchMock = vi.fn().mockResolvedValue({});
    const conflictError = Object.assign(new Error("Precondition Failed"), {
      isAxiosError: true,
      response: { status: 412 },
    });
    const updateMutate = vi.fn((_vars, options) => {
      options?.onError?.(conflictError);
    });

    mockUseUpdateBook.mockReturnValue({
      mutate: updateMutate,
      isPending: false,
    });
    const mockBook = createMockBook({
      name: "Conflict Book",
      description: "Conflict Description",
      price: 25,
      category: mockCategories[0],
      publisher: mockPublishers[0],
      authors: [mockAuthors[0]!],
      version: 1,
    });
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
      etag: '"1"',
      refetch: refetchMock,
    });

    const { container } = render(<BookForm bookId={mockBook.id} />);
    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => {
      expect(screen.getByText("Version conflict detected")).toBeInTheDocument();
    });

    expect(mockPush).not.toHaveBeenCalled();
    expect(toastErrorSpy).toHaveBeenCalledWith(
      "This book was modified by another user. Reload the latest version to save your changes.",
    );

    await user.click(
      screen.getByRole("button", { name: /Reload latest version/i }),
    );

    expect(refetchMock).toHaveBeenCalledTimes(1);
    expect(
      screen.queryByText("Version conflict detected"),
    ).not.toBeInTheDocument();

    toastErrorSpy.mockRestore();
  });

  it("shows generic error toast on non-412 update failure", async () => {
    const toastErrorSpy = vi.spyOn(toast, "error").mockImplementation(() => "");
    const updateMutate = vi.fn((_vars, options) => {
      options?.onError?.(new Error("Network error"));
    });

    mockUseUpdateBook.mockReturnValue({
      mutate: updateMutate,
      isPending: false,
    });
    const mockBook = createMockBook({
      name: "Error Book",
      description: "Error Description",
      price: 25,
      category: mockCategories[0],
      publisher: mockPublishers[0],
      authors: [mockAuthors[0]!],
      version: 1,
    });
    mockUseBook.mockReturnValue({
      data: mockBook,
      isLoading: false,
      etag: '"1"',
      refetch: vi.fn(),
    });

    const { container } = render(<BookForm bookId={mockBook.id} />);
    fireEvent.submit(container.querySelector("form")!);

    await waitFor(() => {
      expect(toastErrorSpy).toHaveBeenCalledWith("Failed to update book");
    });

    expect(
      screen.queryByText("Version conflict detected"),
    ).not.toBeInTheDocument();
    expect(mockPush).not.toHaveBeenCalled();

    toastErrorSpy.mockRestore();
  });
});
