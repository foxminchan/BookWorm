import type { ReactNode } from "react";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import HomepageHeader from "../components/HomepageHeader";

export default function Home(): ReactNode {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title}`}
      description="An architecture for building scalable and maintainable applications"
    >
      <HomepageHeader />
    </Layout>
  );
}
