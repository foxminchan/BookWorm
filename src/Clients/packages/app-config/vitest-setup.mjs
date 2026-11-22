import crypto from "node:crypto";

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
 * Configures Vercel Microfrontends environment variables for Vitest tests.
 */
export function setupMfeTestEnv({ MicrofrontendsServer, configPath, appName }) {
  const mfeServer = MicrofrontendsServer.fromFile({ filePath: configPath });
  process.env.NEXT_PUBLIC_MFE_CURRENT_APPLICATION = appName;
  process.env.NEXT_PUBLIC_MFE_CURRENT_APPLICATION_HASH = crypto
    .createHash("md5")
    .update(appName)
    .digest("hex")
    .slice(0, 6);
  process.env.NEXT_PUBLIC_MFE_CLIENT_CONFIG = JSON.stringify(
    mfeServer.config.toClientConfig({ removeFlaggedPaths: true }).serialize(),
  );
}

const NEXT_IMAGE_PROPS = new Set([
  "fill",
  "priority",
  "quality",
  "sizes",
  "loader",
]);

/**
 * Creates a `next/image` mock that strips Next-specific props in Happy DOM.
 */
export function createNextImageMock(createElement) {
  return {
    __esModule: true,
    default: (props) => {
      const { src, alt, ...rawRest } = props;
      const rest = Object.fromEntries(
        Object.entries(rawRest).filter(([key]) => !NEXT_IMAGE_PROPS.has(key)),
      );
      const resolvedSrc =
        typeof src === "string" ? src : (src?.src ?? "");

      return createElement("img", {
        src: resolvedSrc,
        alt: alt ?? "",
        ...rest,
      });
    },
  };
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
