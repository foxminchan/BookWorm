import { ImageResponse } from "next/og";

export const runtime = "edge";
export const alt = "BookWorm - Curated Books for the Modern Reader";
export const size = {
  width: 1200,
  height: 630,
};
export const contentType = "image/png";

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
      {/* Logo/Brand */}
      <div
        style={{
          fontSize: 80,
          fontWeight: "bold",
          color: "#6d5a54",
          marginBottom: "30px",
          display: "flex",
        }}
      >
        BookWorm
      </div>

      {/* Tagline */}
      <div
        style={{
          fontSize: 36,
          color: "#6d5a54",
          textAlign: "center",
          maxWidth: "900px",
          lineHeight: 1.4,
          display: "flex",
        }}
      >
        Curated books for the modern reader
      </div>

      {/* Decorative element */}
      <div
        style={{
          marginTop: "50px",
          display: "flex",
          gap: "20px",
        }}
      >
        <div
          style={{
            width: "80px",
            height: "100px",
            background: "#9fb89f",
            borderRadius: "4px",
            display: "flex",
          }}
        />
        <div
          style={{
            width: "80px",
            height: "100px",
            background: "#6d5a54",
            borderRadius: "4px",
            display: "flex",
          }}
        />
        <div
          style={{
            width: "80px",
            height: "100px",
            background: "#e8dcc8",
            borderRadius: "4px",
            display: "flex",
          }}
        />
      </div>
    </div>,
    { ...size },
  );
}
