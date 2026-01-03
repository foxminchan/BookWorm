export default function FreeShippingBanner() {
  return (
    <section className="bg-primary text-primary-foreground py-16">
      <div className="container mx-auto px-4 text-center">
        <h2 className="mb-4 font-serif text-3xl font-medium md:text-4xl">
          Free Shipping on Orders Over $50
        </h2>
        <p className="mx-auto max-w-xl text-lg opacity-90">
          No hidden fees. No minimum order requirements for standard shipping.
          Just books, delivered to you.
        </p>
      </div>
    </section>
  );
}
