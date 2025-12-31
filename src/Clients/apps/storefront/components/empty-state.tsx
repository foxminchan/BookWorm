import type { LucideIcon } from "lucide-react";
import { Button } from "@workspace/ui/components/button";
import Link from "next/link";

type EmptyStateProps = {
  icon: LucideIcon;
  title: string;
  description: string;
  actionLabel?: string;
  actionHref?: string;
};

export function EmptyState({
  icon: Icon,
  title,
  description,
  actionLabel,
  actionHref,
}: EmptyStateProps) {
  return (
    <div className="text-center py-24 space-y-6">
      <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto text-muted-foreground">
        <Icon className="size-10" aria-hidden="true" />
      </div>
      <h2 className="text-2xl font-serif font-medium">{title}</h2>
      <p className="text-muted-foreground">{description}</p>
      {actionLabel && actionHref && (
        <Button asChild className="rounded-full">
          <Link href={actionHref}>{actionLabel}</Link>
        </Button>
      )}
    </div>
  );
}
