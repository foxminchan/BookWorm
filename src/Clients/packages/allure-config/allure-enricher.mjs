import { fileURLToPath } from "node:url";

const SPECIAL_TOKENS = new Map([
  ["a11y", "Accessibility"],
  ["a2ui", "A2UI"],
  ["ai", "AI"],
  ["api", "API"],
  ["bdd", "BDD"],
  ["bo", "Backoffice"],
  ["ci", "CI"],
  ["crud", "CRUD"],
  ["db", "DB"],
  ["ddd", "DDD"],
  ["dto", "DTO"],
  ["e2e", "E2E"],
  ["fe", "Frontend"],
  ["grpc", "gRPC"],
  ["http", "HTTP"],
  ["id", "ID"],
  ["jwt", "JWT"],
  ["kpi", "KPI"],
  ["mcp", "MCP"],
  ["msw", "MSW"],
  ["otel", "OpenTelemetry"],
  ["pwa", "PWA"],
  ["seo", "SEO"],
  ["sf", "Storefront"],
  ["sse", "SSE"],
  ["tunit", "TUnit"],
  ["ui", "UI"],
  ["url", "URL"],
  ["uuid", "UUID"],
  ["webmcp", "WebMCP"],
]);

const FRONTEND_AREA_EPICS = new Map([
  ["atoms", "Hooks & State"],
  ["components", "UI Components"],
  ["e2e", "E2E Flows"],
  ["factories", "Core & Utilities"],
  ["fuzz", "Property & Fuzzing"],
  ["hooks", "Hooks & State"],
  ["lib", "Core & Utilities"],
  ["utils", "Core & Utilities"],
]);

const getLabel = (labels, name) =>
  labels.find((label) => label.name === name)?.value;

const ensureLabel = (labels, name, value) => {
  if (!value) {
    return;
  }
  if (!labels.some((label) => label.name === name && label.value)) {
    labels.push({ name, value });
  }
};

const setLabel = (labels, name, value) => {
  if (!value) {
    return;
  }
  const existing = labels.find((label) => label.name === name);
  if (existing) {
    existing.value = value;
  } else {
    labels.push({ name, value });
  }
};

const stripFileExtension = (value) =>
  value
    .replace(/\.(test|spec|fuzz)\.[a-z0-9]+$/i, "")
    .replace(/\.(test|spec|fuzz)$/i, "")
    .replace(/\.feature$/i, "");

export const humanizeSegment = (raw) => {
  if (!raw) {
    return "";
  }
  const cleaned = stripFileExtension(raw.trim());
  if (!cleaned) {
    return "";
  }
  // Preserve camelCase hook/utility identifiers such as useBasketActions or forEachSequentially
  if (/^(use[A-Z]|[a-z]+[A-Z])/.test(cleaned) && !cleaned.includes("-")) {
    return cleaned;
  }
  return cleaned
    .split(/[-_.\s]+/)
    .filter(Boolean)
    .map((word) => {
      const lower = word.toLowerCase();
      if (SPECIAL_TOKENS.has(lower)) {
        return SPECIAL_TOKENS.get(lower);
      }
      return word.charAt(0).toUpperCase() + word.slice(1);
    })
    .join(" ");
};

