const deliveryInfo = [
  {
    title: "Order Processing",
    description:
      "Orders are processed within 1-2 business days. You'll receive a confirmation email with tracking information as soon as your order ships.",
  },
  {
    title: "Tracking Your Order",
    description:
      "All shipments include tracking numbers so you can monitor your package every step of the way. Track your order anytime from your account dashboard.",
  },
  {
    title: "International Shipping",
    description:
      "We ship worldwide! International orders may be subject to customs duties and taxes. These will be calculated and displayed at checkout.",
  },
  {
    title: "Signature Required",
    description:
      "For orders over $100, signature may be required upon delivery. You'll be notified if this applies to your order.",
  },
];

export default function DeliveryInformation() {
  return (
    <section className="py-24 bg-secondary">
      <div className="container mx-auto px-4">
        <h2 className="text-4xl font-serif font-medium mb-12 text-center">
          Delivery Information
        </h2>
        <div className="max-w-3xl mx-auto space-y-8">
          {deliveryInfo.map((item, idx) => (
            <div
              key={idx}
              className="border-b border-primary/20 pb-8 last:border-b-0"
            >
              <h3 className="font-serif font-medium text-xl mb-3">
                {item.title}
              </h3>
              <p className="text-muted-foreground leading-relaxed">
                {item.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
