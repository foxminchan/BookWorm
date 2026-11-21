const navigationMock = {};

/**
 * Creates the shared `next/navigation` mock object for Vitest tests.
 */
export function createNextNavigationMock(vi) {
  return Object.assign(navigationMock, {
    useRouter() {
      return {
        push: vi.fn(),
        replace: vi.fn(),
        prefetch: vi.fn(),
        back: vi.fn(),
        forward: vi.fn(),
        refresh: vi.fn(),
        pathname: "/",
        query: {},
        asPath: "/",
      };
    },
    useSearchParams() {
      return new URLSearchParams();
    },
    usePathname() {
      return "/";
    },
    useParams() {
      return {};
    },
    redirect: vi.fn(),
    notFound: vi.fn(),
  });
}

/**
 * Configures deterministic Faker seeding, MSW lifecycle hooks, and gateway env vars.
 */
export function setupTestLifecycle({
  faker,
  server,
  cleanup,
  beforeAll,
  afterEach,
  afterAll,
  onUnhandledFrame,
  httpsGatewayUrl,
}) {
  const fakerSeed = process.env.FAKER_SEED
    ? Number.parseInt(process.env.FAKER_SEED, 10)
    : 12345;
  faker.seed(fakerSeed);

  beforeAll(() => {
    server.listen({ onUnhandledFrame });
  });

  afterEach(() => {
    server.resetHandlers();
    cleanup();
  });

  afterAll(() => {
    server.close();
  });

  process.env.NEXT_PUBLIC_GATEWAY_HTTP = "http://localhost:5000";
  process.env.NEXT_PUBLIC_GATEWAY_HTTPS = httpsGatewayUrl;
}
