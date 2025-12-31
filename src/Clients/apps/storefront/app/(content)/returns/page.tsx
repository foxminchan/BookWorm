"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";
import { RotateCcw, Clock, CheckCircle, AlertCircle } from "lucide-react";

export default function ReturnsPage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow">
        {/* Hero Section */}
        <section className="py-32 container mx-auto px-4 text-center border-b">
          <h1 className="text-6xl md:text-7xl font-serif font-light mb-8 tracking-tight max-w-5xl mx-auto text-balance">
            Hassle-Free Returns
          </h1>
          <p className="text-lg md:text-xl text-muted-foreground max-w-2xl mx-auto text-pretty">
            Not satisfied with your purchase? We make returns simple and
            straightforward.
          </p>
        </section>

        {/* Returns Policy Overview */}
        <section className="py-24 container mx-auto px-4">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-start">
            <div className="space-y-8">
              <div>
                <h2 className="text-4xl font-serif font-medium mb-6">
                  30-Day Return Guarantee
                </h2>
                <p className="text-lg text-muted-foreground mb-4 leading-relaxed">
                  We're confident you'll love your books. If you're not
                  completely satisfied, we'll accept returns within 30 days of
                  purchase for a full refund or exchange.
                </p>
              </div>
              <div className="space-y-4">
                <div className="flex gap-4">
                  <CheckCircle className="size-6 text-primary shrink-0 mt-1" />
                  <div>
                    <h3 className="font-medium mb-1">Full Refunds</h3>
                    <p className="text-muted-foreground text-sm">
                      Get your money back, no questions asked
                    </p>
                  </div>
                </div>
                <div className="flex gap-4">
                  <CheckCircle className="size-6 text-primary shrink-0 mt-1" />
                  <div>
                    <h3 className="font-medium mb-1">Free Return Shipping</h3>
                    <p className="text-muted-foreground text-sm">
                      We cover the cost of shipping your return
                    </p>
                  </div>
                </div>
                <div className="flex gap-4">
                  <CheckCircle className="size-6 text-primary shrink-0 mt-1" />
                  <div>
                    <h3 className="font-medium mb-1">Easy Exchange</h3>
                    <p className="text-muted-foreground text-sm">
                      Swap for a different title at no cost
                    </p>
                  </div>
                </div>
              </div>
            </div>
            <Card className="border-none bg-secondary">
              <CardContent className="pt-8 space-y-6">
                <div className="space-y-3">
                  <h3 className="font-serif font-medium text-lg flex items-center gap-2">
                    <RotateCcw className="size-5 text-primary" />
                    Return Process
                  </h3>
                  <ol className="space-y-3 text-muted-foreground text-sm">
                    <li className="flex gap-3">
                      <span className="font-medium text-primary shrink-0">
                        1.
                      </span>
                      <span>
                        Start a return from your account or contact support
                      </span>
                    </li>
                    <li className="flex gap-3">
                      <span className="font-medium text-primary shrink-0">
                        2.
                      </span>
                      <span>Receive a prepaid return shipping label</span>
                    </li>
                    <li className="flex gap-3">
                      <span className="font-medium text-primary shrink-0">
                        3.
                      </span>
                      <span>Ship your books back in original condition</span>
                    </li>
                    <li className="flex gap-3">
                      <span className="font-medium text-primary shrink-0">
                        4.
                      </span>
                      <span>
                        Receive your refund or exchange within 5-7 business days
                      </span>
                    </li>
                  </ol>
                </div>
              </CardContent>
            </Card>
          </div>
        </section>

        {/* Eligibility */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-serif font-medium text-center mb-16">
              Return Eligibility
            </h2>
            <div className="max-w-3xl mx-auto grid grid-cols-1 md:grid-cols-2 gap-8">
              <div className="space-y-4">
                <h3 className="font-serif font-medium text-lg flex items-center gap-2 text-green-700">
                  <CheckCircle className="size-5" />
                  Eligible for Return
                </h3>
                <ul className="space-y-2 text-muted-foreground text-sm">
                  <li>✓ Books in original condition</li>
                  <li>✓ Within 30 days of purchase</li>
                  <li>✓ Unopened or like-new condition</li>
                  <li>✓ With original packaging</li>
                  <li>✓ Not damaged by customer</li>
                </ul>
              </div>
              <div className="space-y-4">
                <h3 className="font-serif font-medium text-lg flex items-center gap-2 text-destructive">
                  <AlertCircle className="size-5" />
                  Non-Returnable Items
                </h3>
                <ul className="space-y-2 text-muted-foreground text-sm">
                  <li>✗ Books with visible wear or damage</li>
                  <li>✗ Heavily marked or highlighted books</li>
                  <li>✗ Returns after 30 days</li>
                  <li>✗ Books purchased on final sale</li>
                  <li>✗ Digital or e-book purchases</li>
                </ul>
              </div>
            </div>
          </div>
        </section>

        {/* Timeline */}
        <section className="py-24 container mx-auto px-4">
          <h2 className="text-4xl font-serif font-medium text-center mb-16">
            Refund Timeline
          </h2>
          <div className="max-w-2xl mx-auto space-y-6">
            {[
              {
                icon: Clock,
                title: "Initiate Return",
                timeline: "Day 1",
                description: "Start your return request from your account",
              },
              {
                icon: RotateCcw,
                title: "Ship Your Books",
                timeline: "Days 2-5",
                description:
                  "Use the prepaid label to ship your books back to us",
              },
              {
                icon: CheckCircle,
                title: "We Receive Your Books",
                timeline: "Days 5-10",
                description: "We inspect and process your return",
              },
              {
                icon: Clock,
                title: "Refund Issued",
                timeline: "Days 10-15",
                description:
                  "Refund appears in your account (5-7 business days after approval)",
              },
            ].map((step, idx) => {
              const Icon = step.icon;
              return (
                <div key={idx} className="flex gap-6">
                  <div className="flex flex-col items-center">
                    <Icon className="size-8 text-primary mb-2" />
                    {idx < 3 && <div className="w-1 h-12 bg-border" />}
                  </div>
                  <div className="grow pb-8">
                    <div className="flex items-baseline gap-3 mb-2">
                      <h3 className="font-serif font-medium text-lg">
                        {step.title}
                      </h3>
                      <span className="text-sm text-muted-foreground">
                        {step.timeline}
                      </span>
                    </div>
                    <p className="text-muted-foreground leading-relaxed">
                      {step.description}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </section>

        {/* FAQ */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-serif font-medium text-center mb-16">
              Return Questions
            </h2>
            <div className="max-w-2xl mx-auto space-y-6">
              {[
                {
                  q: "What's your return window?",
                  a: "We accept returns within 30 days of purchase. Make sure to initiate your return within this timeframe.",
                },
                {
                  q: "Is return shipping free?",
                  a: "Yes! We provide a prepaid return label so you don't pay anything to send your books back.",
                },
                {
                  q: "How long does a refund take?",
                  a: "Once we receive and inspect your return, refunds are processed within 5-7 business days.",
                },
                {
                  q: "Can I exchange for a different book?",
                  a: "You can exchange for any book of equal or greater value with no additional charge.",
                },
                {
                  q: "What if my books arrive damaged?",
                  a: "Contact us immediately with photos. We'll send a replacement or issue a full refund right away.",
                },
              ].map((item, idx) => (
                <div
                  key={idx}
                  className="border-b border-primary/20 pb-6 last:border-b-0"
                >
                  <h3 className="font-serif font-medium text-lg mb-2">
                    {item.q}
                  </h3>
                  <p className="text-muted-foreground leading-relaxed">
                    {item.a}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* CTA */}
        <section className="py-24 container mx-auto px-4 text-center">
          <h2 className="text-4xl font-serif font-medium mb-8">
            Need help with a return?
          </h2>
          <p className="text-lg text-muted-foreground mb-12 max-w-2xl mx-auto">
            Our customer support team is here to make the process as smooth as
            possible.
          </p>
          <a href="mailto:support@bookworm.com">
            <Button size="lg" className="rounded-full px-8">
              Contact Our Support Team
            </Button>
          </a>
        </section>
      </main>
      <Footer />
    </div>
  );
}
