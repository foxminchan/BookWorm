"use client";

export function ProductLoadingSkeleton() {
  return (
    <div className="space-y-8 animate-pulse">
      <div className="h-6 bg-secondary rounded w-32" />
      <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
        <div className="aspect-3/4 bg-secondary rounded-2xl" />
        <div className="space-y-6">
          <div className="h-8 bg-secondary rounded w-3/4" />
          <div className="h-6 bg-secondary rounded w-1/2" />
          <div className="h-24 bg-secondary rounded" />
        </div>
      </div>
    </div>
  );
}
