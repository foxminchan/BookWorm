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
    <div className="border-border/40 grid grid-cols-1 gap-12 border-t pt-16 md:grid-cols-2">
      {INFO_SECTIONS.map((section) => (
        <div key={section.title}>
          <h3 className="mb-4 font-serif text-2xl">{section.title}</h3>
          <p className="text-muted-foreground leading-relaxed">
            {section.description}
          </p>
        </div>
      ))}
    </div>
  );
}
