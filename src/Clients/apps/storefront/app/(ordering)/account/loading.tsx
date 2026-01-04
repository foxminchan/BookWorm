import { AccountPageSkeleton } from "@/components/loading-skeleton";

export default function AccountLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <AccountPageSkeleton />
    </div>
  );
}
