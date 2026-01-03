import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import CtaSection from "@/features/content/returns/cta-section";
import EligibilitySection from "@/features/content/returns/eligibility-section";
import FaqSection from "@/features/content/returns/faq-section";
import HeroSection from "@/features/content/returns/hero-section";
import PolicyOverview from "@/features/content/returns/policy-overview";
import TimelineSection from "@/features/content/returns/timeline-section";

export default function ReturnsPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="grow">
        <HeroSection />
        <PolicyOverview />
        <EligibilitySection />
        <TimelineSection />
        <FaqSection />
        <CtaSection />
      </main>
      <Footer />
    </div>
  );
}
