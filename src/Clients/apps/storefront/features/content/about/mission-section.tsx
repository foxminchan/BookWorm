import { Button } from "@workspace/ui/components/button";
import Image from "next/image";
import Link from "next/link";

export default function MissionSection() {
  return (
    <section className="py-24 bg-secondary">
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
          <div className="space-y-8">
            <div>
              <h2 className="text-4xl md:text-5xl font-serif font-medium mb-6 leading-tight">
                Curated for Readers, by Readers
              </h2>
              <p className="text-lg text-muted-foreground mb-4 leading-relaxed">
                BookWorm started with a vision: to create a haven where book
                lovers can explore a thoughtfully curated collection of titles
                across every genre. We handpick every book on our shelves,
                ensuring that quality and discovery go hand in hand.
              </p>
              <p className="text-lg text-muted-foreground leading-relaxed">
                From debut authors to literary classics, from practical guides
                to immersive fiction—we celebrate the diverse world of books and
                the transformative power of reading.
              </p>
            </div>
            <Button size="lg" className="rounded-full px-8 w-fit" asChild>
              <Link href="/shop">Explore Our Collection</Link>
            </Button>
          </div>
          <div className="aspect-square rounded-2xl overflow-hidden shadow-lg">
            <Image
              src="/cozy-library-reader.png"
              alt="Reader enjoying a book"
              width={600}
              height={600}
              className="w-full h-full object-cover"
            />
          </div>
        </div>
      </div>
    </section>
  );
}
