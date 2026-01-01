import { Button } from "@workspace/ui/components/button";

const content = [
  "While we take every precaution to ensure your books arrive safely, accidents do happen. If your package arrives damaged or goes missing, contact our customer support team immediately.",
  "We'll work quickly to resolve the issue—whether that means reshipping your order, issuing a refund, or finding a solution that works for you.",
];

export function DamagedPackages() {
  return (
    <section className="py-24 container mx-auto px-4">
      <div className="max-w-3xl mx-auto">
        <h2 className="text-4xl font-serif font-medium mb-8">
          Damaged or Lost Packages?
        </h2>
        <div className="bg-secondary p-8 rounded-lg space-y-4">
          {content.map((text, idx) => (
            <p key={idx} className="text-muted-foreground leading-relaxed">
              {text}
            </p>
          ))}
          <a href="mailto:support@bookworm.com">
            <Button size="lg" className="rounded-full px-8 mt-4">
              Contact Support
            </Button>
          </a>
        </div>
      </div>
    </section>
  );
}
