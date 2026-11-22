import { OrdersSkeleton } from "@/components/loading-skeleton";

export default function OrdersLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <OrdersSkeleton />
    </div>
  );
}
