import type { ColumnDef } from "@tanstack/react-table";
import { Star } from "lucide-react";

import type { Feedback } from "@workspace/types/rating";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@workspace/ui/components/tooltip";

import { CellAction } from "./cell-action";

export const reviewsColumns: ColumnDef<Feedback>[] = [
  {
    accessorKey: "firstName",
    header: "Customer",
    cell: ({ row }) => {
      const firstName = row.getValue("firstName") as string;
      const lastName = row.original.lastName;
      return (
        <div>
          {firstName && lastName
            ? `${firstName} ${lastName}`
            : firstName || "Anonymous"}
        </div>
      );
    },
  },
  {
    accessorKey: "rating",
    header: "Rating",
    cell: ({ row }) => {
      const rating = row.getValue("rating") as number;
      return (
        <div className="flex items-center gap-1">
          <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
          <span className="font-medium">{rating.toFixed(1)}</span>
        </div>
      );
    },
  },
  {
    accessorKey: "comment",
    header: "Comment",
    cell: ({ row }) => {
      const comment = row.getValue("comment") as string;
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
    },
  },
  {
    id: "actions",
    cell: ({ row }) => <CellAction feedback={row.original} />,
  },
];
