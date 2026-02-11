import { CheckCircle, RotateCcw } from "lucide-react";

import { Card, CardContent } from "@workspace/ui/components/card";

type Benefit = {
  readonly title: string;
  readonly description: string;
};

const benefits = [
  {
    title: "Full Refunds",
    description: "Get your money back, no questions asked",
  },
  {
    title: "Free Return Shipping",
    description: "We cover the cost of shipping your return",
  },
  {
    title: "Easy Exchange",
    description: "Swap for a different title at no cost",
  },
] as const satisfies readonly Benefit[];

const processSteps = [
  "Start a return from your account or contact support",
  "Receive a prepaid return shipping label",
  "Ship your books back in original condition",
  "Receive your refund or exchange within 5-7 business days",
] as const;

export default function PolicyOverview() {
  return (
    <section className="container mx-auto px-4 py-24">
      <div className="grid grid-cols-1 items-start gap-16 lg:grid-cols-2">
        <div className="space-y-8">
          <div>
            <h2 className="mb-6 font-serif text-4xl font-medium">
              30-Day Return Guarantee
            </h2>
            <p className="text-muted-foreground mb-4 text-lg leading-relaxed">
              We're confident you'll love your books. If you're not completely
              satisfied, we'll accept returns within 30 days of purchase for a
              full refund or exchange.
            </p>
          </div>
          <div className="space-y-4">
            {benefits.map((benefit) => (
              <div key={benefit.title} className="flex gap-4">
                <CheckCircle className="text-primary mt-1 size-6 shrink-0" />
                <div>
                  <h3 className="mb-1 font-medium">{benefit.title}</h3>
                  <p className="text-muted-foreground text-sm">
                    {benefit.description}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
        <Card className="bg-secondary border-none">
          <CardContent className="space-y-6 pt-8">
            <div className="space-y-3">
              <h3 className="flex items-center gap-2 font-serif text-lg font-medium">
                <RotateCcw className="text-primary size-5" />
                Return Process
              </h3>
              <ol className="text-muted-foreground space-y-3 text-sm">
                {processSteps.map((step, idx) => (
                  <li key={step} className="flex gap-3">
                    <span className="text-primary shrink-0 font-medium">
                      {idx + 1}.
                    </span>
                    <span>{step}</span>
                  </li>
                ))}
              </ol>
            </div>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}
