"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { EmptyState } from "@/components/empty-state";
import { useSearchParams } from "next/navigation";
import { AlertCircle } from "lucide-react";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";
import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";
import { ConfirmationPageSkeleton } from "@/components/loading-skeleton";
import ConfirmationHeader from "@/features/ordering/checkout/confirmation-header";
import ConfirmationActions from "@/features/ordering/checkout/confirmation-actions";
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
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-24">
          <ConfirmationPageSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-24">
          <div className="max-w-3xl mx-auto">
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
    <div className="min-h-screen flex flex-col bg-background">
      <Header />
      <main className="grow container mx-auto px-4 py-24">
        <div className="max-w-3xl mx-auto">
          <ConfirmationHeader orderId={order.id} />
          <OrderDetailsSection
            status={order.status}
            total={order.total}
            buyerName={buyer?.name ?? undefined}
            buyerAddress={buyer?.address ?? undefined}
          />
          <div className="text-center mb-12 py-8 bg-secondary/30 rounded-xl">
            <p className="text-sm text-muted-foreground">
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
