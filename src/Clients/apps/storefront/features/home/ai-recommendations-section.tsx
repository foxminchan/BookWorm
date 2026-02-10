"use client";

import Image from "next/image";

import { BookOpen, MessageSquare, Quote } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

import type { ChatBotRef } from "@/components/chat-bot";

type AiRecommendationsSectionProps = {
  readonly chatBotRef: React.RefObject<ChatBotRef | null>;
};

export default function AiRecommendationsSection({
  chatBotRef,
}: AiRecommendationsSectionProps) {
  return (
    <section
      className="container mx-auto px-4 py-24"
      aria-labelledby="ai-heading"
    >
      <div className="grid grid-cols-1 items-center gap-12 lg:grid-cols-2">
        <div className="space-y-6">
          <div className="bg-primary/10 text-primary inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-bold tracking-widest uppercase">
            <BookOpen className="size-3" aria-hidden="true" />
            Personalized Discovery
          </div>
          <h2
            id="ai-heading"
            className="font-serif text-4xl leading-tight font-medium text-balance md:text-5xl"
          >
            Your Next Favorite Book is a Conversation Away.
          </h2>
          <p className="text-muted-foreground text-lg text-pretty">
            Not sure what to read next? Our AI literary assistant has read
            thousands of titles to help you find the perfect match for your
            current mood and interests.
          </p>
          <blockquote className="bg-secondary/50 border-primary/20 relative rounded-2xl border p-6 italic">
            <Quote
              className="text-primary/20 absolute -top-3 -left-3 size-8"
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
          <div className="hidden pt-4 lg:block">
            <Button
              type="button"
              variant="outline"
              className="border-primary/20 hover:bg-primary/5 rounded-full bg-transparent px-8"
              onClick={() => chatBotRef.current?.openChat()}
              aria-label="Try AI book recommendations"
            >
              Try AI Recommendations
            </Button>
          </div>
        </div>
        <figure className="group relative aspect-square overflow-hidden rounded-2xl shadow-2xl">
          <Image
            src="/aesthetic-book-journal-photography.jpg"
            alt="AI Book Recommendations - person writing in a journal surrounded by books"
            fill
            className="object-cover transition-transform duration-700 group-hover:scale-105"
            priority
          />
          <figcaption className="absolute inset-0 flex flex-col justify-end bg-linear-to-t from-black/60 via-black/0 to-transparent p-8">
            <div className="flex items-center gap-4 text-white">
              <div className="bg-primary flex size-12 items-center justify-center rounded-full shadow-lg">
                <MessageSquare className="size-6" aria-hidden="true" />
              </div>
              <div>
                <p className="font-serif text-xl">BookWorm AI</p>
                <p className="text-sm text-white/70">
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
