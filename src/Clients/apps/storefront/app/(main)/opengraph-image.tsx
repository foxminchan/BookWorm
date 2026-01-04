import { ImageResponse } from "next/og";

export const runtime = "edge";
export const alt = "BookWorm - Discover Your Next Great Read";
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
          fontSize: 90,
          fontWeight: "bold",
          color: "#2c1810",
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
          color: "#5c4033",
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
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 25,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 15,
            minWidth: 180,
          }}
        >
          <div style={{ fontSize: 48 }}>✨</div>
          <div
            style={{
              fontSize: 24,
              color: "#5c4033",
              marginTop: 12,
              textAlign: "center",
            }}
          >
            Curated
            <br />
            Selection
          </div>
        </div>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 25,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 15,
            minWidth: 180,
          }}
        >
          <div style={{ fontSize: 48 }}>🚚</div>
          <div
            style={{
              fontSize: 24,
              color: "#5c4033",
              marginTop: 12,
              textAlign: "center",
            }}
          >
            Fast
            <br />
            Shipping
          </div>
        </div>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            padding: 25,
            background: "rgba(255, 255, 255, 0.7)",
            borderRadius: 15,
            minWidth: 180,
          }}
        >
          <div style={{ fontSize: 48 }}>⭐</div>
          <div
            style={{
              fontSize: 24,
              color: "#5c4033",
              marginTop: 12,
              textAlign: "center",
            }}
          >
            Expert
            <br />
            Reviews
          </div>
        </div>
      </div>
    </div>,
    {
      ...size,
    },
  );
}
