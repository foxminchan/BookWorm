"use client";

const INFO_SECTIONS = [
  {
    title: "Why Publishers Matter",
    description:
      "Publishing houses are more than distributors—they're curators of human expression. Each brings distinctive editorial vision and commitment to authors and readers alike.",
  },
  {
    title: "Find Your Next Read",
    description:
      "Browse books from your favorite publishers. Discover how each house shapes the literary landscape through their unique selections and commitments.",
  },
];

export default function PublishersInfoSection() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-12 border-t border-border/40 pt-16">
      {INFO_SECTIONS.map((section) => (
        <div key={section.title}>
          <h3 className="text-2xl font-serif mb-4">{section.title}</h3>
          <p className="text-muted-foreground leading-relaxed">
            {section.description}
          </p>
        </div>
      ))}
    </div>
  );
}
