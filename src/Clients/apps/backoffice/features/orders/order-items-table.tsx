import type { OrderItem } from "@workspace/types/ordering/orders";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@workspace/ui/components/table";

import { currencyFormatter } from "@/lib/constants";

type OrderItemsTableProps = {
  items: OrderItem[];
  total: number;
};

export function OrderItemsTable({ items, total }: OrderItemsTableProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Order Items</CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Item Name</TableHead>
              <TableHead>Quantity</TableHead>
              <TableHead>Price</TableHead>
              <TableHead>Total</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name || "Product"}</TableCell>
                <TableCell>{item.quantity}</TableCell>
                <TableCell>{currencyFormatter.format(item.price)}</TableCell>
                <TableCell className="font-medium">
                  {currencyFormatter.format(item.quantity * item.price)}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
          <TableFooter>
            <TableRow>
              <TableCell colSpan={3}>Total</TableCell>
              <TableCell className="font-bold">
                {currencyFormatter.format(total)}
              </TableCell>
            </TableRow>
          </TableFooter>
        </Table>
      </CardContent>
    </Card>
  );
}
