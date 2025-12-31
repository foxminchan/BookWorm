"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import {
  HeroSection,
  MissionSection,
  ValuesSection,
  TimelineSection,
  ContactSection,
} from "@/features/about";

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
