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
];

export default function TimelineSection() {
  return (
    <section className="py-24 bg-secondary">
      <div className="container mx-auto px-4">
        <h2 className="text-4xl font-serif font-medium text-center mb-16">
          Our Journey
        </h2>
        <div className="max-w-3xl mx-auto space-y-12">
          {timeline.map((milestone, idx) => (
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
  );
}
