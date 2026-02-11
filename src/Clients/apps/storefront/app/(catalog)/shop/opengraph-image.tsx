import { ImageResponse } from "next/og";

export const runtime = "edge";
export const alt = "Browse Books at BookWorm";
export const size = {
  width: 1200,
  height: 630,
};
export const contentType = "image/png";

const colors = {
  primary: "#2c1810",
  secondary: "#5c4033",
} as const;

const categories = [
  { emoji: "📖", label: "Fiction" },
  { emoji: "🔬", label: "Science" },
  { emoji: "💼", label: "Business" },
] as const;

const categoryCardStyle = {
  display: "flex" as const,
  flexDirection: "column" as const,
  alignItems: "center" as const,
  padding: 20,
  background: "rgba(255, 255, 255, 0.7)",
  borderRadius: 12,
};

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
          color: colors.primary,
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
          color: colors.secondary,
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
        {categories.map((cat) => (
          <div key={cat.label} style={categoryCardStyle}>
            <div style={{ fontSize: 36 }}>{cat.emoji}</div>
            <div
              style={{ fontSize: 20, color: colors.secondary, marginTop: 8 }}
            >
              {cat.label}
            </div>
          </div>
        ))}
      </div>
    </div>,
    {
      ...size,
    },
  );
}
