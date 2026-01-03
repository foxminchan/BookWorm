import { Clock, Globe, type LucideIcon, Package, Truck } from "lucide-react";

import { Card, CardContent } from "@workspace/ui/components/card";

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
    <section className="container mx-auto px-4 py-24">
      <h2 className="mb-16 text-center font-serif text-4xl font-medium">
        Shipping Options
      </h2>
      <div className="grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4">
        {shippingOptions.map((option, idx) => {
          const Icon = option.icon;
          return (
            <Card
              key={idx}
              className="bg-secondary border-none transition-all hover:shadow-lg"
            >
              <CardContent className="space-y-4 pt-8 text-center">
                <Icon className="text-primary mx-auto size-12" />
                <h3 className="font-serif text-lg font-medium">
                  {option.title}
                </h3>
                <div className="space-y-2">
                  <p className="text-muted-foreground text-sm">{option.time}</p>
                  <p className="text-primary font-medium">{option.price}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </section>
  );
}
