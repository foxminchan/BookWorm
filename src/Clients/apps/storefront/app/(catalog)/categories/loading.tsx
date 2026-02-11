import { CategoryCardSkeleton } from "@/components/loading-skeleton";

export default function CategoriesLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 6 }, () => crypto.randomUUID()).map((id) => (
          <CategoryCardSkeleton key={id} />
        ))}
      </div>
    </div>
  );
}
