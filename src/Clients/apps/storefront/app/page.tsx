"use client";

import { useRef } from "react";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { ChatBot, type ChatBotRef } from "@/components/chat-bot";
import HeroSection from "@/features/Home/hero-section";
import FeaturedBooksSection from "@/features/Home/featured-books-section";
import CategoriesSection from "@/features/Home/categories-section";
import AiRecommendationsSection from "@/features/Home/ai-recommendations-section";
import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

export default function BookshopPage() {
  const chatBotRef = useRef<ChatBotRef>(null);

  const { data: booksData, isLoading: booksLoading } = useBooks({
    pageSize: 4,
  });

  const { data: categoriesData, isLoading: categoriesLoading } =
    useCategories();

  const featuredBooks = Array.isArray(booksData?.items)
    ? booksData.items.slice(0, 4)
    : [];

  const categories = Array.isArray(categoriesData)
    ? categoriesData.slice(0, 6)
    : [];

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow" id="main-content">
        <HeroSection />
        <FeaturedBooksSection books={featuredBooks} isLoading={booksLoading} />
        <CategoriesSection
          categories={categories}
          isLoading={categoriesLoading}
        />
        <AiRecommendationsSection chatBotRef={chatBotRef} />
      </main>
      <ChatBot ref={chatBotRef} />
      <Footer />
    </div>
  );
}
