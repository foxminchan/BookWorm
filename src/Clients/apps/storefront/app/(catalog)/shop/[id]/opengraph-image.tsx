import type { ImageResponse } from "next/og";

import booksApiClient from "@workspace/api-client/catalog/books";

import { DEFAULT_BOOK_IMAGE, currencyFormatter } from "@/lib/constants";

export const runtime = "edge";
export const size = {
  width: 1200,
  height: 630,
};
export const contentType = "image/png";

type OpengraphImageProps = {
  params: Promise<{ id: string }>;
};

const colors = {
  primary: "#2c1810",
  secondary: "#5c4033",
  muted: "#999",
  badgeBg: "rgba(255, 255, 255, 0.7)",
} as const;

const badgeStyle = {
  padding: "10px 20px",
  background: colors.badgeBg,
  borderRadius: 8,
  fontSize: 24,
  color: colors.secondary,
} as const;

function formatAuthorNames(authors: { name: string | null }[]): string {
  return authors
    .map((a) => a.name)
    .filter((name): name is string => name !== null)
    .join(", ");
}

export async function generateAlt({ params }: OpengraphImageProps) {
  const { id } = await params;
  const book = await booksApiClient.get(id);
  return book.name ?? "Book Details";
}

export default async function Image(
  _request: Request,
  { params }: OpengraphImageProps,
): Promise<ImageResponse> {
  const { ImageResponse } = await import("next/og");
  const { id } = await params;
  const book = await booksApiClient.get(id);

  const bookName = book.name ?? "Book";
  const authorNames = formatAuthorNames(book.authors);
  const price = book.priceSale ?? book.price;
  const hasSaleDiscount =
    book.priceSale !== null &&
    book.priceSale !== undefined &&
    book.priceSale < book.price;

  return new ImageResponse(
    <div
      style={{
        background: "linear-gradient(135deg, #fbf8f1 0%, #f5ede0 100%)",
        width: "100%",
        height: "100%",
        display: "flex",
        padding: "60px",
        fontFamily: "serif",
      }}
    >
      {/* Book Cover Area */}
      <div
        style={{
          width: "40%",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <img
          src={book.imageUrl ?? DEFAULT_BOOK_IMAGE}
          alt={bookName}
          width={300}
          height={450}
          style={{
            objectFit: "cover",
            borderRadius: 8,
            boxShadow: "0 10px 40px rgba(0,0,0,0.2)",
          }}
        />
      </div>

      {/* Book Details */}
      <div
        style={{
          width: "60%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          paddingLeft: 40,
        }}
      >
        {/* Title */}
        <div
          style={{
            fontSize: 56,
            fontWeight: "bold",
            color: colors.primary,
            marginBottom: 20,
            lineHeight: 1.2,
            display: "-webkit-box",
            overflow: "hidden",
            textOverflow: "ellipsis",
          }}
        >
          {bookName}
        </div>

        {/* Author */}
        {authorNames && (
          <div
            style={{
              fontSize: 32,
              color: colors.secondary,
              marginBottom: 30,
            }}
          >
            by {authorNames}
          </div>
        )}

        {/* Category & Publisher */}
        <div
          style={{
            display: "flex",
            gap: 20,
            marginBottom: 30,
          }}
        >
          {book.category?.name && (
            <div style={badgeStyle}>{book.category.name}</div>
          )}
          {book.publisher?.name && (
            <div style={badgeStyle}>{book.publisher.name}</div>
          )}
        </div>

        {/* Price */}
        <div
          style={{
            fontSize: 48,
            fontWeight: "bold",
            color: colors.primary,
            display: "flex",
            alignItems: "center",
            gap: 15,
          }}
        >
          <div>{price ? currencyFormatter.format(price) : "N/A"}</div>
          {hasSaleDiscount && (
            <div
              style={{
                fontSize: 32,
                color: colors.muted,
                textDecoration: "line-through",
              }}
            >
              {currencyFormatter.format(book.price)}
            </div>
          )}
        </div>

        {/* Brand */}
        <div
          style={{
            fontSize: 28,
            color: colors.secondary,
            marginTop: 40,
            display: "flex",
            alignItems: "center",
            gap: 10,
          }}
        >
          <div>📚</div>
          <div>BookWorm</div>
        </div>
      </div>
    </div>,
    {
      ...size,
    },
  );
}
