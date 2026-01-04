import type { ImageResponse } from "next/og";
import { NextRequest } from "next/server";

import booksApiClient from "@workspace/api-client/catalog/books";

export const runtime = "edge";
export const size = {
  width: 1200,
  height: 630,
};
export const contentType = "image/png";

type OpengraphImageProps = {
  params: Promise<{ id: string }>;
};

export async function generateAlt({ params }: OpengraphImageProps) {
  const { id } = await params;
  const book = await booksApiClient.get(id);
  return book.name ?? "Book Details";
}

export default async function Image(
  _: NextRequest,
  { params }: OpengraphImageProps,
): Promise<ImageResponse> {
  const { ImageResponse } = await import("next/og");
  const { id } = await params;
  const book = await booksApiClient.get(id);

  const bookName = book.name ?? "Book";
  const authorNames = book.authors
    .map((a) => a.name)
    .filter((name): name is string => name !== null)
    .join(", ");
  const price = book.priceSale ?? book.price;

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
        {book.imageUrl ? (
          <img
            src={book.imageUrl}
            alt={bookName}
            width={300}
            height={450}
            style={{
              objectFit: "cover",
              borderRadius: 8,
              boxShadow: "0 10px 40px rgba(0,0,0,0.2)",
            }}
          />
        ) : (
          <div
            style={{
              width: 300,
              height: 450,
              background: "rgba(255, 255, 255, 0.7)",
              borderRadius: 8,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontSize: 100,
            }}
          >
            📖
          </div>
        )}
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
            color: "#2c1810",
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
              color: "#5c4033",
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
            <div
              style={{
                padding: "10px 20px",
                background: "rgba(255, 255, 255, 0.7)",
                borderRadius: 8,
                fontSize: 24,
                color: "#5c4033",
              }}
            >
              {book.category.name}
            </div>
          )}
          {book.publisher?.name && (
            <div
              style={{
                padding: "10px 20px",
                background: "rgba(255, 255, 255, 0.7)",
                borderRadius: 8,
                fontSize: 24,
                color: "#5c4033",
              }}
            >
              {book.publisher.name}
            </div>
          )}
        </div>

        {/* Price */}
        <div
          style={{
            fontSize: 48,
            fontWeight: "bold",
            color: "#2c1810",
            display: "flex",
            alignItems: "center",
            gap: 15,
          }}
        >
          <div>${price?.toFixed(2)}</div>
          {book.priceSale && book.priceSale < book.price && (
            <div
              style={{
                fontSize: 32,
                color: "#999",
                textDecoration: "line-through",
              }}
            >
              ${book.price.toFixed(2)}
            </div>
          )}
        </div>

        {/* Brand */}
        <div
          style={{
            fontSize: 28,
            color: "#5c4033",
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
