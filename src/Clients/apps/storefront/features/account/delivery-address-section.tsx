"use client";

import { useState } from "react";
import { MapPin, Edit2, Save, X, Loader2 } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@workspace/ui/components/form";
import type { Buyer } from "@workspace/types/ordering/buyers";
import {
  updateAddressSchema,
  type UpdateAddressInput,
} from "@workspace/validations/ordering/buyers";
import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";

type DeliveryAddressSectionProps = {
  buyer: Buyer;
};

export default function DeliveryAddressSection({
  buyer,
}: DeliveryAddressSectionProps) {
  const updateAddressMutation = useUpdateBuyerAddress();
  const [isEditingAddress, setIsEditingAddress] = useState(false);

  const form = useForm<UpdateAddressInput>({
    resolver: zodResolver(updateAddressSchema),
    defaultValues: {
      street: "",
      city: "",
      province: "",
    },
  });

  const handleEditAddress = () => {
    const parts = buyer?.address?.split(", ") || [];
    form.reset({
      street: parts[0] || "",
      city: parts[1] || "",
      province: parts[2] || "",
    });
    setIsEditingAddress(true);
  };

  const handleSaveAddress = async (data: UpdateAddressInput) => {
    try {
      await updateAddressMutation.mutateAsync({ request: data });
      setIsEditingAddress(false);
    } catch (error) {
      console.error("Failed to update address:", error);
    }
  };

  const handleCancelEdit = () => {
    setIsEditingAddress(false);
    form.reset();
  };

  return (
    <div className="border border-border/40 rounded-lg p-8 hover:bg-secondary/20 transition-colors">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <div className="size-10 bg-primary/10 rounded-lg flex items-center justify-center">
            <MapPin className="size-5 text-primary" />
          </div>
          <h3 className="font-serif text-xl font-semibold">Delivery Address</h3>
        </div>
        {!isEditingAddress && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleEditAddress}
            className="gap-2 text-primary hover:text-primary hover:bg-primary/5"
          >
            <Edit2 className="size-4" />
            <span>Edit</span>
          </Button>
        )}
      </div>

      {!isEditingAddress ? (
        <p className="text-base text-muted-foreground leading-relaxed pl-13">
          {buyer.address || "No address set"}
        </p>
      ) : (
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSaveAddress)}
            className="space-y-4 mt-6 pl-13"
          >
            <FormField
              control={form.control}
              name="street"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Street Address</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Enter street address"
                      className="border-border/40"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="city"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>City</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter city"
                        className="border-border/40"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="province"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Province</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter province"
                        className="border-border/40"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <div className="flex gap-3 pt-2">
              <Button
                type="submit"
                className="flex-1 gap-2 bg-primary hover:bg-primary/90"
                disabled={updateAddressMutation.isPending}
              >
                {updateAddressMutation.isPending ? (
                  <Loader2 className="size-4 animate-spin" />
                ) : (
                  <Save className="size-4" />
                )}
                {updateAddressMutation.isPending ? "Saving..." : "Save Address"}
              </Button>
              <Button
                type="button"
                onClick={handleCancelEdit}
                variant="outline"
                className="gap-2 bg-transparent"
                disabled={updateAddressMutation.isPending}
              >
                <X className="size-4" />
                Cancel
              </Button>
            </div>
          </form>
        </Form>
      )}
    </div>
  );
}
