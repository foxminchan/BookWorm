import { CheckCircle2 } from "lucide-react";

type ConfirmationHeaderProps = {
  orderId: string;
};

export function ConfirmationHeader({ orderId }: ConfirmationHeaderProps) {
  return (
    <div className="text-center mb-24 space-y-8">
      <div className="flex justify-center">
        <div className="relative">
          <div className="size-24 rounded-full bg-linear-to-br from-primary/20 to-primary/5 flex items-center justify-center animate-pulse">
            <CheckCircle2 className="size-12 text-primary animate-in zoom-in duration-700" />
          </div>
        </div>
      </div>

      <div className="space-y-4">
        <h1 className="font-serif text-6xl md:text-7xl font-medium text-foreground tracking-tight text-balance">
          Order Confirmed
        </h1>
        <p className="font-serif text-xl md:text-2xl text-muted-foreground font-light leading-relaxed">
          Your books are on their way to you
        </p>
      </div>

      <div className="pt-8">
        <p className="text-sm uppercase tracking-widest text-muted-foreground mb-2">
          Order Number
        </p>
        <p className="font-serif text-3xl font-medium text-foreground">
          #{orderId.slice(0, 8).toUpperCase()}
        </p>
      </div>
    </div>
  );
}
