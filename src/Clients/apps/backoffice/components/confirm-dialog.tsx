"use client";

import { Loader2 } from "lucide-react";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogTitle,
} from "@workspace/ui/components/alert-dialog";

type ActionType = "delete" | "complete" | "cancel" | "submit" | "default";

type ConfirmDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  actionLabel: string;
  isLoading: boolean;
  onConfirm: () => Promise<void>;
  actionType?: ActionType;
};

const actionStyles: Record<ActionType, string> = {
  delete: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
  complete: "bg-green-600 text-white hover:bg-green-700",
  cancel: "bg-orange-600 text-white hover:bg-orange-700",
  submit: "bg-primary text-primary-foreground hover:bg-primary/90",
  default: "",
};

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  actionLabel,
  isLoading,
  onConfirm,
  actionType = "default",
}: ConfirmDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>{title}</AlertDialogTitle>
        <AlertDialogDescription>{description}</AlertDialogDescription>
        <div className="flex justify-end gap-2">
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            disabled={isLoading}
            className={actionStyles[actionType]}
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Processing...
              </>
            ) : (
              actionLabel
            )}
          </AlertDialogAction>
        </div>
      </AlertDialogContent>
    </AlertDialog>
  );
}
