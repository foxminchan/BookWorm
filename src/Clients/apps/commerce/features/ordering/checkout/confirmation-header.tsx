import { CheckCircle2 } from "lucide-react";

type ConfirmationHeaderProps = {
  orderId: string;
};

export default function ConfirmationHeader({
  orderId,
}: Readonly<ConfirmationHeaderProps>) {
  return (
    <div className="mb-24 space-y-8 text-center">
      <div className="flex justify-center">
        <div className="relative">
          <div className="from-primary/20 to-primary/5 flex size-24 animate-pulse items-center justify-center rounded-full bg-linear-to-br">
            <CheckCircle2 className="text-primary animate-in zoom-in size-12 duration-700" />
          </div>
        </div>
      </div>

      <div className="space-y-4">
        <h1 className="text-foreground font-serif text-6xl font-medium tracking-tight text-balance md:text-7xl">
          Order Confirmed
        </h1>
        <p className="text-muted-foreground font-serif text-xl leading-relaxed font-light md:text-2xl">
          Your books are on their way to you
        </p>
      </div>

      <div className="pt-8">
        <p className="text-muted-foreground mb-2 text-sm tracking-widest uppercase">
          Order Number
        </p>
        <p className="text-foreground font-serif text-3xl font-medium">
          #{orderId.slice(0, 8).toUpperCase()}
        </p>
      </div>
    </div>
  );
}
