type Milestone = {
  readonly year: string;
  readonly title: string;
  readonly description: string;
};

const timeline = [
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
] as const satisfies readonly Milestone[];

export default function TimelineSection() {
  return (
    <section className="bg-secondary py-24">
      <div className="container mx-auto px-4">
        <h2 className="mb-16 text-center font-serif text-4xl font-medium">
          Our Journey
        </h2>
        <div className="mx-auto max-w-3xl space-y-12">
          {timeline.map((milestone) => (
            <div key={milestone.year} className="flex items-start gap-8">
              <div className="min-w-24">
                <span className="text-primary font-serif text-2xl font-bold">
                  {milestone.year}
                </span>
              </div>
              <div className="border-primary/20 grow border-b pb-8 last:border-b-0">
                <h3 className="mb-2 font-serif text-xl font-medium">
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
  );
}
