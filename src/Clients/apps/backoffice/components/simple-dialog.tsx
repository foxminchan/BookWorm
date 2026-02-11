"use client";

import type React from "react";
import { useCallback, useState } from "react";

import { Button } from "@workspace/ui/components/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@workspace/ui/components/dialog";
import { Input } from "@workspace/ui/components/input";

type SimpleDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  placeholder: string;
  onSubmit: (value: string) => Promise<void>;
  isLoading?: boolean;
  submitLabel?: string;
  submittingLabel?: string;
};

export function SimpleDialog({
  open,
  onOpenChange,
  title,
  description,
  placeholder,
  onSubmit,
  isLoading = false,
  submitLabel = "Create",
  submittingLabel = "Submitting...",
}: Readonly<SimpleDialogProps>) {
  const [value, setValue] = useState("");

  const handleSubmit = useCallback(async () => {
    const trimmed = value.trim();
    if (!trimmed || isLoading) return;

    await onSubmit(trimmed);
    setValue("");
  }, [value, isLoading, onSubmit]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "Enter") {
        e.preventDefault();
        handleSubmit();
      }
    },
    [handleSubmit],
  );

  const handleOpenChange = useCallback(
    (nextOpen: boolean) => {
      if (!nextOpen) setValue("");
      onOpenChange(nextOpen);
    },
    [onOpenChange],
  );

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <Input
            placeholder={placeholder}
            value={value}
            onChange={(e) => setValue(e.target.value)}
            onKeyDown={handleKeyDown}
            disabled={isLoading}
          />
          <div className="flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() => handleOpenChange(false)}
              disabled={isLoading}
            >
              Cancel
            </Button>
            <Button
              onClick={handleSubmit}
              disabled={isLoading || !value.trim()}
            >
              {isLoading ? submittingLabel : submitLabel}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
