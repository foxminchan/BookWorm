import { flag } from "flags/next";

export const showCopilotKit = flag<boolean>({
  key: "copilot-kit",
  decide() {
    return false; // Turn off by default - feature in development
  },
});
