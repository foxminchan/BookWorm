/**
 * Creates shared React Testing Library, TanStack Query, and Jotai test utilities.
 * Dependencies are injected so `@workspace/app-config` stays dependency-free.
 *
 * @template TQueryClient
 * @param {{
 *   createElement: typeof import("react").createElement;
 *   QueryClient: new (config?: any) => TQueryClient;
 *   QueryClientProvider: any;
 *   render: any;
 *   JotaiProvider: any;
 *   queryClientAtom: any;
 *   useHydrateAtoms: any;
 *   vi: { fn: () => any };
 * }} deps
 */
export function createTestUtils({
  createElement,
  QueryClient,
  QueryClientProvider,
  render,
  JotaiProvider,
  queryClientAtom,
  useHydrateAtoms,
  vi,
}) {
  function createTestQueryClient() {
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

  function HydrateTestAtoms({ queryClient, children }) {
    useHydrateAtoms([[queryClientAtom, queryClient]]);
    return children;
  }

  function AllTheProviders({ children }) {
    const queryClient = createTestQueryClient();

    return createElement(
      QueryClientProvider,
      { client: queryClient },
      createElement(
        JotaiProvider,
        null,
        createElement(HydrateTestAtoms, { queryClient }, children),
      ),
    );
  }

  function createWrapper() {
    return AllTheProviders;
  }

  function renderWithProviders(ui, options) {
    return render(ui, { wrapper: AllTheProviders, ...options });
  }

  const mockSession = {
    user: {
      id: "test-user-id",
      email: "test@example.com",
      name: "Test User",
    },
    session: {
      token: "mock-token",
      expiresAt: new Date(Date.now() + 3600000).toISOString(),
    },
  };

  const mockAuthClient = {
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

  return {
    createTestQueryClient,
    createWrapper,
    renderWithProviders,
    mockSession,
    mockAuthClient,
  };
}
