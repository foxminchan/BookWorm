import { createNextConfig } from "@workspace/app-config/next";

import "./env.mjs";

export default createNextConfig({
  cacheComponents: true,
  optimizePackageImports: ["lucide-react", "date-fns", "ts-pattern"],
});
