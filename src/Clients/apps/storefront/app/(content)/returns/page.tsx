import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import {
  HeroSection,
  PolicyOverview,
  EligibilitySection,
  TimelineSection,
  FaqSection,
  CtaSection,
} from "@/features/content/returns";

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
