import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import HeroSection from "@/features/content/returns/hero-section";
import TimelineSection from "@/features/content/returns/timeline-section";
import CtaSection from "@/features/content/returns/cta-section";
import EligibilitySection from "@/features/content/returns/eligibility-section";
import PolicyOverview from "@/features/content/returns/policy-overview";
import FaqSection from "@/features/content/returns/faq-section";

export default function ReturnsPage() {
  return (
    <div className="min-h-screen flex flex-col">
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
