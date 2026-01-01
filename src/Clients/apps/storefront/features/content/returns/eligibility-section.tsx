import { CheckCircle, AlertCircle } from "lucide-react";

const eligibleItems = [
  "Books in original condition",
  "Within 30 days of purchase",
  "Unopened or like-new condition",
  "With original packaging",
  "Not damaged by customer",
];

const nonReturnableItems = [
  "Books with visible wear or damage",
  "Heavily marked or highlighted books",
  "Returns after 30 days",
  "Books purchased on final sale",
  "Digital or e-book purchases",
];

export default function EligibilitySection() {
  return (
    <section className="py-24 bg-secondary">
      <div className="container mx-auto px-4">
        <h2 className="text-4xl font-serif font-medium text-center mb-16">
          Return Eligibility
        </h2>
        <div className="max-w-3xl mx-auto grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="space-y-4">
            <h3 className="font-serif font-medium text-lg flex items-center gap-2 text-green-700">
              <CheckCircle className="size-5" />
              Eligible for Return
            </h3>
            <ul className="space-y-2 text-muted-foreground text-sm">
              {eligibleItems.map((item, idx) => (
                <li key={idx}>✓ {item}</li>
              ))}
            </ul>
          </div>
          <div className="space-y-4">
            <h3 className="font-serif font-medium text-lg flex items-center gap-2 text-destructive">
              <AlertCircle className="size-5" />
              Non-Returnable Items
            </h3>
            <ul className="space-y-2 text-muted-foreground text-sm">
              {nonReturnableItems.map((item, idx) => (
                <li key={idx}>✗ {item}</li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
}
