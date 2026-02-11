import { BookOpen, Globe, Heart, Users } from "lucide-react";
import type { LucideIcon } from "lucide-react";

import { Card, CardContent } from "@workspace/ui/components/card";

type Value = {
  readonly icon: LucideIcon;
  readonly title: string;
  readonly description: string;
};

const values = [
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
] as const satisfies readonly Value[];

export default function ValuesSection() {
  return (
    <section className="container mx-auto px-4 py-24">
      <h2 className="mb-16 text-center font-serif text-4xl font-medium">
        What we stand for
      </h2>
      <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4">
        {values.map((value) => {
          const Icon = value.icon;
          return (
            <Card
              key={value.title}
              className="bg-secondary border-none transition-all hover:-translate-y-1 hover:shadow-lg"
            >
              <CardContent className="space-y-4 pt-8 text-center">
                <Icon className="text-primary mx-auto size-12" />
                <h3 className="font-serif text-lg font-medium">
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
  );
}
