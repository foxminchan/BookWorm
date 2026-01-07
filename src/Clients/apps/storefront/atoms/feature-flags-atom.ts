"use client";

import { atom } from "jotai";

import { env } from "@/env.mjs";

export const isCopilotEnabledAtom = atom<boolean>(
  env.NEXT_PUBLIC_COPILOT_ENABLED,
);
