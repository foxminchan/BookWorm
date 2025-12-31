"use client";

import { Button } from "@workspace/ui/components/button";
import { ArrowRight } from "lucide-react";

interface SectionHeaderProps {
  title: string;
  description?: string;
  actionLabel?: string;
  onActionClick?: () => void;
  showAction?: boolean;
}

export function SectionHeader({
  title,
  description,
  actionLabel,
  onActionClick,
  showAction = true,
}: SectionHeaderProps) {
  return (
    <div className="flex items-end justify-between mb-12">
      <div>
        <h2 className="text-3xl font-serif font-medium mb-2">{title}</h2>
        {description && <p className="text-muted-foreground">{description}</p>}
      </div>
      {showAction && actionLabel && onActionClick && (
        <Button
          variant="ghost"
          className="hidden md:flex gap-2"
          onClick={onActionClick}
        >
          {actionLabel} <ArrowRight className="size-4" />
        </Button>
      )}
    </div>
  );
}
