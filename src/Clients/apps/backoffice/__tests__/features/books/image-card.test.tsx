import React from "react";

import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

// Mock next/image to avoid URL validation issues in test environment
vi.mock("next/image", () => ({
  __esModule: true,
  default: (props: Record<string, unknown>) => {
    const { src, alt, ...rest } = props;
    const resolvedSrc = typeof src === "string" ? src : "";
    return React.createElement("img", {
      src: resolvedSrc,
      alt: (alt as string) ?? "",
      ...rest,
    });
  },
}));

import { ImageCard } from "@/features/books/image-card";

describe("ImageCard", () => {
  const defaultProps = {
    imagePreview: null,
    onImageChange: vi.fn(),
    onImageRemove: vi.fn(),
    onImageError: vi.fn(),
  };

  it("renders the card title and description", () => {
    render(<ImageCard {...defaultProps} />);

    expect(screen.getByText("Image")).toBeInTheDocument();
    expect(
      screen.getByText("Upload a book cover image (optional)"),
    ).toBeInTheDocument();
  });

  it("renders file upload input when no image preview", () => {
    render(<ImageCard {...defaultProps} imagePreview={null} />);

    const fileInput = screen.getByLabelText(/Upload book cover/i);
    expect(fileInput).toBeInTheDocument();
    expect(fileInput).toHaveAttribute("type", "file");
    expect(fileInput).toHaveAttribute("accept", "image/*");
  });

  it("renders image preview when imagePreview is set", () => {
    render(
      <ImageCard {...defaultProps} imagePreview="https://example.com/book.jpg" />,
    );

    const image = screen.getByRole("img");
    expect(image).toBeInTheDocument();
    expect(image.getAttribute("src")).toContain("example.com");
    expect(image).toHaveAttribute("alt", "Book cover preview");
  });

  it("renders image with book name in alt text when provided", () => {
    render(
      <ImageCard
        {...defaultProps}
        imagePreview="https://example.com/book.jpg"
        bookName="The Great Gatsby"
      />,
    );

    const image = screen.getByRole("img");
    expect(image).toHaveAttribute(
      "alt",
      "Cover image for The Great Gatsby",
    );
  });

  it("renders change image input when preview exists", () => {
    render(
      <ImageCard {...defaultProps} imagePreview="https://example.com/book.jpg" />,
    );

    const fileInput = screen.getByLabelText(/Change book cover/i);
    expect(fileInput).toBeInTheDocument();
    expect(fileInput).toHaveAttribute("type", "file");
    expect(fileInput).toHaveAttribute("accept", "image/*");
  });

  it("renders remove button when bookId is provided with preview", () => {
    render(
      <ImageCard
        {...defaultProps}
        bookId="book-123"
        imagePreview="https://example.com/book.jpg"
      />,
    );

    const removeButton = screen.getByRole("button", {
      name: /Remove current book cover/i,
    });
    expect(removeButton).toBeInTheDocument();
  });

  it("does not render remove button when no bookId", () => {
    render(
      <ImageCard {...defaultProps} imagePreview="https://example.com/book.jpg" />,
    );

    expect(
      screen.queryByRole("button", { name: /Remove/i }),
    ).not.toBeInTheDocument();
  });

  it("calls onImageRemove when remove button is clicked", async () => {
    const onImageRemove = vi.fn();
    const user = userEvent.setup();

    render(
      <ImageCard
        {...defaultProps}
        bookId="book-123"
        imagePreview="https://example.com/book.jpg"
        onImageRemove={onImageRemove}
      />,
    );

    await user.click(
      screen.getByRole("button", { name: /Remove current book cover/i }),
    );
    expect(onImageRemove).toHaveBeenCalledOnce();
  });

  it("calls onImageChange when file is selected", async () => {
    const onImageChange = vi.fn();
    const user = userEvent.setup();

    render(<ImageCard {...defaultProps} onImageChange={onImageChange} />);

    const fileInput = screen.getByLabelText(/Upload book cover/i);
    const file = new File(["test"], "test.png", { type: "image/png" });
    await user.upload(fileInput, file);

    expect(onImageChange).toHaveBeenCalled();
  });

  it("calls onImageError when image fails to load", () => {
    const onImageError = vi.fn();

    render(
      <ImageCard
        {...defaultProps}
        imagePreview="https://example.com/broken.jpg"
        onImageError={onImageError}
      />,
    );

    const image = screen.getByRole("img");
    image.dispatchEvent(new Event("error"));

    expect(onImageError).toHaveBeenCalled();
  });
});
