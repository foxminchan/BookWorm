import { Badge } from "@workspace/ui/components/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { currencyFormatter } from "@/lib/constants";
import { type OrderStatus, getOrderStatusStyle } from "@/lib/pattern";

type OrderSummaryCardsProps = {
  status: OrderStatus;
  total: number;
  itemCount: number;
};

export function OrderSummaryCards({
  status,
  total,
  itemCount,
}: OrderSummaryCardsProps) {
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
      <Card>
        <CardHeader>
          <CardTitle className="text-sm">Order Status</CardTitle>
        </CardHeader>
        <CardContent>
          <Badge className={getOrderStatusStyle(status)}>{status}</Badge>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle className="text-sm">Order Total</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">
            {currencyFormatter.format(total)}
          </div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle className="text-sm">Items</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-2xl font-bold">{itemCount}</div>
        </CardContent>
      </Card>
    </div>
  );
}
