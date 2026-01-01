const faqs = [
  {
    q: "Do you offer free shipping?",
    a: "Yes! Standard shipping is free on all orders over $50. Orders under $50 are $5.99 for standard shipping.",
  },
  {
    q: "How can I track my order?",
    a: "You'll receive a tracking number via email once your order ships. You can use this to track your package on the carrier's website.",
  },
  {
    q: "What if I need my books urgently?",
    a: "We offer Express (2-3 days) and Overnight shipping options for urgent orders. Select these at checkout.",
  },
  {
    q: "Can you ship to a P.O. Box?",
    a: "Unfortunately, we can only ship to physical street addresses at this time. Please provide your home or business address.",
  },
];

export default function FaqSection() {
  return (
    <section className="py-24 bg-secondary">
      <div className="container mx-auto px-4">
        <h2 className="text-4xl font-serif font-medium text-center mb-16">
          Frequently Asked Questions
        </h2>
        <div className="max-w-2xl mx-auto space-y-6">
          {faqs.map((item, idx) => (
            <div
              key={idx}
              className="border-b border-primary/20 pb-6 last:border-b-0"
            >
              <h3 className="font-serif font-medium text-lg mb-2">{item.q}</h3>
              <p className="text-muted-foreground leading-relaxed">{item.a}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
