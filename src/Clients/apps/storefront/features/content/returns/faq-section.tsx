const faqs = [
  {
    q: "What's your return window?",
    a: "We accept returns within 30 days of purchase. Make sure to initiate your return within this timeframe.",
  },
  {
    q: "Is return shipping free?",
    a: "Yes! We provide a prepaid return label so you don't pay anything to send your books back.",
  },
  {
    q: "How long does a refund take?",
    a: "Once we receive and inspect your return, refunds are processed within 5-7 business days.",
  },
  {
    q: "Can I exchange for a different book?",
    a: "You can exchange for any book of equal or greater value with no additional charge.",
  },
  {
    q: "What if my books arrive damaged?",
    a: "Contact us immediately with photos. We'll send a replacement or issue a full refund right away.",
  },
];

export default function FaqSection() {
  return (
    <section className="bg-secondary py-24">
      <div className="container mx-auto px-4">
        <h2 className="mb-16 text-center font-serif text-4xl font-medium">
          Return Questions
        </h2>
        <div className="mx-auto max-w-2xl space-y-6">
          {faqs.map((item, idx) => (
            <div
              key={idx}
              className="border-primary/20 border-b pb-6 last:border-b-0"
            >
              <h3 className="mb-2 font-serif text-lg font-medium">{item.q}</h3>
              <p className="text-muted-foreground leading-relaxed">{item.a}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
