import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import {
  HeroSection,
  FreeShippingBanner,
  ShippingOptions,
  DeliveryInformation,
  DamagedPackages,
  FaqSection,
} from "@/features/content/shipping";

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
