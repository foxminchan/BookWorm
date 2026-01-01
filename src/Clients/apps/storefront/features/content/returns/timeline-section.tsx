import { Clock, RotateCcw, CheckCircle } from "lucide-react";

const timelineSteps = [
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
    description: "Use the prepaid label to ship your books back to us",
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
];

export function TimelineSection() {
  return (
    <section className="py-24 container mx-auto px-4">
      <h2 className="text-4xl font-serif font-medium text-center mb-16">
        Refund Timeline
      </h2>
      <div className="max-w-2xl mx-auto space-y-6">
        {timelineSteps.map((step, idx) => {
          const Icon = step.icon;
          return (
            <div key={idx} className="flex gap-6">
              <div className="flex flex-col items-center">
                <Icon className="size-8 text-primary mb-2" />
                {idx < timelineSteps.length - 1 && (
                  <div className="w-1 h-12 bg-border" />
                )}
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
  );
}
