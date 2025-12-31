"use client";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { CheckCircle2, ArrowRight, Loader2 } from "lucide-react";
import Link from "next/link";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import { useSearchParams } from "next/navigation";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";
import { Skeleton } from "@workspace/ui/components/skeleton";

export default function ConfirmationPage() {
  const searchParams = useSearchParams();
  const orderId = searchParams.get("orderId");

  const { data: order, isPending } = useOrder(orderId ?? "", {
    enabled: !!orderId,
  });

  if (!orderId) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-24">
          <div className="max-w-3xl mx-auto text-center">
            <h1 className="font-serif text-4xl font-medium mb-4">
              Order Not Found
            </h1>
            <p className="text-muted-foreground mb-8">
              No order ID was provided.
            </p>
            <Button asChild>
              <Link href="/basket">Return to Basket</Link>
            </Button>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (isPending) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="grow container mx-auto px-4 py-24">
          <div className="max-w-3xl mx-auto">
            <div className="text-center mb-24 space-y-8">
              <div className="flex justify-center">
                <Loader2 className="size-12 animate-spin text-primary" />
              </div>
              <Skeleton className="h-12 w-96 mx-auto" />
              <Skeleton className="h-6 w-64 mx-auto" />
            </div>
          </div>
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
          <div className="max-w-3xl mx-auto text-center">
            <h1 className="font-serif text-4xl font-medium mb-4">
              Order Not Found
            </h1>
            <p className="text-muted-foreground mb-8">
              Unable to load order details.
            </p>
            <Button asChild>
              <Link href="/basket">Return to Basket</Link>
            </Button>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  const getStatusColor = (status: OrderStatus): string => {
    switch (status) {
      case "Completed":
        return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400";
      case "Cancelled":
        return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400";
      case "New":
        return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />
      <main className="grow container mx-auto px-4 py-24">
        <div className="max-w-3xl mx-auto">
          {/* Success Header Section */}
          <div className="text-center mb-24 space-y-8">
            <div className="flex justify-center">
              <div className="relative">
                <div className="size-24 rounded-full bg-linear-to-br from-primary/20 to-primary/5 flex items-center justify-center animate-pulse">
                  <CheckCircle2 className="size-12 text-primary animate-in zoom-in duration-700" />
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <h1 className="font-serif text-6xl md:text-7xl font-medium text-foreground tracking-tight text-balance">
                Order Confirmed
              </h1>
              <p className="font-serif text-xl md:text-2xl text-muted-foreground font-light leading-relaxed">
                Your books are on their way to you
              </p>
            </div>

            {/* Order Number - More prominent */}
            <div className="pt-8">
              <p className="text-sm uppercase tracking-widest text-muted-foreground mb-2">
                Order Number
              </p>
              <p className="font-serif text-3xl font-medium text-foreground">
                #{order.id.slice(0, 8).toUpperCase()}
              </p>
            </div>
          </div>

          {/* Details Section */}
          <div className="grid md:grid-cols-3 gap-8 mb-24 py-12 border-y border-border">
            {/* Shipping Address */}
            <div className="space-y-4">
              <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
                Shipping Address
              </h3>
              <div className="space-y-1 text-muted-foreground leading-relaxed">
                <p className="font-medium text-foreground">Jane Doe</p>
                <p>123 Literary Lane, Apt 4B</p>
                <p>Booktown, NY 10001</p>
              </div>
            </div>

            <div className="space-y-4">
              <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
                Order Status
              </h3>
              <div className="flex flex-col items-start">
                <p className="text-sm text-muted-foreground mb-2">
                  Current Status
                </p>
                <Badge
                  className={`${getStatusColor(order.status)} border-0 text-base px-3 py-1`}
                >
                  {order.status}
                </Badge>
              </div>
            </div>

            {/* Order Total */}
            <div className="space-y-4">
              <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
                Order Total
              </h3>
              <div className="flex flex-col items-start">
                <p className="text-sm text-muted-foreground mb-2">
                  Amount Paid
                </p>
                <p className="font-serif text-4xl font-medium text-primary">
                  ${order.total.toFixed(2)}
                </p>
              </div>
            </div>
          </div>

          {/* Email Confirmation */}
          <div className="text-center mb-12 py-8 bg-secondary/30 rounded-xl">
            <p className="text-sm text-muted-foreground">
              <span className="font-medium">
                Confirmation email sent to your email
              </span>
            </p>
          </div>

          {/* CTA Section */}
          <div className="flex flex-col sm:flex-row items-center justify-center gap-6">
            <Button
              asChild
              size="lg"
              className="rounded-full h-14 px-12 text-lg group"
            >
              <Link href="/shop">
                Continue Shopping{" "}
                <ArrowRight className="ml-3 size-5 group-hover:translate-x-1 transition-transform" />
              </Link>
            </Button>
            <Button
              asChild
              variant="outline"
              size="lg"
              className="rounded-full h-14 px-12 text-lg border-primary/20 hover:bg-primary/5 bg-transparent"
            >
              <Link href={`/account/orders/${order.id}`}>View Order</Link>
            </Button>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
}
