import { createNextConfig } from "@workspace/app-config/next";

import "./env.mjs";

export default createNextConfig({
  basePath: "/backoffice",
  optimizePackageImports: ["lucide-react", "recharts", "date-fns"],
});
