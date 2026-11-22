import { type ReactElement, createElement } from "react";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  type RenderOptions,
  type RenderResult,
  render,
} from "@testing-library/react";
import { Provider as JotaiProvider } from "jotai";
import { queryClientAtom } from "jotai-tanstack-query";
import { useHydrateAtoms } from "jotai/utils";
import { vi } from "vitest";

import { createTestUtils } from "@workspace/app-config/test-utils";

const utils = createTestUtils({
  createElement,
  QueryClient,
  QueryClientProvider,
  render,
  JotaiProvider,
  queryClientAtom,
  useHydrateAtoms,
  vi,
});

export const {
  createTestQueryClient,
  createWrapper,
  mockSession,
  mockAuthClient,
} = utils;

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, "wrapper">,
): RenderResult {
  return utils.renderWithProviders(ui, options);
}

export * from "@testing-library/react";
