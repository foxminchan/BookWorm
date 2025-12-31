"use client";

export function ReviewsLoadingSkeleton() {
  return (
    <div className="space-y-6">
      <div className="h-10 bg-secondary rounded-lg animate-pulse w-1/3" />
      <div className="h-6 bg-secondary rounded-lg animate-pulse w-1/2" />
      <div className="space-y-8">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="space-y-4">
            <div className="flex gap-3">
              <div className="size-10 rounded-full bg-secondary animate-pulse shrink-0" />
              <div className="flex-1 space-y-2">
                <div className="h-4 bg-secondary rounded animate-pulse w-1/4" />
                <div className="flex gap-1">
                  {[...Array(5)].map((_, j) => (
                    <div
                      key={j}
                      className="size-3 bg-secondary rounded animate-pulse"
                    />
                  ))}
                </div>
              </div>
            </div>
            <div className="h-16 bg-secondary rounded animate-pulse" />
          </div>
        ))}
      </div>
    </div>
  );
}
