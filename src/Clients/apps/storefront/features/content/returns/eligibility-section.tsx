import { AlertCircle, CheckCircle } from "lucide-react";

const eligibleItems = [
  "Books in original condition",
  "Within 30 days of purchase",
  "Unopened or like-new condition",
  "With original packaging",
  "Not damaged by customer",
] as const;

const nonReturnableItems = [
  "Books with visible wear or damage",
  "Heavily marked or highlighted books",
  "Returns after 30 days",
  "Books purchased on final sale",
  "Digital or e-book purchases",
] as const;

export default function EligibilitySection() {
  return (
    <section className="bg-secondary py-24">
      <div className="container mx-auto px-4">
        <h2 className="mb-16 text-center font-serif text-4xl font-medium">
          Return Eligibility
        </h2>
        <div className="mx-auto grid max-w-3xl grid-cols-1 gap-8 md:grid-cols-2">
          <div className="space-y-4">
            <h3 className="flex items-center gap-2 font-serif text-lg font-medium text-green-700">
              <CheckCircle className="size-5" />
              Eligible for Return
            </h3>
            <ul className="text-muted-foreground space-y-2 text-sm">
              {eligibleItems.map((item) => (
                <li key={item}>✓ {item}</li>
              ))}
            </ul>
          </div>
          <div className="space-y-4">
            <h3 className="text-destructive flex items-center gap-2 font-serif text-lg font-medium">
              <AlertCircle className="size-5" />
              Non-Returnable Items
            </h3>
            <ul className="text-muted-foreground space-y-2 text-sm">
              {nonReturnableItems.map((item) => (
                <li key={item}>✗ {item}</li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
}
