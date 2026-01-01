import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import HeroSection from "@/features/content/shipping/hero-section";
import FaqSection from "@/features/content/shipping/faq-section";
import DamagedPackages from "@/features/content/shipping/damaged-packages";
import DeliveryInformation from "@/features/content/shipping/delivery-information";
import FreeShippingBanner from "@/features/content/shipping/free-shipping-banner";
import ShippingOptions from "@/features/content/shipping/shipping-options";

export default function ShippingPage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="grow">
        <HeroSection />
        <FreeShippingBanner />
        <ShippingOptions />
        <DeliveryInformation />
        <DamagedPackages />
        <FaqSection />
      </main>
      <Footer />
    </div>
  );
}
