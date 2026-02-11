import { createStore } from "jotai";
import { describe, expect, it, vi } from "vitest";

import { isCopilotEnabledAtom } from "@/atoms/feature-flags-atom";

vi.mock("@/env.mjs", () => ({
  env: {
    NEXT_PUBLIC_COPILOT_ENABLED: true,
  },
}));

describe("feature-flags-atom", () => {
  it("should initialize isCopilotEnabledAtom with env value", () => {
    const store = createStore();
    const value = store.get(isCopilotEnabledAtom);

    expect(value).toBe(true);
  });

  it("should allow updating the atom value", () => {
    const store = createStore();

    store.set(isCopilotEnabledAtom, false);

    expect(store.get(isCopilotEnabledAtom)).toBe(false);
  });
});
