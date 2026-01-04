import "./env.mjs";

/** @type {import('next').NextConfig} */
const nextConfig = {
  transpilePackages: ["@workspace/ui"],
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "*.blob.core.windows.net",
      },
      ...(process.env.NODE_ENV === "development"
        ? [
            {
              protocol: "https",
              hostname: "images.unsplash.com",
            },
            {
              protocol: "http",
              hostname: "localhost",
            },
            {
              protocol: "https",
              hostname: "localhost",
            },
          ]
        : []),
    ],
  },
  output: "standalone",
};

export default nextConfig;
