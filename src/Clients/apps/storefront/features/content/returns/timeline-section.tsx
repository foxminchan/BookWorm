import { CheckCircle, Clock, RotateCcw } from "lucide-react";
import type { LucideIcon } from "lucide-react";

type TimelineStep = {
  readonly icon: LucideIcon;
  readonly title: string;
  readonly timeline: string;
  readonly description: string;
};

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
] as const satisfies readonly TimelineStep[];

export default function TimelineSection() {
  return (
    <section className="container mx-auto px-4 py-24">
      <h2 className="mb-16 text-center font-serif text-4xl font-medium">
        Refund Timeline
      </h2>
      <div className="mx-auto max-w-2xl space-y-6">
        {timelineSteps.map((step, idx) => {
          const Icon = step.icon;
          return (
            <div key={step.title} className="flex gap-6">
              <div className="flex flex-col items-center">
                <Icon className="text-primary mb-2 size-8" />
                {idx < timelineSteps.length - 1 ? (
                  <div className="bg-border h-12 w-1" />
                ) : null}
              </div>
              <div className="grow pb-8">
                <div className="mb-2 flex items-baseline gap-3">
                  <h3 className="font-serif text-lg font-medium">
                    {step.title}
                  </h3>
                  <span className="text-muted-foreground text-sm">
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
