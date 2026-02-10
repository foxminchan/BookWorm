import Link from "next/link";

import { ArrowRight } from "lucide-react";

type PublisherCardProps = {
  id: string;
  name: string;
};

export default function PublisherCard({ id, name }: Readonly<PublisherCardProps>) {
  return (
    <Link
      href={`/shop?publisher=${id}`}
      className="group from-secondary/10 to-secondary/5 border-border/40 hover:border-primary/40 relative flex aspect-square flex-col justify-between overflow-hidden rounded-lg border bg-linear-to-br p-8 transition-all duration-500 hover:shadow-lg"
    >
      <div className="from-primary/5 absolute inset-0 bg-linear-to-br to-transparent opacity-0 transition-opacity duration-500 group-hover:opacity-100" />

      <div className="relative z-10">
        <div className="bg-primary/10 border-primary/20 mb-4 inline-block rounded-full border px-3 py-1">
          <span className="text-primary font-mono text-xs tracking-widest uppercase">
            Publisher
          </span>
        </div>
      </div>

      <div className="relative z-10 space-y-4">
        <h2 className="font-serif text-2xl font-medium transition-transform duration-500 group-hover:translate-x-1 md:text-3xl">
          {name}
        </h2>

        <div className="text-muted-foreground group-hover:text-primary inline-flex items-center gap-2 text-sm transition-colors">
          <span>Explore</span>
          <ArrowRight className="size-4 transition-transform duration-500 group-hover:translate-x-2" aria-hidden="true" />
        </div>
      </div>
    </Link>
  );
}
