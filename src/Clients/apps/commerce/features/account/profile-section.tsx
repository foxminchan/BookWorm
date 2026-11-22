import { User } from "lucide-react";

import type { Buyer } from "@workspace/types/ordering/buyers";

type ProfileSectionProps = {
  buyer: Buyer;
};

export default function ProfileSection({
  buyer,
}: Readonly<ProfileSectionProps>) {
  return (
    <div className="border-border/40 rounded-lg border p-6">
      <div className="mb-6 flex items-center gap-4">
        <div className="bg-primary/10 flex size-16 items-center justify-center rounded-full">
          <User className="text-primary size-8" />
        </div>
        <div>
          <h3 className="font-serif text-xl font-semibold">
            {buyer.name || "No Name"}
          </h3>
        </div>
      </div>
    </div>
  );
}
