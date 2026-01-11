import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

const content = [
  "While we take every precaution to ensure your books arrive safely, accidents do happen. If your package arrives damaged or goes missing, contact our customer support team immediately.",
  "We'll work quickly to resolve the issue—whether that means reshipping your order, issuing a refund, or finding a solution that works for you.",
];

export default function DamagedPackages() {
  return (
    <section className="container mx-auto px-4 py-24">
      <div className="mx-auto max-w-3xl">
        <h2 className="mb-8 font-serif text-4xl font-medium">
          Damaged or Lost Packages?
        </h2>
        <div className="bg-secondary space-y-4 rounded-lg p-8">
          {content.map((text, idx) => (
            <p key={idx} className="text-muted-foreground leading-relaxed">
              {text}
            </p>
          ))}
          <Link href="mailto:support@bookworm.com">
            <Button size="lg" className="mt-4 rounded-full px-8">
              Contact Support
            </Button>
          </Link>
        </div>
      </div>
    </section>
  );
}
