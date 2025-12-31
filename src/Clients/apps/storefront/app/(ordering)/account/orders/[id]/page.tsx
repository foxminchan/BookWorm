"use client";

import { use } from "react";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { ArrowLeft, Package } from "lucide-react";
import Link from "next/link";
import { Skeleton } from "@workspace/ui/components/skeleton";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

function getStatusColor(status: OrderStatus): string {
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
}

function OrderDetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <Skeleton className="h-10 w-32" />
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-48" />
      </div>

      {/* Order Items Skeleton */}
      <div className="border border-border/40 rounded-lg overflow-hidden">
        <div className="border-b border-border/40 px-6 py-4">
          <Skeleton className="h-6 w-32" />
        </div>
        <div className="divide-y divide-border/40">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="px-6 py-4 space-y-3">
              <Skeleton className="h-5 w-48" />
              <Skeleton className="h-4 w-32" />
            </div>
          ))}
        </div>
      </div>

      {/* Order Summary Skeleton */}
      <div className="border border-border/40 rounded-lg p-6">
        <Skeleton className="h-6 w-32 mb-6" />
        <div className="space-y-3">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-6 w-full" />
        </div>
      </div>
    </div>
  );
}

export default function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: orderDetail, isLoading, error } = useOrder(id);

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
          <OrderDetailSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (error || !orderDetail) {
    return (
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
          <div className="text-center py-12">
            <h1 className="text-2xl font-serif mb-4">Order Not Found</h1>
            <p className="text-muted-foreground mb-6">
              We couldn't find the order you're looking for.
            </p>
            <Link href="/account/orders">
              <Button variant="outline">
                <ArrowLeft className="size-4 mr-2" />
                Back to Orders
              </Button>
            </Link>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col">
      <Header />

      <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
        <div className="mb-8">
          <Link href="/account/orders">
            <Button variant="ghost" className="mb-4 gap-2 -ml-2">
              <ArrowLeft className="size-4" />
              Back to Orders
            </Button>
          </Link>
          <div className="flex flex-wrap items-center gap-3 mb-2">
            <h1 className="font-serif text-4xl">Order {orderDetail.id}</h1>
            <Badge
              className={`${getStatusColor(orderDetail.status)} border-0 text-sm`}
            >
              {orderDetail.status}
            </Badge>
          </div>
          <p className="text-muted-foreground">
            Placed on{" "}
            {new Date(orderDetail.date).toLocaleDateString("en-US", {
              dateStyle: "long",
            })}
          </p>
        </div>

        <div className="grid gap-6">
          {/* Order Items */}
          <div className="border border-border/40 rounded-lg overflow-hidden bg-background">
            <div className="border-b border-border/40 px-6 py-4">
              <div className="flex items-center gap-2">
                <Package className="size-5 text-muted-foreground" />
                <h2 className="font-serif text-xl font-semibold">
                  Order Items
                </h2>
              </div>
            </div>
            <div className="divide-y divide-border/40">
              {orderDetail.items.map((item) => (
                <div
                  key={item.id}
                  className="px-6 py-4 hover:bg-secondary/20 transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <h3 className="font-medium mb-1">
                        {item.name || "Unnamed Item"}
                      </h3>
                      <p className="text-sm text-muted-foreground">
                        Quantity: {item.quantity}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-semibold">
                        ${(item.price * item.quantity).toFixed(2)}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        ${item.price.toFixed(2)} each
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Order Summary */}
          <div className="border border-border/40 rounded-lg p-6 bg-background hover:bg-secondary/20 transition-colors">
            <h2 className="font-serif text-xl font-semibold mb-4">
              Order Summary
            </h2>
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span>${orderDetail.total.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Shipping</span>
                <span className="text-green-600">Free</span>
              </div>
              <div className="border-t border-border/40 pt-2 mt-2">
                <div className="flex justify-between font-semibold text-lg">
                  <span>Total</span>
                  <span>${orderDetail.total.toFixed(2)}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
