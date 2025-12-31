"use client";

import { Button } from "@workspace/ui/components/button";
import { BookOpen, Quote, MessageSquare } from "lucide-react";
import type { ChatBotRef } from "@/components/chat-bot";

type AiRecommendationsSectionProps = {
  chatBotRef: React.RefObject<ChatBotRef | null>;
};

export default function AiRecommendationsSection({
  chatBotRef,
}: AiRecommendationsSectionProps) {
  return (
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
              "I was looking for something like 'The Art of Simplicity' but with
              a more historical focus, and the BookWorm Assistant found exactly
              what I needed in seconds."
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
  );
}
