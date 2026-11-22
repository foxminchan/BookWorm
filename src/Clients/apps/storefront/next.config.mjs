import path from "node:path";

import { withMicrofrontends } from "@vercel/microfrontends/next/config";

import { createNextConfig } from "@workspace/app-config/next";

import "./env.mjs";

const nextConfig = createNextConfig({
  cacheComponents: true,
  optimizePackageImports: ["lucide-react", "date-fns", "ts-pattern"],
});

export default withMicrofrontends(nextConfig, {
  appName: "storefront",
  configPath: path.resolve(import.meta.dirname, "./microfrontends.config.json"),
  skipTransforms: ["redirects"],
});
