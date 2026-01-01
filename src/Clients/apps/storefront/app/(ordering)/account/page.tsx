"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { AccountPageSkeleton } from "@/components/loading-skeleton";
import { EmptyState } from "@/components/empty-state";
import { AlertCircle } from "lucide-react";
import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";
import AccountNavigation from "@/features/account/account-navigation";
import DeliveryAddressSection from "@/features/account/delivery-address-section";
import LogoutButton from "@/features/account/logout-button";
import ProfileSection from "@/features/account/profile-section";

export default function AccountPage() {
  const {
    data: buyer,
    isLoading: isLoadingBuyer,
    error: buyerError,
  } = useBuyer();

  if (isLoadingBuyer) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
          <AccountPageSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (buyerError || !buyer) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
          <EmptyState
            icon={AlertCircle}
            title="Error Loading Account"
            description="We couldn't load your account information. Please try again later."
            actionLabel="Back to Home"
            actionHref="/"
          />
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
        <div className="mb-12 md:mb-16">
          <h1 className="font-serif text-5xl md:text-6xl font-light mb-3 text-balance">
            Your Account
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl leading-relaxed">
            Manage your profile information, delivery address, and view your
            order history
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-8">
            <ProfileSection buyer={buyer} />
            <DeliveryAddressSection buyer={buyer} />
          </div>

          <div className="lg:col-span-1">
            <div className="sticky top-24 space-y-6">
              <AccountNavigation />
              <LogoutButton />
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
