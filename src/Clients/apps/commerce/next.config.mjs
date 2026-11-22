import path from "node:path";

import { withMicrofrontends } from "@vercel/microfrontends/next/config";

import { createNextConfig } from "@workspace/app-config/next";

import "./env.mjs";

const nextConfig = createNextConfig({
  cacheComponents: false,
  optimizePackageImports: ["lucide-react", "date-fns", "ts-pattern"],
});

export default withMicrofrontends(nextConfig, {
  appName: "commerce",
  configPath: path.resolve(
    import.meta.dirname,
    "../storefront/microfrontends.json",
  ),
  skipTransforms: ["redirects"],
});
