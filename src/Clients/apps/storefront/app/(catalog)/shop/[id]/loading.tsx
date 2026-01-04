import { ProductLoadingSkeleton } from "@/components/loading-skeleton";

export default function ProductLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <ProductLoadingSkeleton />
    </div>
  );
}
