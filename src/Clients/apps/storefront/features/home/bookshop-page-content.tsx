"use client";

import type React from "react";
import { useRef } from "react";

import { ChatBot, type ChatBotRef } from "@/components/chat-bot";
import AiRecommendationsSection from "@/features/home/ai-recommendations-section";
import CategoriesSection from "@/features/home/categories-section";
import FeaturedBooksSection from "@/features/home/featured-books-section";

export default function BookshopPageContent({
  heroSlot,
}: Readonly<{
  heroSlot?: React.ReactNode;
}>) {
  const chatBotRef = useRef<ChatBotRef>(null);

  return (
    <div>
      {heroSlot}
      <FeaturedBooksSection />
      <CategoriesSection />
      <AiRecommendationsSection chatBotRef={chatBotRef} />
      <ChatBot ref={chatBotRef} />
    </div>
  );
}
