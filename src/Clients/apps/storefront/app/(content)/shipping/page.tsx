import DamagedPackages from "@/features/content/shipping/damaged-packages";
import DeliveryInformation from "@/features/content/shipping/delivery-information";
import FaqSection from "@/features/content/shipping/faq-section";
import FreeShippingBanner from "@/features/content/shipping/free-shipping-banner";
import HeroSection from "@/features/content/shipping/hero-section";
import ShippingOptions from "@/features/content/shipping/shipping-options";

export default function ShippingPage() {
  return (
    <main className="grow">
      <HeroSection />
      <FreeShippingBanner />
      <ShippingOptions />
      <DeliveryInformation />
      <DamagedPackages />
      <FaqSection />
    </main>
  );
}
