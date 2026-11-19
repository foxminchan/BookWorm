export default {
  name: "BookWorm Unified Test Report",
  output: "allure-report",
  plugins: {
    awesome: {
      options: {
        singleFile: true,
        reportLanguage: "en",
        reportName: "BookWorm Unified Test Report",
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
        reportName: "BookWorm Unified Test Report",
        reportLanguage: "en",
      },
    },
  },
};
