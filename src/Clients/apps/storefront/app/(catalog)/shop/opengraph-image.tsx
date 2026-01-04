import { ImageResponse } from "next/og";

export const runtime = "edge";
export const alt = "Browse Books at BookWorm";
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
          color: "#2c1810",
          marginBottom: 20,
          display: "flex",
          alignItems: "center",
        }}
      >
        📚 BookWorm
      </div>

      {/* Subtitle */}
      <div
        style={{
          fontSize: 40,
          color: "#5c4033",
          marginBottom: 40,
          textAlign: "center",
          maxWidth: "80%",
        }}
      >
        Browse Our Curated Collection
      </div>

      {/* Feature Grid */}
      <div
        style={{
          display: "flex",
          gap: 30,
          marginTop: 20,
        }}
      >
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 20,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 12,
          }}
        >
          <div style={{ fontSize: 36 }}>📖</div>
          <div style={{ fontSize: 20, color: "#5c4033", marginTop: 8 }}>
            Fiction
          </div>
        </div>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 20,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 12,
          }}
        >
          <div style={{ fontSize: 36 }}>🔬</div>
          <div style={{ fontSize: 20, color: "#5c4033", marginTop: 8 }}>
            Science
          </div>
        </div>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 20,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 12,
          }}
        >
          <div style={{ fontSize: 36 }}>💼</div>
          <div style={{ fontSize: 20, color: "#5c4033", marginTop: 8 }}>
            Business
          </div>
        </div>
      </div>
    </div>,
    {
      ...size,
    },
  );
}