export const defaultCategoriesConfig = [
  {
    id: "architecture-violations",
    name: "Architecture & Boundary Violations",
    matchers: {
      statuses: ["failed", "broken"],
      labels: { epic: /Architecture/i },
    },
    groupBy: ["severity", { label: "feature" }],
    groupByMessage: true,
  },
  {
    id: "contract-failures",
    name: "Contract & Messaging Verification Failures",
    matchers: {
      statuses: ["failed", "broken"],
      message: /Pact|Contract|Consumer|Provider|Schema/i,
    },
    groupBy: ["environment", { label: "epic" }],
    groupByMessage: true,
  },
  {
    id: "ui-assertion-failures",
    name: "UI & Component Assertion Failures",
    matchers: {
      statuses: ["failed"],
      message:
        /TestingLibraryElementError|Unable to find|toBeInTheDocument|toHaveTextContent|expect\(/i,
    },
    groupBy: ["layer", { label: "epic" }, { label: "feature" }],
    groupByMessage: true,
  },
  {
    id: "timeout-infrastructure-errors",
    name: "Timeout & Infrastructure Errors",
    matchers: {
      statuses: ["failed", "broken"],
      message: /Timeout|timed out|ETIMEDOUT|ECONNREFUSED|network/i,
    },
    groupBy: ["environment", "layer"],
    groupByMessage: true,
  },
  {
    id: "flaky-tests",
    name: "Flaky Tests",
    matchers: {
      flaky: true,
    },
    groupBy: ["environment", { label: "epic" }],
    groupByMessage: false,
  },
];

const resolveDotNetEpic = (parentSuite) => {
  if (
    parentSuite === "BookWorm.ArchTests" ||
    parentSuite.endsWith(".ArchTests")
  ) {
    return "Architecture";
  }

  return (
    parentSuite
      .replace(/^BookWorm\./, "")
      .replace(/\.(UnitTests|ContractTests|IntegrationTests|ArchTests)$/, "") ||
    "Backend"
  );
};

const enrichDotNetTestLabels = (
  testResult,
  labels,
  { parentSuite, suite, subSuite },
) => {
  const isIntegrationOrContract =
    parentSuite.endsWith(".ContractTests") ||
    parentSuite.endsWith(".IntegrationTests");

  setLabel(labels, "layer", isIntegrationOrContract ? "integration" : "unit");
  ensureLabel(labels, "framework", "TUnit");
  ensureLabel(labels, "package", suite || parentSuite);

  const epic = resolveDotNetEpic(parentSuite);
  setLabel(labels, "epic", epic);

  const relativeSuite =
    parentSuite && suite.startsWith(`${parentSuite}.`)
      ? suite.slice(parentSuite.length + 1)
      : suite;
  const suiteSegments = relativeSuite.split(".").filter(Boolean);

  const feature =
    suiteSegments[0] === "Features" && suiteSegments.length > 1
      ? suiteSegments[1]
      : suiteSegments[0] || subSuite || epic;

  setLabel(labels, "feature", feature);
  setLabel(
    labels,
    "story",
    subSuite || suiteSegments.at(-1) || testResult.name,
  );
};

const resolveFrontendLayer = (isE2E, topFolder) => {
  if (isE2E) {
    return "e2e";
  }
  if (topFolder === "features") {
    return "integration";
  }
  return "unit";
};

const resolveFrontendAppName = (defaultEpic, pkg, fullName) => {
  if (defaultEpic) {
    return defaultEpic;
  }
  if (pkg.includes("backoffice") || fullName.includes("backoffice")) {
    return "Backoffice";
  }
  if (pkg.includes("storefront") || fullName.includes("storefront")) {
    return "Storefront";
  }
  return "Frontend";
};

const resolveFrontendDomainSegment = (
  topFolder,
  pathSegments,
  isE2E,
  appName,
) => {
  if (topFolder === "features" && pathSegments.length > 1) {
    return humanizeSegment(pathSegments[1]);
  }
  const areaEpic = FRONTEND_AREA_EPICS.get(topFolder);
  if (areaEpic) {
    return areaEpic;
  }
  if (isE2E) {
    return "E2E Flows";
  }
  return humanizeSegment(topFolder) || appName;
};

const applyIndividualFrontendLabels = (
  labels,
  {
    appName,
    domainSegment,
    fileModuleName,
    pathSegments,
    suite,
    subSuite,
    testName,
  },
) => {
  const nestedSegment =
    pathSegments.length > 2 ? humanizeSegment(pathSegments[2]) : "";
  const individualEpic = domainSegment || appName;
  const individualFeature =
    fileModuleName || nestedSegment || suite || individualEpic;
  const individualStory = subSuite || suite || fileModuleName || testName;

  setLabel(labels, "epic", individualEpic);
  setLabel(labels, "feature", individualFeature);
  setLabel(labels, "story", individualStory);
};

const applyUnifiedFrontendLabels = (
  labels,
  { appName, domainSegment, fileModuleName, suite, subSuite, testName },
) => {
  const unifiedFeature = domainSegment || fileModuleName || suite || appName;
  const unifiedStory = fileModuleName || subSuite || suite || testName;

  setLabel(labels, "epic", appName);
  setLabel(labels, "feature", unifiedFeature);
  setLabel(labels, "story", unifiedStory);
};

const enrichFrontendTestLabels = (
  testResult,
  labels,
  options,
  { framework, pkg, parentSuite, suite, subSuite },
) => {
  const normalizedParent = parentSuite.replaceAll("\\", "/");
  const cleanedPath = normalizedParent.replace(/^(__tests__|e2e)\//, "");
  const pathSegments = cleanedPath.split("/").filter(Boolean);
  const topFolder = (pathSegments[0] || "").toLowerCase();

  const isE2E =
    framework.toLowerCase() === "playwright" ||
    normalizedParent.startsWith("e2e/") ||
    normalizedParent.includes("/e2e/") ||
    normalizedParent.endsWith(".feature");

  setLabel(labels, "layer", resolveFrontendLayer(isE2E, topFolder));
  ensureLabel(labels, "framework", isE2E ? "playwright" : "vitest");

  const fullName = testResult.fullName ?? "";
  const appName = resolveFrontendAppName(options.defaultEpic, pkg, fullName);
  const fileModuleName = humanizeSegment(pathSegments.at(-1) || "");
  const domainSegment = resolveFrontendDomainSegment(
    topFolder,
    pathSegments,
    isE2E,
    appName,
  );

  const scope = options.scope || "unified";
  const isIndividualFrontendScope =
    scope === "backoffice" || scope === "storefront";
  const labelContext = {
    appName,
    domainSegment,
    fileModuleName,
    pathSegments,
    suite,
    subSuite,
    testName: testResult.name,
  };

  if (isIndividualFrontendScope) {
    applyIndividualFrontendLabels(labels, labelContext);
  } else {
    applyUnifiedFrontendLabels(labels, labelContext);
  }
};

export const enrichTestResultLabels = (testResult, options = {}) => {
  testResult.labels ??= [];
  const { labels } = testResult;

  const language = getLabel(labels, "language") ?? "";
  const framework = getLabel(labels, "framework") ?? "";
  const pkg = getLabel(labels, "package") ?? "";
  const parentSuite = getLabel(labels, "parentSuite") ?? "";
  const suite = getLabel(labels, "suite") ?? "";
  const subSuite = getLabel(labels, "subSuite") ?? "";

  const isDotNet =
    language.toLowerCase() === "c#" || parentSuite.startsWith("BookWorm.");

  if (isDotNet) {
    enrichDotNetTestLabels(testResult, labels, {
      parentSuite,
      suite,
      subSuite,
    });
  } else {
    enrichFrontendTestLabels(testResult, labels, options, {
      framework,
      pkg,
      parentSuite,
      suite,
      subSuite,
    });
  }

  ensureLabel(labels, "severity", "normal");
  ensureLabel(labels, "owner", "BookWorm Engineering");
};

const createBaselineHistoryEntry = (tr, baselineTimestamp) => ({
  id: `${tr.id}-baseline`,
  name: tr.name,
  fullName: tr.fullName,
  environment: tr.environment,
  status: tr.status,
  retries: [],
  message: tr.error?.message,
  trace: tr.error?.trace,
  start: tr.start ? tr.start - 60_000 : baselineTimestamp,
  stop: tr.stop ? tr.stop - 60_000 : baselineTimestamp + (tr.duration ?? 0),
  duration: tr.duration ?? 0,
  labels: tr.labels,
  flaky: false,
  transition: undefined,
  url: "",
  retryHash: tr.retryHash,
  reportLinks: [],
});

const buildCurrentResultLookups = (nonRetryResults) => {
  const currentByRetryHash = new Map();
  const currentByFullName = new Map();

  for (const tr of nonRetryResults) {
    currentByRetryHash.set(tr.retryHash, tr);
    if (tr.fullName) {
      currentByFullName.set(tr.fullName, tr);
    }
  }

  return { currentByRetryHash, currentByFullName };
};

const findMatchingCurrentResult = (key, htr, lookups) => {
  const byHash = lookups.currentByRetryHash.get(key);
  if (byHash) {
    return byHash;
  }
  return htr.fullName ? lookups.currentByFullName.get(htr.fullName) : undefined;
};

const reconcileSingleHistoryPoint = (point, lookups, options) => {
  const nextTestResults = {};
  const knownIds = new Set(point.knownTestCaseIds ?? []);

  for (const [key, htr] of Object.entries(point.testResults ?? {})) {
    enrichTestResultLabels(htr, options);
    const matchedTr = findMatchingCurrentResult(key, htr, lookups);

    if (matchedTr) {
      htr.environment = matchedTr.environment;
      htr.retryHash = matchedTr.retryHash;
      if (matchedTr.testCase?.id) {
        knownIds.add(matchedTr.testCase.id);
      }
    }

    const targetKey = htr.retryHash || key;
    nextTestResults[targetKey] = htr;
  }

  point.testResults = nextTestResults;
  point.knownTestCaseIds = [...knownIds];
};

const backfillLatestHistoryPoint = (latestPoint, nonRetryResults) => {
  latestPoint.testResults ??= {};
  const knownIds = new Set(latestPoint.knownTestCaseIds ?? []);
  const fallbackTimestamp = latestPoint.timestamp || Date.now() - 60_000;

  for (const tr of nonRetryResults) {
    if (!latestPoint.testResults[tr.retryHash]) {
      latestPoint.testResults[tr.retryHash] = createBaselineHistoryEntry(
        tr,
        fallbackTimestamp,
      );
    }
    if (tr.testCase?.id) {
      knownIds.add(tr.testCase.id);
    }
  }

  latestPoint.knownTestCaseIds = [...knownIds];
};

const reconcileHistoryPoints = (historyPoints, testResults, options) => {
  const nonRetryResults = testResults.filter(
    (tr) => !tr.isRetry && tr.retryHash,
  );
  const lookups = buildCurrentResultLookups(nonRetryResults);

  for (const point of historyPoints) {
    reconcileSingleHistoryPoint(point, lookups, options);
  }

  // Ensure the most recent history point covers all active test results so
  // per-environment stability & coverage diff charts always have baseline data.
  if (historyPoints.length > 0 && nonRetryResults.length > 0) {
    backfillLatestHistoryPoint(historyPoints[0], nonRetryResults);
  }
};

const seedBaselineHistoryPoint = (historyPoints, testResults, context) => {
  const nonRetryResults = testResults.filter(
    (tr) => !tr.isRetry && tr.retryHash,
  );
  if (nonRetryResults.length === 0) {
    return;
  }

  const minStart = nonRetryResults.reduce(
    (min, tr) => (tr.start && tr.start < min ? tr.start : min),
    Date.now(),
  );
  const baselineTimestamp = minStart - 60_000;
  const historyTestResults = {};
  const knownTestCaseIds = [];

  for (const tr of nonRetryResults) {
    if (tr.testCase?.id) {
      knownTestCaseIds.push(tr.testCase.id);
    }
    historyTestResults[tr.retryHash] = createBaselineHistoryEntry(
      tr,
      baselineTimestamp,
    );
  }

  historyPoints.push({
    uuid: `${context.reportUuid}-baseline`,
    name: "Baseline",
    timestamp: baselineTimestamp,
    knownTestCaseIds: [...new Set(knownTestCaseIds)],
    testResults: historyTestResults,
    metrics: {},
    url: "",
  });
};

export default class AllureEnricherPlugin {
  constructor(options = {}) {
    this.options = options;
  }

  async done(context, store) {
    const testResults = await store.allTestResults({ includeRetries: true });
    for (const tr of testResults) {
      enrichTestResultLabels(tr, this.options);
    }

    const historyPoints = await store.allHistoryDataPoints();
    if (historyPoints.length > 0) {
      reconcileHistoryPoints(historyPoints, testResults, this.options);
      store.updateHistoryFlags();
    } else if (testResults.length > 0) {
      seedBaselineHistoryPoint(historyPoints, testResults, context);
      store.updateHistoryFlags();
    }
  }
}

export const hasPathSegment = (labels, segment) =>
  labels.some(({ name, value }) => {
    if (!value || (name !== "parentSuite" && name !== "package")) {
      return false;
    }
    const normalized = value.replaceAll("\\", "/").toLowerCase();
    return (
      normalized.includes(`/${segment}/`) ||
      normalized.startsWith(`${segment}/`) ||
      normalized.includes(`.${segment}.`)
    );
  });

export const isE2ETest = (labels) =>
  labels.some(
    ({ name, value }) =>
      (name === "framework" && value?.toLowerCase() === "playwright") ||
      (name === "layer" && value?.toLowerCase() === "e2e") ||
      (name === "parentSuite" &&
        (value?.startsWith("e2e/") || value?.endsWith(".feature"))),
  );

export const createFrontendEnvironments = ({
  featuresScope,
  componentsScope,
  coreName,
  coreScope,
  coreSegments,
  e2eScope,
  includeFuzz = false,
}) => {
  const environments = {
    features: {
      name: "Domain Features",
      variables: {
        Layer: "Integration",
        Scope: featuresScope,
        Runner: "Vitest + MSW + TanStack Query",
      },
      matcher: ({ labels }) =>
        !isE2ETest(labels) && hasPathSegment(labels, "features"),
    },
    components: {
      name: "UI Components",
      variables: {
        Layer: "Unit",
        Scope: componentsScope,
        Runner: "Vitest + Testing Library",
      },
      matcher: ({ labels }) =>
        !isE2ETest(labels) && hasPathSegment(labels, "components"),
    },
    core: {
      name: coreName,
      variables: {
        Layer: "Unit",
        Scope: coreScope,
        Runner: "Vitest",
      },
      matcher: ({ labels }) =>
        !isE2ETest(labels) &&
        coreSegments.some((segment) => hasPathSegment(labels, segment)),
    },
  };

  if (includeFuzz) {
    environments.fuzz = {
      name: "Property & Fuzz Tests",
      variables: {
        Layer: "Unit",
        Scope: "Property-Based Invariants & Fuzzing",
        Runner: "Vitest + fast-check",
      },
      matcher: ({ labels }) =>
        !isE2ETest(labels) && hasPathSegment(labels, "fuzz"),
    };
  }

  environments.e2e = {
    name: "E2E (Playwright BDD)",
    variables: {
      Layer: "E2E",
      Scope: e2eScope,
      Runner: "Playwright + Chromium",
    },
    matcher: ({ labels }) => isE2ETest(labels),
  };

  return environments;
};

export const createAppAllureConfig = ({
  reportName,
  enricherPluginPath = fileURLToPath(import.meta.url),
  enricherOptions,
  variables,
  environments,
  qualityGateRules,
}) => ({
  name: reportName,
  output: "allure-report",
  historyPath: "./.allure/history.jsonl",
  historyLimit: 20,
  defaultLabels: {
    severity: "normal",
    owner: "BookWorm Engineering",
  },
  variables,
  environments,
  categories: defaultCategoriesConfig,
  plugins: {
    enricher: {
      import: enricherPluginPath,
      options: enricherOptions,
    },
    awesome: {
      options: {
        singleFile: true,
        reportLanguage: "en",
        reportName,
        groupBy: ["parentSuite", "suite", "subSuite"],
      },
    },
    log: {
      options: {
        groupBy: "none",
        filter: ({ status }) => status === "failed" || status === "broken",
      },
    },
    dashboard: {
      options: {
        reportName,
        reportLanguage: "en",
      },
    },
  },
  qualityGate: {
    rules: [
      {
        maxFailures: 0,
        fastFail: true,
      },
    ],
    use: [...qualityGateRules],
  },
});
