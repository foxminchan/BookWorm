import { createNextConfig } from "@workspace/app-config/next";

import "./env.mjs";

export default createNextConfig({
  optimizePackageImports: ["lucide-react", "date-fns", "ts-pattern"],
});
