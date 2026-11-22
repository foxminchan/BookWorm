/**
 * Creates a shared Next.js configuration for BookWorm frontend applications.
 */
export function createNextConfig({
  basePath,
  assetPrefix,
  optimizePackageImports,
  cacheComponents,
}) {
  /** @type {import('next').NextConfig} */
  const nextConfig = {
    ...(basePath ? { basePath } : {}),
    ...(assetPrefix ? { assetPrefix } : {}),
    ...(cacheComponents
      ? { cacheComponents: true, partialPrefetching: true }
      : {}),
    transpilePackages: [
      "@workspace/ui",
      "@workspace/auth",
      "@workspace/seo",
      "@workspace/storefront-shell",
    ],
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
