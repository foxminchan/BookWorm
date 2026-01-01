import { Button } from "@workspace/ui/components/button";

export default function ContactSection() {
  return (
    <section className="py-24 container mx-auto px-4 text-center">
      <h2 className="text-4xl md:text-5xl font-serif font-medium mb-8">
        Get In Touch
      </h2>
      <p className="text-lg text-muted-foreground mb-12 max-w-2xl mx-auto">
        Have questions or want to share your reading journey with us? We'd love
        to hear from you.
      </p>
      <Button size="lg" className="rounded-full px-8" asChild>
        <a href="mailto:support@bookworm.com">Get In Touch</a>
      </Button>
    </section>
  );
}
