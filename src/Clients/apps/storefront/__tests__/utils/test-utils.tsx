import type { ReactElement, ReactNode } from "react";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  type RenderOptions,
  type RenderResult,
  render,
} from "@testing-library/react";
import { Provider as JotaiProvider } from "jotai";
import { vi } from "vitest";

/**
 * Create a new QueryClient for testing with default options
 * that disable retries and set cache time to 0
 */
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

/**
 * Wrapper component that provides all necessary contexts for testing
 */
function AllTheProviders({ children }: AllTheProvidersProps) {
  const queryClient = createTestQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <JotaiProvider>{children}</JotaiProvider>
    </QueryClientProvider>
  );
}

/**
 * Create wrapper for hook testing with renderHook
 */
export function createWrapper() {
  return AllTheProviders;
}

/**
 * Custom render function that wraps components with necessary providers
 */
export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, "wrapper">,
): RenderResult {
  return render(ui, { wrapper: AllTheProviders, ...options });
}

/**
 * Mock session object for testing authenticated components
 */
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

/**
 * Mock auth client for testing
 */
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

// Re-export everything from React Testing Library
export * from "@testing-library/react";
