import { CategoryCardSkeleton } from "@/components/loading-skeleton";
import CategoriesHeader from "@/features/catalog/categories/categories-header";

export default function CategoriesLoading() {
  return (
    <div className="container mx-auto grow px-4 py-16 md:py-24">
      <CategoriesHeader />
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 6 }, (_, idx) => (
          <CategoryCardSkeleton key={idx} />
        ))}
      </div>
    </div>
  );
}
