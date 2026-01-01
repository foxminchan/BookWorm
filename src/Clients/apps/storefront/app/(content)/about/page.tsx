"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import ContactSection from "@/features/content/about/contact-section";
import MissionSection from "@/features/content/about/mission-section";
import ValuesSection from "@/features/content/about/values-section";
import HeroSection from "@/features/content/about/hero-section";
import TimelineSection from "@/features/content/about/timeline-section";

export default function AboutPage() {
  return (
    <div className="min-h-screen flex flex-col">
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
