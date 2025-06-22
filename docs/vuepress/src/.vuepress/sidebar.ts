import { sidebar } from "vuepress-theme-hope";

export default sidebar({
  "/": [
    "",
    {
      text: "Architecture",
      icon: "diagram-project",
      prefix: "architecture/",
      children: [
        {
          text: "1. Introduction Goals",
          link: "01-introduction-goals",
        },
        {
          text: "2. Constraints",
          link: "02-constraints",
        },
        {
          text: "3. Context and Scope",
          link: "03-context-scope",
        },
        {
          text: "4. Solution Strategy",
          link: "04-solution-strategy",
        },
        {
          text: "5. Building Block View",
          link: "05-building-block-view",
        },
        {
          text: "6. Runtime View",
          link: "06-runtime-view",
        },
        {
          text: "7. Deployment View",
          link: "07-deployment-view",
        },
        {
          text: "8. Cross-cutting Concepts",
          link: "08-cross-cutting-concepts",
        },
        {
          text: "9. Architecture Decisions",
          link: "09-architecture-decisions",
          prefix: "adr/",
          children: [
            {
              text: "Microservices Architecture",
              link: "adr-001-microservices-architecture",
            },
            {
              text: "Event-Driven Architecture with CQRS",
              link: "adr-002-event-driven-cqrs",
            },
            {
              text: ".NET Aspire for Cloud-Native Development",
              link: "adr-003-aspire-cloud-native",
            },
            {
              text: "PostgreSQL as Primary Database",
              link: "adr-004-postgresql-database",
            },
            {
              text: "Keycloak for Identity Management",
              link: "adr-005-keycloak-identity",
            },
            {
              text: "SignalR for Real-time Communication",
              link: "adr-006-signalr-realtime",
            },
            {
              text: "Container-First Deployment Strategy",
              link: "adr-007-container-deployment",
            },
            {
              text: "API Gateway Pattern Implementation",
              link: "adr-008-api-gateway",
            },
            {
              text: "AI Integration Strategy",
              link: "adr-009-ai-integration",
            },
          ],
        },
        {
          text: "10. Quality Requirements",
          link: "10-quality-requirements",
        },
        {
          text: "11. Risks and Technical Debt",
          link: "11-risks-technical-debt",
        },
        {
          text: "12. Glossary",
          link: "12-glossary",
        },
      ],
    },
  ],
});
