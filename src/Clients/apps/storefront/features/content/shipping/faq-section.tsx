import FaqSection from "@/features/content/shared/faq-section";

const faqs = [
  {
    question: "Do you offer free shipping?",
    answer:
      "Yes! Standard shipping is free on all orders over $50. Orders under $50 are $5.99 for standard shipping.",
  },
  {
    question: "How can I track my order?",
    answer:
      "You'll receive a tracking number via email once your order ships. You can use this to track your package on the carrier's website.",
  },
  {
    question: "What if I need my books urgently?",
    answer:
      "We offer Express (2-3 days) and Overnight shipping options for urgent orders. Select these at checkout.",
  },
  {
    question: "Can you ship to a P.O. Box?",
    answer:
      "Unfortunately, we can only ship to physical street addresses at this time. Please provide your home or business address.",
  },
] as const;

export default function ShippingFaqSection() {
  return <FaqSection title="Frequently Asked Questions" items={faqs} />;
}
