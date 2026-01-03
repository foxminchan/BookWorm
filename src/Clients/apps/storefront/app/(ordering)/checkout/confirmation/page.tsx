"use client";

import { useSearchParams } from "next/navigation";

import { AlertCircle } from "lucide-react";

import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

import { EmptyState } from "@/components/empty-state";
import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import { ConfirmationPageSkeleton } from "@/components/loading-skeleton";
import ConfirmationActions from "@/features/ordering/checkout/confirmation-actions";
import ConfirmationHeader from "@/features/ordering/checkout/confirmation-header";
import OrderDetailsSection from "@/features/ordering/checkout/order-details-section";

export default function ConfirmationPage() {
  const searchParams = useSearchParams();
  const orderId = searchParams.get("orderId");

  const { data: order, isPending } = useOrder(orderId ?? "", {
    enabled: !!orderId,
  });
  const { data: buyer } = useBuyer();

  if (isPending) {
    return (
      <div className="bg-background flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto grow px-4 py-24">
          <ConfirmationPageSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="bg-background flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto grow px-4 py-24">
          <div className="mx-auto max-w-3xl">
            <EmptyState
              icon={AlertCircle}
              title="Order Not Found"
              description="Unable to load order details."
              actionLabel="Return to Basket"
              actionHref="/basket"
            />
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="bg-background flex min-h-screen flex-col">
      <Header />
      <main className="container mx-auto grow px-4 py-24">
        <div className="mx-auto max-w-3xl">
          <ConfirmationHeader orderId={order.id} />
          <OrderDetailsSection
            status={order.status}
            total={order.total}
            buyerName={buyer?.name ?? undefined}
            buyerAddress={buyer?.address ?? undefined}
          />
          <div className="bg-secondary/30 mb-12 rounded-xl py-8 text-center">
            <p className="text-muted-foreground text-sm">
              <span className="font-medium">
                Confirmation email sent to your email
              </span>
            </p>
          </div>
          <ConfirmationActions orderId={order.id} />
        </div>
      </main>
      <Footer />
    </div>
  );
}
