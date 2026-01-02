"use client";

import { useState } from "react";

import { zodResolver } from "@hookform/resolvers/zod";
import { Edit2, Loader2, MapPin, Save, X } from "lucide-react";
import { useForm } from "react-hook-form";

import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";
import type { Buyer } from "@workspace/types/ordering/buyers";
import { Button } from "@workspace/ui/components/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@workspace/ui/components/form";
import { Input } from "@workspace/ui/components/input";
import {
  type UpdateAddressInput,
  updateAddressSchema,
} from "@workspace/validations/ordering/buyers";

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
    <div className="border-border/40 hover:bg-secondary/20 rounded-lg border p-8 transition-colors">
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="bg-primary/10 flex size-10 items-center justify-center rounded-lg">
            <MapPin className="text-primary size-5" />
          </div>
          <h3 className="font-serif text-xl font-semibold">Delivery Address</h3>
        </div>
        {!isEditingAddress && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleEditAddress}
            className="text-primary hover:text-primary hover:bg-primary/5 gap-2"
          >
            <Edit2 className="size-4" />
            <span>Edit</span>
          </Button>
        )}
      </div>

      {!isEditingAddress ? (
        <p className="text-muted-foreground pl-13 text-base leading-relaxed">
          {buyer.address || "No address set"}
        </p>
      ) : (
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSaveAddress)}
            className="mt-6 space-y-4 pl-13"
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
                className="bg-primary hover:bg-primary/90 flex-1 gap-2"
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
