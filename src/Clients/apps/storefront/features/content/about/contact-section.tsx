import { Button } from "@workspace/ui/components/button";

export default function ContactSection() {
  return (
    <section className="container mx-auto px-4 py-24 text-center">
      <h2 className="mb-8 font-serif text-4xl font-medium md:text-5xl">
        Get In Touch
      </h2>
      <p className="text-muted-foreground mx-auto mb-12 max-w-2xl text-lg">
        Have questions or want to share your reading journey with us? We'd love
        to hear from you.
      </p>
      <Button size="lg" className="rounded-full px-8" asChild>
        <a href="mailto:support@bookworm.com">Get In Touch</a>
      </Button>
    </section>
  );
}
