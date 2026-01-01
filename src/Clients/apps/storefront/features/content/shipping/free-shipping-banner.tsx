export function FreeShippingBanner() {
  return (
    <section className="py-16 bg-primary text-primary-foreground">
      <div className="container mx-auto px-4 text-center">
        <h2 className="text-3xl md:text-4xl font-serif font-medium mb-4">
          Free Shipping on Orders Over $50
        </h2>
        <p className="text-lg opacity-90 max-w-xl mx-auto">
          No hidden fees. No minimum order requirements for standard shipping.
          Just books, delivered to you.
        </p>
      </div>
    </section>
  );
}
