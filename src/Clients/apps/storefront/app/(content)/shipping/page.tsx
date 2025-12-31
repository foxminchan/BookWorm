"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Card, CardContent } from "@workspace/ui/components/card";
import { Truck, Clock, Globe, Package } from "lucide-react";

export default function ShippingPage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow">
        {/* Hero Section */}
        <section className="py-32 container mx-auto px-4 text-center border-b">
          <h1 className="text-6xl md:text-7xl font-serif font-light mb-8 tracking-tight max-w-5xl mx-auto text-balance">
            Fast, Reliable Shipping
          </h1>
          <p className="text-lg md:text-xl text-muted-foreground max-w-2xl mx-auto text-pretty">
            We're committed to getting your books to your doorstep quickly and
            safely. Free shipping on orders over $50.
          </p>
        </section>

        {/* Free Shipping Banner */}
        <section className="py-16 bg-primary text-primary-foreground">
          <div className="container mx-auto px-4 text-center">
            <h2 className="text-3xl md:text-4xl font-serif font-medium mb-4">
              Free Shipping on Orders Over $50
            </h2>
            <p className="text-lg opacity-90 max-w-xl mx-auto">
              No hidden fees. No minimum order requirements for standard
              shipping. Just books, delivered to you.
            </p>
          </div>
        </section>

        {/* Shipping Options */}
        <section className="py-24 container mx-auto px-4">
          <h2 className="text-4xl font-serif font-medium mb-16 text-center">
            Shipping Options
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {[
              {
                icon: Package,
                title: "Standard Shipping",
                time: "5-7 Business Days",
                price: "Free on orders over $50",
              },
              {
                icon: Clock,
                title: "Express Shipping",
                time: "2-3 Business Days",
                price: "$9.99",
              },
              {
                icon: Truck,
                title: "Overnight Shipping",
                time: "Next Business Day",
                price: "$24.99",
              },
              {
                icon: Globe,
                title: "International",
                time: "10-21 Business Days",
                price: "Calculated at checkout",
              },
            ].map((option, idx) => {
              const Icon = option.icon;
              return (
                <Card
                  key={idx}
                  className="border-none bg-secondary hover:shadow-lg transition-all"
                >
                  <CardContent className="pt-8 space-y-4 text-center">
                    <Icon className="size-12 text-primary mx-auto" />
                    <h3 className="font-serif font-medium text-lg">
                      {option.title}
                    </h3>
                    <div className="space-y-2">
                      <p className="text-sm text-muted-foreground">
                        {option.time}
                      </p>
                      <p className="font-medium text-primary">{option.price}</p>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </section>

        {/* Delivery Information */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-serif font-medium mb-12 text-center">
              Delivery Information
            </h2>
            <div className="max-w-3xl mx-auto space-y-8">
              {[
                {
                  title: "Order Processing",
                  description:
                    "Orders are processed within 1-2 business days. You'll receive a confirmation email with tracking information as soon as your order ships.",
                },
                {
                  title: "Tracking Your Order",
                  description:
                    "All shipments include tracking numbers so you can monitor your package every step of the way. Track your order anytime from your account dashboard.",
                },
                {
                  title: "International Shipping",
                  description:
                    "We ship worldwide! International orders may be subject to customs duties and taxes. These will be calculated and displayed at checkout.",
                },
                {
                  title: "Signature Required",
                  description:
                    "For orders over $100, signature may be required upon delivery. You'll be notified if this applies to your order.",
                },
              ].map((item, idx) => (
                <div
                  key={idx}
                  className="border-b border-primary/20 pb-8 last:border-b-0"
                >
                  <h3 className="font-serif font-medium text-xl mb-3">
                    {item.title}
                  </h3>
                  <p className="text-muted-foreground leading-relaxed">
                    {item.description}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Damaged or Lost Packages */}
        <section className="py-24 container mx-auto px-4">
          <div className="max-w-3xl mx-auto">
            <h2 className="text-4xl font-serif font-medium mb-8">
              Damaged or Lost Packages?
            </h2>
            <div className="bg-secondary p-8 rounded-lg space-y-4">
              <p className="text-muted-foreground leading-relaxed">
                While we take every precaution to ensure your books arrive
                safely, accidents do happen. If your package arrives damaged or
                goes missing, contact our customer support team immediately.
              </p>
              <p className="text-muted-foreground leading-relaxed">
                We'll work quickly to resolve the issue—whether that means
                reshipping your order, issuing a refund, or finding a solution
                that works for you.
              </p>
              <a href="mailto:support@bookworm.com">
                <Button size="lg" className="rounded-full px-8 mt-4">
                  Contact Support
                </Button>
              </a>
            </div>
          </div>
        </section>

        {/* FAQ Section */}
        <section className="py-24 bg-secondary">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-serif font-medium text-center mb-16">
              Frequently Asked Questions
            </h2>
            <div className="max-w-2xl mx-auto space-y-6">
              {[
                {
                  q: "Do you offer free shipping?",
                  a: "Yes! Standard shipping is free on all orders over $50. Orders under $50 are $5.99 for standard shipping.",
                },
                {
                  q: "How can I track my order?",
                  a: "You'll receive a tracking number via email once your order ships. You can use this to track your package on the carrier's website.",
                },
                {
                  q: "What if I need my books urgently?",
                  a: "We offer Express (2-3 days) and Overnight shipping options for urgent orders. Select these at checkout.",
                },
                {
                  q: "Can you ship to a P.O. Box?",
                  a: "Unfortunately, we can only ship to physical street addresses at this time. Please provide your home or business address.",
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
      </main>
      <Footer />
    </div>
  );
}
