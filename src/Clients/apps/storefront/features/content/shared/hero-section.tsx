type HeroSectionProps = {
  readonly title: string;
  readonly description: string;
};

export default function HeroSection({ title, description }: HeroSectionProps) {
  return (
    <section className="container mx-auto border-b px-4 py-32 text-center">
      <h1 className="mx-auto mb-8 max-w-5xl font-serif text-6xl font-light tracking-tight text-balance md:text-7xl">
        {title}
      </h1>
      <p className="text-muted-foreground mx-auto max-w-2xl text-lg text-pretty md:text-xl">
        {description}
      </p>
    </section>
  );
}
