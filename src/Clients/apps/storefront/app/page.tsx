"use client";

import { useRef } from "react";
import { useRouter } from "next/navigation";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { ChatBot, type ChatBotRef } from "@/components/chat-bot";
import { Button } from "@workspace/ui/components/button";
import { ArrowRight, BookOpen, Quote, MessageSquare } from "lucide-react";
import { BookCardSkeleton } from "@/components/book-card-skeleton";
import { BookCard } from "@/components/book-card";
import useBooks from "@workspace/api-hooks/catalog/books/useBooks";
import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";

export default function BookshopPage() {
  const router = useRouter();
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

  const hasFeaturedBooks = featuredBooks.length > 0;
  const hasCategories = categories.length > 0;

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow" id="main-content">
        {/* Hero Section */}
        <section
          className="py-20 md:py-32 container mx-auto px-4 text-center border-b"
          aria-labelledby="hero-heading"
        >
          <h1
            id="hero-heading"
            className="text-5xl md:text-7xl font-serif font-light mb-8 tracking-tight max-w-4xl mx-auto text-balance"
          >
            Great stories await your next chapter.
          </h1>
          <p className="text-lg md:text-xl text-muted-foreground mb-12 max-w-2xl mx-auto">
            Discover a curated collection of literature, design, and inspiration
            for the modern intellect.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button
              size="lg"
              className="rounded-full px-8"
              onClick={() => router.push("/shop")}
            >
              Browse Collection
            </Button>
          </div>
        </section>

        {/* Featured Books */}
        {hasFeaturedBooks ? (
          <section
            className="py-24 container mx-auto px-4"
            aria-labelledby="featured-heading"
          >
            <div className="flex items-end justify-between mb-12">
              <div>
                <h2
                  id="featured-heading"
                  className="text-3xl font-serif font-medium mb-2"
                >
                  Featured Books
                </h2>
                <p className="text-muted-foreground">
                  The most anticipated titles of the season.
                </p>
              </div>
              <Button
                variant="ghost"
                className="hidden md:flex gap-2"
                onClick={() => router.push("/shop")}
              >
                View All <ArrowRight className="size-4" />
              </Button>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-8">
              {booksLoading
                ? Array.from({ length: 4 }).map((_, i) => (
                    <BookCardSkeleton key={i} />
                  ))
                : featuredBooks.map((book) => (
                    <BookCard
                      key={book.id}
                      book={book}
                      onClick={() => router.push(`/shop/${book.id}`)}
                    />
                  ))}
            </div>
          </section>
        ) : (
          <section className="py-24 container mx-auto px-4">
            <div className="flex items-end justify-between mb-12">
              <div>
                <h2 className="text-3xl font-serif font-medium mb-2">
                  Featured Books
                </h2>
                <p className="text-muted-foreground">
                  The most anticipated titles of the season.
                </p>
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
              {Array.from({ length: 4 }).map((_, i) => (
                <BookCardSkeleton key={i} />
              ))}
            </div>
          </section>
        )}

        {/* Browse by Category */}
        {hasCategories ? (
          <section
            className="py-24 bg-secondary"
            aria-labelledby="category-heading"
          >
            <div className="container mx-auto px-4">
              <h2
                id="category-heading"
                className="text-3xl font-serif font-medium mb-12 text-center"
              >
                Browse by Category
              </h2>
              <nav
                className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4"
                aria-label="Book Categories"
              >
                {categoriesLoading
                  ? Array.from({ length: 6 }).map((_, i) => (
                      <div
                        key={i}
                        className="bg-background p-6 rounded-lg animate-pulse"
                      >
                        <div className="h-5 bg-muted rounded" />
                      </div>
                    ))
                  : categories.map((cat) => (
                      <a
                        key={cat.id}
                        href={`/shop?category=${encodeURIComponent(cat.id)}`}
                        className="bg-background p-6 rounded-lg text-center hover:shadow-md transition-all hover:-translate-y-1 group"
                      >
                        <h3 className="font-serif font-medium mb-1 group-hover:text-primary">
                          {cat.name}
                        </h3>
                      </a>
                    ))}
              </nav>
            </div>
          </section>
        ) : (
          <section className="py-24 bg-secondary text-center">
            <div className="container mx-auto px-4">
              <p className="text-muted-foreground text-lg">
                Categories will be available soon
              </p>
            </div>
          </section>
        )}

        {/* AI Recommendations Section */}
        <section
          className="py-24 container mx-auto px-4"
          aria-labelledby="ai-heading"
        >
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <div className="space-y-6">
              <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-bold uppercase tracking-widest">
                <BookOpen className="size-3" aria-hidden="true" />
                Personalized Discovery
              </div>
              <h2
                id="ai-heading"
                className="text-4xl md:text-5xl font-serif font-medium leading-tight text-balance"
              >
                Your Next Favorite Book is a Conversation Away.
              </h2>
              <p className="text-lg text-muted-foreground text-pretty">
                Not sure what to read next? Our AI literary assistant has read
                thousands of titles to help you find the perfect match for your
                current mood and interests.
              </p>
              <blockquote className="bg-secondary/50 p-6 rounded-2xl border border-primary/20 italic relative">
                <Quote
                  className="absolute -top-3 -left-3 size-8 text-primary/20"
                  aria-hidden="true"
                />
                <p className="text-muted-foreground">
                  "I was looking for something like 'The Art of Simplicity' but
                  with a more historical focus, and the BookWorm Assistant found
                  exactly what I needed in seconds."
                </p>
                <p className="mt-4 font-serif text-sm not-italic">
                  — Julian, Frequent Reader
                </p>
              </blockquote>
              <div className="pt-4 hidden lg:block">
                <Button
                  variant="outline"
                  className="rounded-full px-8 bg-transparent border-primary/20 hover:bg-primary/5"
                  onClick={() => chatBotRef.current?.openChat()}
                >
                  Try AI Recommendations
                </Button>
              </div>
            </div>
            <figure className="relative aspect-square rounded-2xl overflow-hidden shadow-2xl group">
              <img
                src="/aesthetic-book-journal-photography.jpg"
                alt="AI Book Recommendations - person writing in a journal surrounded by books"
                className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
              />
              <figcaption className="absolute inset-0 bg-linear-to-t from-black/60 via-black/0 to-transparent flex flex-col justify-end p-8">
                <div className="flex items-center gap-4 text-white">
                  <div className="size-12 rounded-full bg-primary flex items-center justify-center shadow-lg">
                    <MessageSquare className="size-6" aria-hidden="true" />
                  </div>
                  <div>
                    <p className="font-serif text-xl">BookWorm AI</p>
                    <p className="text-white/70 text-sm">
                      Available 24/7 for literary guidance
                    </p>
                  </div>
                </div>
              </figcaption>
            </figure>
          </div>
        </section>
      </main>
      <ChatBot ref={chatBotRef} />
      <Footer />
    </div>
  );
}
