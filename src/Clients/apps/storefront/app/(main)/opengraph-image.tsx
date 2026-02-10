import { ImageResponse } from "next/og";

export const runtime = "edge";
export const alt = "BookWorm - Discover Your Next Great Read";
export const size = {
  width: 1200,
  height: 630,
};
export const contentType = "image/png";

const colors = {
  primary: "#2c1810",
  secondary: "#5c4033",
} as const;

const features = [
  { emoji: "✨", label: "Curated\nSelection" },
  { emoji: "🚚", label: "Fast\nShipping" },
  { emoji: "⭐", label: "Expert\nReviews" },
] as const;

const featureCardStyle = {
  display: "flex" as const,
  flexDirection: "column" as const,
  alignItems: "center" as const,
  padding: 25,
  background: "rgba(255, 255, 255, 0.7)",
  borderRadius: 15,
  minWidth: 180,
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
          fontSize: 90,
          fontWeight: "bold",
          color: colors.primary,
          marginBottom: 30,
          display: "flex",
          alignItems: "center",
        }}
      >
        📚 BookWorm
      </div>

      {/* Tagline */}
      <div
        style={{
          fontSize: 48,
          color: colors.secondary,
          marginBottom: 60,
          textAlign: "center",
          maxWidth: "80%",
        }}
      >
        Discover Your Next Great Read
      </div>

      {/* Features */}
      <div
        style={{
          display: "flex",
          gap: 40,
          marginTop: 20,
        }}
      >
        {features.map((feature) => (
          <div key={feature.label} style={featureCardStyle}>
            <div style={{ fontSize: 48 }}>{feature.emoji}</div>
            <div
              style={{
                fontSize: 24,
                color: colors.secondary,
                marginTop: 12,
                textAlign: "center",
              }}
            >
              {feature.label}
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
