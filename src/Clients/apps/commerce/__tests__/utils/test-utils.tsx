import type { ReactElement, ReactNode } from "react";

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

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
        staleTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

type AllTheProvidersProps = {
  children: ReactNode;
};

function HydrateTestAtoms({
  queryClient,
  children,
}: Readonly<{
  queryClient: QueryClient;
  children: ReactNode;
}>) {
  useHydrateAtoms([[queryClientAtom, queryClient]]);
  return children;
}

function AllTheProviders({ children }: Readonly<AllTheProvidersProps>) {
  const queryClient = createTestQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <JotaiProvider>
        <HydrateTestAtoms queryClient={queryClient}>
          {children}
        </HydrateTestAtoms>
      </JotaiProvider>
    </QueryClientProvider>
  );
}

export function createWrapper() {
  return AllTheProviders;
}

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, "wrapper">,
): RenderResult {
  return render(ui, { wrapper: AllTheProviders, ...options });
}

export const mockSession = {
  user: {
    id: "test-user-id",
    email: "test@example.com",
    name: "Test User",
  },
  session: {
    token: "mock-token",
    expiresAt: new Date(Date.now() + 3600000).toISOString(),
  },
} as const;

type MockAuthClient = {
  useSession: () => {
    data: typeof mockSession;
    isPending: boolean;
    error: null;
  };
  signIn: {
    social: ReturnType<typeof vi.fn>;
  };
  signOut: ReturnType<typeof vi.fn>;
};

export const mockAuthClient: MockAuthClient = {
  useSession: () => ({
    data: mockSession,
    isPending: false,
    error: null,
  }),
  signIn: {
    social: vi.fn(),
  },
  signOut: vi.fn(),
};

export * from "@testing-library/react";
