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

const stripFileExtension = (value) =>
  value
    .replace(/\.(test|spec|fuzz)\.[a-z0-9]+$/i, "")
    .replace(/\.feature$/i, "");

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
    const isIntegrationOrContract =
      parentSuite.endsWith(".ContractTests") ||
      parentSuite.endsWith(".IntegrationTests");

    ensureLabel(
      labels,
      "layer",
      isIntegrationOrContract ? "integration" : "unit",
    );

    const epic =
      parentSuite === "BookWorm.ArchTests"
        ? "Architecture"
        : parentSuite
            .replace(/^BookWorm\./, "")
            .replace(
              /\.(UnitTests|ContractTests|IntegrationTests|ArchTests)$/,
              "",
            ) || "Backend";

    ensureLabel(labels, "epic", epic);

    const relativeSuite =
      parentSuite && suite.startsWith(`${parentSuite}.`)
        ? suite.slice(parentSuite.length + 1)
        : suite;
    const suiteSegments = relativeSuite.split(".").filter(Boolean);

    const feature =
      suiteSegments[0] === "Features" && suiteSegments.length > 1
        ? suiteSegments[1]
        : suiteSegments[0] || subSuite || epic;

    ensureLabel(labels, "feature", feature);
    ensureLabel(
      labels,
      "story",
      subSuite || suiteSegments.at(-1) || testResult.name,
    );
  } else {
    const normalizedParent = parentSuite.replace(/\\/g, "/");
    const isE2E =
      framework.toLowerCase() === "playwright" ||
      normalizedParent.startsWith("e2e/") ||
      normalizedParent.includes("/e2e/") ||
      normalizedParent.endsWith(".feature");

    ensureLabel(labels, "layer", isE2E ? "e2e" : "unit");

    const fullName = testResult.fullName ?? "";
    const epic =
      options.defaultEpic ||
      (pkg.includes("backoffice") || fullName.includes("backoffice")
        ? "Backoffice"
        : pkg.includes("storefront") || fullName.includes("storefront")
          ? "Storefront"
          : "Frontend");

    ensureLabel(labels, "epic", epic);

    const cleanedPath = normalizedParent.replace(/^(__tests__|e2e)\//, "");
    const pathSegments = cleanedPath.split("/").filter(Boolean);

    const feature =
      pathSegments[0] === "features" && pathSegments.length > 1
        ? stripFileExtension(pathSegments[1])
        : pathSegments.length > 1
          ? pathSegments[0]
          : stripFileExtension(pathSegments[0] || "") || suite || epic;

    ensureLabel(labels, "feature", feature);
    ensureLabel(
      labels,
      "story",
      subSuite ||
        suite ||
        stripFileExtension(pathSegments.at(-1) || "") ||
        testResult.name,
    );
  }

  ensureLabel(labels, "severity", "normal");
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
    historyTestResults[tr.retryHash] = {
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
    };
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
    for (const point of historyPoints) {
      for (const htr of Object.values(point.testResults ?? {})) {
        enrichTestResultLabels(htr, this.options);
      }
    }

    if (historyPoints.length === 0 && testResults.length > 0) {
      seedBaselineHistoryPoint(historyPoints, testResults, context);
      store.updateHistoryFlags();
    }
  }
}
