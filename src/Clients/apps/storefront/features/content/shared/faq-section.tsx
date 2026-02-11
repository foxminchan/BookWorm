type FaqItem = {
  readonly question: string;
  readonly answer: string;
};

type FaqSectionProps = {
  readonly title: string;
  readonly items: readonly FaqItem[];
};

export default function FaqSection({ title, items }: FaqSectionProps) {
  return (
    <section className="bg-secondary py-24">
      <div className="container mx-auto px-4">
        <h2 className="mb-16 text-center font-serif text-4xl font-medium">
          {title}
        </h2>
        <div className="mx-auto max-w-2xl space-y-6">
          {items.map((item) => (
            <div
              key={item.question}
              className="border-primary/20 border-b pb-6 last:border-b-0"
            >
              <h3 className="mb-2 font-serif text-lg font-medium">
                {item.question}
              </h3>
              <p className="text-muted-foreground leading-relaxed">
                {item.answer}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
