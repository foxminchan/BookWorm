/**
 * Lighthouse CI configuration for BookWorm Storefront
 * @see https://github.com/GoogleChrome/lighthouse-ci/blob/main/docs/configuration.md
 */

export default {
  ci: {
    collect: {
      // URLs to test
      url: [
        "http://localhost:3000/",
        "http://localhost:3000/books",
        "http://localhost:3000/basket",
      ],
      // Number of runs per URL
      numberOfRuns: 3,
      // Start server before collecting
      startServerCommand: "pnpm run start",
      startServerReadyPattern: "Ready",
      startServerReadyTimeout: 30000,
      // Chrome settings
      settings: {
        preset: "desktop",
        throttling: {
          rttMs: 40,
          throughputKbps: 10240,
          cpuSlowdownMultiplier: 1,
        },
      },
    },
    assert: {
      preset: "lighthouse:recommended",
      assertions: {
        // Performance
        "first-contentful-paint": ["warn", { maxNumericValue: 2000 }],
        "largest-contentful-paint": ["warn", { maxNumericValue: 2500 }],
        "cumulative-layout-shift": ["error", { maxNumericValue: 0.1 }],
        "total-blocking-time": ["warn", { maxNumericValue: 300 }],
        "speed-index": ["warn", { maxNumericValue: 3000 }],

        // Accessibility
        "categories:accessibility": ["error", { minScore: 0.9 }],

        // Best Practices
        "categories:best-practices": ["warn", { minScore: 0.9 }],

        // SEO
        "categories:seo": ["warn", { minScore: 0.9 }],

        // PWA (optional, adjust based on requirements)
        "categories:pwa": "off",

        // Modern image formats
        "modern-image-formats": "off",

        // Disable some checks that may not apply
        "unsized-images": "off",
        "uses-rel-preconnect": "off",
      },
    },
    upload: {
      target: "temporary-public-storage",
    },
  },
};
