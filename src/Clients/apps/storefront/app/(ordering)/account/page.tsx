"use client";

import { AlertCircle } from "lucide-react";

import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";

import { EmptyState } from "@/components/empty-state";
import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import { AccountPageSkeleton } from "@/components/loading-skeleton";
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
      <div className="bg-background flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto max-w-5xl flex-1 px-4 py-16 md:px-8">
          <AccountPageSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (buyerError || !buyer) {
    return (
      <div className="bg-background flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto max-w-5xl flex-1 px-4 py-16 md:px-8">
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
    <div className="bg-background flex min-h-screen flex-col">
      <Header />

      <main className="container mx-auto max-w-5xl flex-1 px-4 py-16 md:px-8">
        <div className="mb-12 md:mb-16">
          <h1 className="mb-3 font-serif text-5xl font-light text-balance md:text-6xl">
            Your Account
          </h1>
          <p className="text-muted-foreground max-w-2xl text-lg leading-relaxed">
            Manage your profile information, delivery address, and view your
            order history
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-3">
          <div className="space-y-8 lg:col-span-2">
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
