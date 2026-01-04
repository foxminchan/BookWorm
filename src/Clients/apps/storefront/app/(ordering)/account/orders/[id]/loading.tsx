import { OrderDetailSkeleton } from "@/components/loading-skeleton";

export default function OrderDetailLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <OrderDetailSkeleton />
    </div>
  );
}
