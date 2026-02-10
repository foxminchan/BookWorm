import FaqSection from "@/features/content/shared/faq-section";

const faqs = [
  {
    question: "What's your return window?",
    answer:
      "We accept returns within 30 days of purchase. Make sure to initiate your return within this timeframe.",
  },
  {
    question: "Is return shipping free?",
    answer:
      "Yes! We provide a prepaid return label so you don't pay anything to send your books back.",
  },
  {
    question: "How long does a refund take?",
    answer:
      "Once we receive and inspect your return, refunds are processed within 5-7 business days.",
  },
  {
    question: "Can I exchange for a different book?",
    answer:
      "You can exchange for any book of equal or greater value with no additional charge.",
  },
  {
    question: "What if my books arrive damaged?",
    answer:
      "Contact us immediately with photos. We'll send a replacement or issue a full refund right away.",
  },
] as const;

export default function ReturnsFaqSection() {
  return <FaqSection title="Return Questions" items={faqs} />;
}
