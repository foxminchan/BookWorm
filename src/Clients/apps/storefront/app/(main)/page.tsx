"use client";

import { useRef } from "react";

import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

import { ChatBot, type ChatBotRef } from "@/components/chat-bot";
import AiRecommendationsSection from "@/features/home/ai-recommendations-section";
import CategoriesSection from "@/features/home/categories-section";
import FeaturedBooksSection from "@/features/home/featured-books-section";
import HeroSection from "@/features/home/hero-section";

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
    <>
      <HeroSection />
      <FeaturedBooksSection books={featuredBooks} isLoading={booksLoading} />
      <CategoriesSection
        categories={categories}
        isLoading={categoriesLoading}
      />
      <AiRecommendationsSection chatBotRef={chatBotRef} />
      <ChatBot ref={chatBotRef} />
    </>
  );
}
