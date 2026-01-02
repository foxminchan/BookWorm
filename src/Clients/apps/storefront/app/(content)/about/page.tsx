"use client";

import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import ContactSection from "@/features/content/about/contact-section";
import HeroSection from "@/features/content/about/hero-section";
import MissionSection from "@/features/content/about/mission-section";
import TimelineSection from "@/features/content/about/timeline-section";
import ValuesSection from "@/features/content/about/values-section";

export default function AboutPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="grow">
        <HeroSection />
        <MissionSection />
        <ValuesSection />
        <TimelineSection />
        <ContactSection />
      </main>
      <Footer />
    </div>
  );
}
