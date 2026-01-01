import { Card, CardContent } from "@workspace/ui/components/card";
import { Package, Clock, Truck, Globe, type LucideIcon } from "lucide-react";

type ShippingOption = {
  icon: LucideIcon;
  title: string;
  time: string;
  price: string;
};

const shippingOptions: ShippingOption[] = [
  {
    icon: Package,
    title: "Standard Shipping",
    time: "5-7 Business Days",
    price: "Free on orders over $50",
  },
  {
    icon: Clock,
    title: "Express Shipping",
    time: "2-3 Business Days",
    price: "$9.99",
  },
  {
    icon: Truck,
    title: "Overnight Shipping",
    time: "Next Business Day",
    price: "$24.99",
  },
  {
    icon: Globe,
    title: "International",
    time: "10-21 Business Days",
    price: "Calculated at checkout",
  },
];

export default function ShippingOptions() {
  return (
    <section className="py-24 container mx-auto px-4">
      <h2 className="text-4xl font-serif font-medium mb-16 text-center">
        Shipping Options
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        {shippingOptions.map((option, idx) => {
          const Icon = option.icon;
          return (
            <Card
              key={idx}
              className="border-none bg-secondary hover:shadow-lg transition-all"
            >
              <CardContent className="pt-8 space-y-4 text-center">
                <Icon className="size-12 text-primary mx-auto" />
                <h3 className="font-serif font-medium text-lg">
                  {option.title}
                </h3>
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">{option.time}</p>
                  <p className="font-medium text-primary">{option.price}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </section>
  );
}
