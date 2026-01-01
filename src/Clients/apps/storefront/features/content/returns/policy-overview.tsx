import { Card, CardContent } from "@workspace/ui/components/card";
import { RotateCcw, CheckCircle } from "lucide-react";

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
];

const processSteps = [
  "Start a return from your account or contact support",
  "Receive a prepaid return shipping label",
  "Ship your books back in original condition",
  "Receive your refund or exchange within 5-7 business days",
];

export default function PolicyOverview() {
  return (
    <section className="py-24 container mx-auto px-4">
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-start">
        <div className="space-y-8">
          <div>
            <h2 className="text-4xl font-serif font-medium mb-6">
              30-Day Return Guarantee
            </h2>
            <p className="text-lg text-muted-foreground mb-4 leading-relaxed">
              We're confident you'll love your books. If you're not completely
              satisfied, we'll accept returns within 30 days of purchase for a
              full refund or exchange.
            </p>
          </div>
          <div className="space-y-4">
            {benefits.map((benefit, idx) => (
              <div key={idx} className="flex gap-4">
                <CheckCircle className="size-6 text-primary shrink-0 mt-1" />
                <div>
                  <h3 className="font-medium mb-1">{benefit.title}</h3>
                  <p className="text-muted-foreground text-sm">
                    {benefit.description}
                  </p>
                </div>
              </div>
            ))}
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
                {processSteps.map((step, idx) => (
                  <li key={idx} className="flex gap-3">
                    <span className="font-medium text-primary shrink-0">
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
