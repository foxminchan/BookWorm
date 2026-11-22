import { createEnv } from "@t3-oss/env-nextjs";
import * as v from "valibot";

/** Validates CopilotKit chat assistant environment variables. */
export const copilotKeys = () =>
  createEnv({
    client: {
      NEXT_PUBLIC_COPILOT_AGENT_NAME: v.optional(v.string(), "chat-workflow"),
      NEXT_PUBLIC_COPILOT_ENABLED: v.pipe(
        v.optional(v.string(), "false"),
        v.transform((val) => val === "true"),
      ),
    },
    runtimeEnv: {
      NEXT_PUBLIC_COPILOT_AGENT_NAME:
        process.env.NEXT_PUBLIC_COPILOT_AGENT_NAME,
      NEXT_PUBLIC_COPILOT_ENABLED: process.env.NEXT_PUBLIC_COPILOT_ENABLED,
    },
    skipValidation: !!process.env.SKIP_ENV_VALIDATION,
    emptyStringAsUndefined: true,
  });
