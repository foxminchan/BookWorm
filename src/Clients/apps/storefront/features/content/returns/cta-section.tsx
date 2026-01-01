import { Button } from "@workspace/ui/components/button";

export function CtaSection() {
  return (
    <section className="py-24 container mx-auto px-4 text-center">
      <h2 className="text-4xl font-serif font-medium mb-8">
        Need help with a return?
      </h2>
      <p className="text-lg text-muted-foreground mb-12 max-w-2xl mx-auto">
        Our customer support team is here to make the process as smooth as
        possible.
      </p>
      <a href="mailto:support@bookworm.com">
        <Button size="lg" className="rounded-full px-8">
          Contact Our Support Team
        </Button>
      </a>
    </section>
  );
}
