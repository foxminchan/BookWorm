/**
 * Creates a shared Next.js configuration for BookWorm frontend applications.
 */
export function createNextConfig({ basePath, optimizePackageImports }) {
  /** @type {import('next').NextConfig} */
  const nextConfig = {
    ...(basePath ? { basePath } : {}),
    transpilePackages: ["@workspace/ui"],
    experimental: {
      optimizePackageImports,
    },
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

  return nextConfig;
}
