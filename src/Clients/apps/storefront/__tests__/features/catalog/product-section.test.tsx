import { describe, expect, it, vi } from "vitest";

import { renderWithProviders, screen } from "@/__tests__/utils/test-utils";
import ProductSection from "@/features/catalog/product/product-section";

// Mock child components to isolate unit tests
vi.mock("@/features/catalog/product/product-image", () => ({
  default: ({
    imageUrl,
    name,
    hasSale,
  }: {
    imageUrl?: string;
    name: string;
    hasSale?: boolean;
  }) => (
    <div data-testid="product-image" data-image-url={imageUrl} data-name={name}>
      {hasSale && <span data-testid="sale-badge">SALE</span>}
    </div>
  ),
}));

vi.mock("@/features/catalog/product/product-info", () => ({
  default: ({
    name,
    category,
    price,
    priceSale,
    status,
    description,
    publisher,
    authors,
    averageRating,
    totalReviews,
  }: {
    name: string;
    category?: string;
    price: number;
    priceSale?: number;
    status: string;
    description?: string;
    publisher?: string;
    authors: Array<{ name: string }>;
    averageRating: number;
    totalReviews: number;
  }) => (
    <div
      data-testid="product-info"
      data-name={name}
      data-category={category}
      data-price={price}
      data-price-sale={priceSale}
      data-status={status}
      data-description={description}
      data-publisher={publisher}
      data-authors={authors.map((a) => a.name).join(",")}
      data-average-rating={averageRating}
      data-total-reviews={totalReviews}
    />
  ),
}));

vi.mock("@/features/catalog/product/product-actions", () => ({
  default: ({
    quantity,
    status,
    isAddingToBasket,
  }: {
    quantity: number;
    status: string;
    isAddingToBasket: boolean;
  }) => (
    <div
      data-testid="product-actions"
      data-quantity={quantity}
      data-status={status}
      data-is-adding={isAddingToBasket}
    />
  ),
}));

function createBook(
  overrides: Partial<Parameters<typeof ProductSection>[0]["book"]> = {},
) {
  return {
    imageUrl: "https://example.com/image.jpg",
    name: "Test Book",
    priceSale: null,
    category: { name: "Fiction" },
    authors: [{ name: "Author One" }],
    averageRating: 4.5,
    totalReviews: 100,
    price: 19.99,
    status: "InStock",
    description: "A great book",
    publisher: { name: "Publisher Inc." },
    ...overrides,
  };
}

describe("ProductSection", () => {
  it("renders article with schema.org Book markup", () => {
    renderWithProviders(
      <ProductSection
        book={createBook()}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const article = screen.getByRole("article");
    expect(article).toBeInTheDocument();
    expect(article).toHaveAttribute("itemtype", "https://schema.org/Book");
  });

  it("passes image props correctly", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ imageUrl: "https://example.com/cover.jpg" })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const image = screen.getByTestId("product-image");
    expect(image).toHaveAttribute(
      "data-image-url",
      "https://example.com/cover.jpg",
    );
    expect(image).toHaveAttribute("data-name", "Test Book");
  });

  it("passes hasSale=true when priceSale is set", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ priceSale: 9.99 })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    expect(screen.getByTestId("sale-badge")).toBeInTheDocument();
  });

  it("passes hasSale=false when priceSale is null", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ priceSale: null })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    expect(screen.queryByTestId("sale-badge")).not.toBeInTheDocument();
  });

  it("passes info props with nullish coalescing", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({
          category: null,
          publisher: null,
          description: null,
          priceSale: null,
        })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const info = screen.getByTestId("product-info");
    expect(info).not.toHaveAttribute("data-category");
    expect(info).not.toHaveAttribute("data-publisher");
    expect(info).not.toHaveAttribute("data-description");
    expect(info).not.toHaveAttribute("data-price-sale");
  });

  it("maps author names correctly, replacing null with empty string", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({
          authors: [{ name: "Alice" }, { name: null }],
        })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const info = screen.getByTestId("product-info");
    expect(info).toHaveAttribute("data-authors", "Alice,");
  });

  it("passes action props correctly", () => {
    renderWithProviders(
      <ProductSection
        book={createBook()}
        quantity={3}
        isAddingToBasket={true}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const actions = screen.getByTestId("product-actions");
    expect(actions).toHaveAttribute("data-quantity", "3");
    expect(actions).toHaveAttribute("data-status", "InStock");
    expect(actions).toHaveAttribute("data-is-adding", "true");
  });

  it("passes imageUrl as undefined when null", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ imageUrl: null })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const image = screen.getByTestId("product-image");
    expect(image).not.toHaveAttribute("data-image-url");
  });

  it("passes category name when present", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ category: { name: "Science Fiction" } })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const info = screen.getByTestId("product-info");
    expect(info).toHaveAttribute("data-category", "Science Fiction");
  });

  it("handles category with null name", () => {
    renderWithProviders(
      <ProductSection
        book={createBook({ category: { name: null } })}
        quantity={1}
        isAddingToBasket={false}
        onAddToBasket={vi.fn()}
        onQuantityChange={vi.fn()}
        onDecrease={vi.fn()}
        onIncrease={vi.fn()}
      />,
    );

    const info = screen.getByTestId("product-info");
    expect(info).not.toHaveAttribute("data-category");
  });
});
