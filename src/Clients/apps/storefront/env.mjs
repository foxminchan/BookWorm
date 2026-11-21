import { createEnv } from "@t3-oss/env-nextjs";

import { authKeys } from "@workspace/env/auth";
import { copilotKeys } from "@workspace/env/copilot";
import { gatewayKeys } from "@workspace/env/gateway";
import { otelKeys } from "@workspace/env/otel";

export const env = createEnv({
  extends: [authKeys(), gatewayKeys(), otelKeys(), copilotKeys()],
  server: {},
  client: {},
  runtimeEnv: {},
  skipValidation: !!process.env.SKIP_ENV_VALIDATION,
  emptyStringAsUndefined: true,
});
