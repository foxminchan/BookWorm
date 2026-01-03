import Image from "next/image";
import Link from "next/link";

import { Button } from "@workspace/ui/components/button";

export default function MissionSection() {
  return (
    <section className="bg-secondary py-24">
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 items-center gap-16 lg:grid-cols-2">
          <div className="space-y-8">
            <div>
              <h2 className="mb-6 font-serif text-4xl leading-tight font-medium md:text-5xl">
                Curated for Readers, by Readers
              </h2>
              <p className="text-muted-foreground mb-4 text-lg leading-relaxed">
                BookWorm started with a vision: to create a haven where book
                lovers can explore a thoughtfully curated collection of titles
                across every genre. We handpick every book on our shelves,
                ensuring that quality and discovery go hand in hand.
              </p>
              <p className="text-muted-foreground text-lg leading-relaxed">
                From debut authors to literary classics, from practical guides
                to immersive fiction—we celebrate the diverse world of books and
                the transformative power of reading.
              </p>
            </div>
            <Button size="lg" className="w-fit rounded-full px-8" asChild>
              <Link href="/shop">Explore Our Collection</Link>
            </Button>
          </div>
          <div className="aspect-square overflow-hidden rounded-2xl shadow-lg">
            <Image
              src="/cozy-library-reader.png"
              alt="Reader enjoying a book"
              width={600}
              height={600}
              className="h-full w-full object-cover"
            />
          </div>
        </div>
      </div>
    </section>
  );
}
