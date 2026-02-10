import Link from "next/link";

import type { LucideIcon } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

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
}: Readonly<EmptyStateProps>) {
  return (
    <output className="block space-y-6 py-24 text-center" aria-live="polite">
      <div className="bg-secondary text-muted-foreground mx-auto flex size-20 items-center justify-center rounded-full">
        <Icon className="size-10" aria-hidden="true" />
      </div>
      <h2 className="font-serif text-2xl font-medium">{title}</h2>
      <p className="text-muted-foreground">{description}</p>
      {actionLabel && actionHref && (
        <Button asChild className="rounded-full">
          <Link href={actionHref}>{actionLabel}</Link>
        </Button>
      )}
    </output>
  );
}
