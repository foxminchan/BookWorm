import { Card, CardContent } from "@workspace/ui/components/card";
import { BookOpen, Heart, Globe, Users } from "lucide-react";

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
];

export default function ValuesSection() {
  return (
    <section className="py-24 container mx-auto px-4">
      <h2 className="text-4xl font-serif font-medium text-center mb-16">
        What we stand for
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        {values.map((value, idx) => {
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
  );
}
