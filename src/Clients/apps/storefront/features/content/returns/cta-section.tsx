import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

export default function CtaSection() {
  return (
    <section className="container mx-auto px-4 py-24 text-center">
      <h2 className="mb-8 font-serif text-4xl font-medium">
        Need help with a return?
      </h2>
      <p className="text-muted-foreground mx-auto mb-12 max-w-2xl text-lg">
        Our customer support team is here to make the process as smooth as
        possible.
      </p>
      <Link href="mailto:support@bookworm.com">
        <Button size="lg" className="rounded-full px-8">
          Contact Our Support Team
        </Button>
      </Link>
    </section>
  );
}
