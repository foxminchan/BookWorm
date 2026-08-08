import type { CellContext, ColumnDef } from "@tanstack/react-table";
import { Star } from "lucide-react";

import type { Feedback } from "@workspace/types/rating";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@workspace/ui/components/tooltip";

import { backofficeTableFeatures } from "@/lib/table";

import { CellAction } from "./cell-action";

type FeedbackCellContext = CellContext<
  typeof backofficeTableFeatures,
  Feedback,
  unknown
>;

function CustomerCell({ row }: Readonly<FeedbackCellContext>) {
  const firstName = row.getValue<string>("firstName");
  const lastName = row.original.lastName;
  return (
    <div>
      {firstName && lastName
        ? `${firstName} ${lastName}`
        : (firstName ?? "Anonymous")}
    </div>
  );
}

function RatingCell({ row }: Readonly<FeedbackCellContext>) {
  const rating = row.getValue<number>("rating");
  return (
    <div className="flex items-center gap-1">
      <Star
        className="h-4 w-4 fill-yellow-400 text-yellow-400"
        aria-hidden="true"
      />
      <span className="font-medium">{rating.toFixed(1)}</span>
    </div>
  );
}

function CommentCell({ row }: Readonly<FeedbackCellContext>) {
  const comment = row.getValue<string>("comment");
  if (!comment) {
    return <div className="text-muted-foreground text-sm">No comment</div>;
  }
  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          <div className="text-muted-foreground max-w-xs cursor-help truncate text-sm">
            {comment}
          </div>
        </TooltipTrigger>
        <TooltipContent className="max-w-md">
          <p className="whitespace-pre-wrap">{comment}</p>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}

function ActionsCell({ row }: Readonly<FeedbackCellContext>) {
  return <CellAction feedback={row.original} />;
}

export const reviewsColumns: ColumnDef<
  typeof backofficeTableFeatures,
  Feedback
>[] = [
  { accessorKey: "firstName", header: "Customer", cell: CustomerCell },
  { accessorKey: "rating", header: "Rating", cell: RatingCell },
  { accessorKey: "comment", header: "Comment", cell: CommentCell },
  { id: "actions", cell: ActionsCell },
];
