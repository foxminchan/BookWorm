"use client";

import { useRef } from "react";

import { ChatBot, type ChatBotRef } from "@/components/chat-bot";
import AiRecommendationsSection from "@/features/home/ai-recommendations-section";
import CategoriesSection from "@/features/home/categories-section";
import FeaturedBooksSection from "@/features/home/featured-books-section";
import HeroSection from "@/features/home/hero-section";

/**
 * Client Component: Renders the homepage content using hydrated data.
 * This component will receive the prefetched data from the server.
 */
export default function BookshopPageContent() {
  const chatBotRef = useRef<ChatBotRef>(null);

  return (
    <main id="main-content">
      <HeroSection />
      <FeaturedBooksSection />
      <CategoriesSection />
      <AiRecommendationsSection chatBotRef={chatBotRef} />
      <ChatBot ref={chatBotRef} />
    </main>
  );
}
