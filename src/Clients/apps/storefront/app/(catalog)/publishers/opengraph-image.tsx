import { ImageResponse } from "next/og"

export const runtime = "edge"
export const alt = "BookWorm - Browse by Publisher"
export const size = {
  width: 1200,
  height: 630,
}
export const contentType = "image/png"

export default async function Image() {
  return new ImageResponse(
    <div
      style={{
        background: "linear-gradient(135deg, #fbf8f1 0%, #f5ede0 100%)",
        width: "100%",
        height: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: "serif",
        padding: "60px",
      }}
    >
      <div
        style={{
          fontSize: 32,
          color: "#6d5a54",
          marginBottom: "20px",
          display: "flex",
        }}
      >
        BookWorm
      </div>
      <div
        style={{
          fontSize: 64,
          fontWeight: "bold",
          color: "#2d2622",
          display: "flex",
        }}
      >
        Browse by Publisher
      </div>
      <div
        style={{
          fontSize: 28,
          color: "#6d5a54",
          marginTop: "20px",
          display: "flex",
        }}
      >
        Penguin • HarperCollins • Simon & Schuster & More
      </div>
    </div>,
    { ...size },
  )
}
