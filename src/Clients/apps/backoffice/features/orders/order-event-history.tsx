"use client";

import { useState } from "react";

import { format } from "date-fns";
import { History, RotateCcw } from "lucide-react";

import useOrderHistory from "@workspace/api-hooks/ordering/orders/useOrderHistory";
import useOrderSummary from "@workspace/api-hooks/ordering/orders/useOrderSummary";
import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { formatPrice } from "@workspace/utils/format";
import { getOrderStatusStyle } from "@workspace/utils/order";

type OrderEventHistoryProps = Readonly<{
  orderId: string;
}>;

export function OrderEventHistory({ orderId }: OrderEventHistoryProps) {
  const [selectedVersion, setSelectedVersion] = useState<number | undefined>(
    undefined,
  );

  const {
    data: history,
    isLoading: isHistoryLoading,
    isError: isHistoryError,
  } = useOrderHistory(orderId, { retry: false });

  const { data: summary } = useOrderSummary(
    orderId,
    selectedVersion !== undefined ? { version: selectedVersion } : undefined,
    { retry: false },
  );

  if (isHistoryLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <History className="size-4" />
            Event Stream &amp; Time-Travel
          </CardTitle>
          <CardDescription>Loading event stream...</CardDescription>
        </CardHeader>
      </Card>
    );
  }

  if (isHistoryError || !history || history.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <History className="size-4" />
            Event Stream &amp; Time-Travel
          </CardTitle>
          <CardDescription>
            No event stream recorded for this order yet.
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-start justify-between gap-4">
        <div>
          <CardTitle className="flex items-center gap-2 text-base">
            <History className="size-4" />
            Event Stream &amp; Time-Travel
          </CardTitle>
          <CardDescription>
            Audit log reconstructed from Marten events. Select any version to
            replay state.
          </CardDescription>
        </div>
        {selectedVersion !== undefined && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => setSelectedVersion(undefined)}
          >
            <RotateCcw className="mr-1.5 size-3.5" />
            Latest
          </Button>
        )}
      </CardHeader>
      <CardContent className="space-y-4">
        {summary && (
          <div className="bg-muted/40 flex flex-wrap items-center justify-between gap-3 rounded-md border p-3">
            <div className="flex items-center gap-2 text-sm">
              <span className="text-muted-foreground">
                {selectedVersion !== undefined
                  ? `Replayed at v${summary.version}:`
                  : `Current stream state (v${summary.version}):`}
              </span>
              <Badge className={getOrderStatusStyle(summary.status)}>
                {summary.status}
              </Badge>
            </div>
            <span className="text-sm font-semibold">
              {formatPrice(summary.totalPrice)}
            </span>
          </div>
        )}

        <ol className="space-y-2">
          {history.map((item) => {
            const isActive =
              selectedVersion !== undefined
                ? selectedVersion === item.version
                : item.version === history[history.length - 1]?.version;

            return (
              <li
                key={`${item.sequence}-${item.version}`}
                className="flex flex-wrap items-center justify-between gap-3 rounded-md border p-3 text-sm"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant="outline">v{item.version}</Badge>
                  <span className="font-mono text-xs font-medium">
                    {item.eventType}
                  </span>
                  {item.status && (
                    <Badge className={getOrderStatusStyle(item.status)}>
                      {item.status}
                    </Badge>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-muted-foreground text-xs">
                    {format(new Date(item.timestamp), "MMM dd, yyyy HH:mm:ss")}
                  </span>
                  <Button
                    type="button"
                    variant={isActive ? "secondary" : "ghost"}
                    size="sm"
                    onClick={() => setSelectedVersion(item.version)}
                  >
                    Replay v{item.version}
                  </Button>
                </div>
              </li>
            );
          })}
        </ol>
      </CardContent>
    </Card>
  );
}
