"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";
import { BookOpen, Heart, Globe, Users } from "lucide-react";
import Image from "next/image";
import Link from "next/link";

export default function AboutPage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow">
        {/* Hero Section */}
        <section className="py-32 container mx-auto px-4 text-center border-b">
          <h1 className="text-6xl md:text-7xl font-serif font-light mb-8 tracking-tight max-w-5xl mx-auto text-balance">
            The story behind BookWorm
          </h1>
          <p className="text-lg md:text-xl text-muted-foreground max-w-2xl mx-auto text-pretty">
            We believe every reader deserves to discover their next great read.
            BookWorm was born from a simple passion: connecting people with
            stories that change lives.
          </p>
        </section>

        {/* Our Mission */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
              <div className="space-y-8">
                <div>
                  <h2 className="text-4xl md:text-5xl font-serif font-medium mb-6 leading-tight">
                    Curated for Readers, by Readers
                  </h2>
                  <p className="text-lg text-muted-foreground mb-4 leading-relaxed">
                    BookWorm started with a vision: to create a haven where book
                    lovers can explore a thoughtfully curated collection of
                    titles across every genre. We handpick every book on our
                    shelves, ensuring that quality and discovery go hand in
                    hand.
                  </p>
                  <p className="text-lg text-muted-foreground leading-relaxed">
                    From debut authors to literary classics, from practical
                    guides to immersive fiction—we celebrate the diverse world
                    of books and the transformative power of reading.
                  </p>
                </div>
                <Button size="lg" className="rounded-full px-8 w-fit" asChild>
                  <Link href="/shop">Explore Our Collection</Link>
                </Button>
              </div>
              <div className="aspect-square rounded-2xl overflow-hidden shadow-lg">
                <Image
                  src="/cozy-library-reader.png"
                  alt="Reader enjoying a book"
                  width={600}
                  height={600}
                  className="w-full h-full object-cover"
                />
              </div>
            </div>
          </div>
        </section>

        {/* Values Section */}
        <section className="py-24 container mx-auto px-4">
          <h2 className="text-4xl font-serif font-medium text-center mb-16">
            What we stand for
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {[
              {
                icon: BookOpen,
                title: "Quality",
                description:
                  "Every book is selected for its literary merit and ability to inspire readers",
              },
              {
                icon: Heart,
                title: "Passion",
                description:
                  "We read, love, and share the stories that resonate with our community",
              },
              {
                icon: Globe,
                title: "Diversity",
                description:
                  "We celebrate voices from around the world and perspectives that challenge us",
              },
              {
                icon: Users,
                title: "Community",
                description:
                  "We're building a space where readers connect and share their love of books",
              },
            ].map((value, idx) => {
              const Icon = value.icon;
              return (
                <Card
                  key={idx}
                  className="border-none bg-secondary hover:shadow-lg transition-all hover:-translate-y-1"
                >
                  <CardContent className="pt-8 space-y-4 text-center">
                    <Icon className="size-12 text-primary mx-auto" />
                    <h3 className="font-serif font-medium text-lg">
                      {value.title}
                    </h3>
                    <p className="text-muted-foreground text-sm leading-relaxed">
                      {value.description}
                    </p>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </section>

        {/* Timeline */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-serif font-medium text-center mb-16">
              Our Journey
            </h2>
            <div className="max-w-3xl mx-auto space-y-12">
              {[
                {
                  year: "2024",
                  title: "The Idea",
                  description:
                    "BookWorm begins as a vision to create a curated online bookstore that puts quality and discovery first.",
                },
                {
                  year: "2025",
                  title: "Complete Core Business",
                  description:
                    "We launch BookWorm with a curated collection, user reviews, publisher partnerships, and a robust e-commerce platform serving thousands of readers.",
                },
                {
                  year: "2026",
                  title: "AI Agent Integration",
                  description:
                    "We introduce advanced AI agents to personalize recommendations, predict reading trends, and revolutionize how readers discover their next great book.",
                },
              ].map((milestone, idx) => (
                <div key={idx} className="flex gap-8 items-start">
                  <div className="min-w-24">
                    <span className="text-2xl font-serif font-bold text-primary">
                      {milestone.year}
                    </span>
                  </div>
                  <div className="grow pb-8 border-b border-primary/20 last:border-b-0">
                    <h3 className="font-serif font-medium text-xl mb-2">
                      {milestone.title}
                    </h3>
                    <p className="text-muted-foreground leading-relaxed">
                      {milestone.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Get In Touch */}
        <section className="py-24 container mx-auto px-4 text-center">
          <h2 className="text-4xl md:text-5xl font-serif font-medium mb-8">
            Get In Touch
          </h2>
          <p className="text-lg text-muted-foreground mb-12 max-w-2xl mx-auto">
            Have questions or want to share your reading journey with us? We'd
            love to hear from you.
          </p>
          <Button size="lg" className="rounded-full px-8" asChild>
            <a href="mailto:support@bookworm.com">Get In Touch</a>
          </Button>
        </section>
      </main>
      <Footer />
    </div>
  );
}
