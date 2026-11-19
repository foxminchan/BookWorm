/**
 * Creates a shared Playwright BDD configuration for BookWorm frontend applications.
 * @param {{
 *   testDir: string;
 *   epic: string;
 *   application: string;
 *   defaultPort: number;
 *   desktopChromeDevice: Record<string, unknown>;
 * }} options
 * @returns {any}
 */
export function createPlaywrightConfig({
  testDir,
  epic,
  application,
  defaultPort,
  desktopChromeDevice,
}) {
  const isCI = !!process.env.CI;

  return {
    testDir,
    fullyParallel: true,
    forbidOnly: isCI,
    retries: isCI ? 2 : 0,
    workers: isCI ? 4 : undefined,
    reporter: [
      isCI ? ["github"] : ["list"],
      [
        "allure-playwright",
        {
          resultsDir: "./allure-results",
          detail: true,
          suiteTitle: true,
          globalLabels: {
            layer: "e2e",
            epic,
            severity: "normal",
            owner: "BookWorm Engineering",
          },
          environmentInfo: {
            Application: application,
            Framework: "Next.js 16 / React 19",
            Runner: "Playwright BDD",
            Browser: "Chromium",
          },
          links: {
            issue: {
              nameTemplate: "Issue #%s",
              urlTemplate: "https://github.com/foxminchan/BookWorm/issues/%s",
            },
            tms: {
              nameTemplate: "TMS #%s",
              urlTemplate: "https://github.com/foxminchan/BookWorm/issues/%s",
            },
          },
        },
      ],
    ],
    use: {
      baseURL: process.env.BASE_URL || `http://localhost:${defaultPort}`,
      trace: "on-first-retry",
      screenshot: "only-on-failure",
      viewport: { width: 1920, height: 1080 },
    },
    projects: [
      {
        name: "chromium",
        use: { ...desktopChromeDevice },
      },
    ],
  };
}
