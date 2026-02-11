import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

type CtaSectionProps = {
  readonly title: string;
  readonly description: string;
  readonly href: string;
  readonly label: string;
};

export default function CtaSection({
  title,
  description,
  href,
  label,
}: CtaSectionProps) {
  return (
    <section className="container mx-auto px-4 py-24 text-center">
      <h2 className="mb-8 font-serif text-4xl font-medium md:text-5xl">
        {title}
      </h2>
      <p className="text-muted-foreground mx-auto mb-12 max-w-2xl text-lg">
        {description}
      </p>
      <Button size="lg" className="rounded-full px-8" asChild>
        <Link href={href}>{label}</Link>
      </Button>
    </section>
  );
}
