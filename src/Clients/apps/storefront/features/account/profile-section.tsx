import { User } from "lucide-react";
import type { Buyer } from "@workspace/types/ordering/buyers";

type ProfileSectionProps = {
  buyer: Buyer;
};

export default function ProfileSection({ buyer }: ProfileSectionProps) {
  return (
    <div className="border border-border/40 rounded-lg p-6">
      <div className="flex items-center gap-4 mb-6">
        <div className="size-16 bg-primary/10 rounded-full flex items-center justify-center">
          <User className="size-8 text-primary" />
        </div>
        <div>
          <h3 className="font-serif text-xl font-semibold">
            {buyer.name || "Guest User"}
          </h3>
          <p className="text-sm text-muted-foreground">
            Customer ID: <span className="font-mono">{buyer.id}</span>
          </p>
        </div>
      </div>
    </div>
  );
}
