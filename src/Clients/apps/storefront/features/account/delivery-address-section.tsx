"use client";

import { useState } from "react";
import { MapPin, Edit2, Save, X, Loader2 } from "lucide-react";
import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";
import type {
  Buyer,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";

type DeliveryAddressSectionProps = {
  buyer: Buyer;
};

export default function DeliveryAddressSection({
  buyer,
}: DeliveryAddressSectionProps) {
  const updateAddressMutation = useUpdateBuyerAddress();
  const [isEditingAddress, setIsEditingAddress] = useState(false);
  const [addressForm, setAddressForm] = useState<UpdateAddressRequest>({
    street: "",
    city: "",
    province: "",
  });

  const handleEditAddress = () => {
    const parts = buyer?.address?.split(", ") || [];
    setAddressForm({
      street: parts[0] || "",
      city: parts[1] || "",
      province: parts[2] || "",
    });
    setIsEditingAddress(true);
  };

  const handleSaveAddress = async () => {
    try {
      await updateAddressMutation.mutateAsync({ request: addressForm });
      setIsEditingAddress(false);
    } catch (error) {
      console.error("Failed to update address:", error);
    }
  };

  const handleCancelEdit = () => {
    setIsEditingAddress(false);
    setAddressForm({ street: "", city: "", province: "" });
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
        <div className="space-y-4 mt-6 pl-13">
          <div>
            <Label htmlFor="street" className="text-sm font-medium mb-2 block">
              Street Address
            </Label>
            <Input
              id="street"
              value={addressForm.street}
              onChange={(e) =>
                setAddressForm({
                  ...addressForm,
                  street: e.target.value,
                })
              }
              placeholder="Enter street address"
              className="border-border/40"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="city" className="text-sm font-medium mb-2 block">
                City
              </Label>
              <Input
                id="city"
                value={addressForm.city}
                onChange={(e) =>
                  setAddressForm({
                    ...addressForm,
                    city: e.target.value,
                  })
                }
                placeholder="Enter city"
                className="border-border/40"
              />
            </div>
            <div>
              <Label
                htmlFor="province"
                className="text-sm font-medium mb-2 block"
              >
                Province
              </Label>
              <Input
                id="province"
                value={addressForm.province}
                onChange={(e) =>
                  setAddressForm({
                    ...addressForm,
                    province: e.target.value,
                  })
                }
                placeholder="Enter province"
                className="border-border/40"
              />
            </div>
          </div>
          <div className="flex gap-3 pt-2">
            <Button
              onClick={handleSaveAddress}
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
              onClick={handleCancelEdit}
              variant="outline"
              className="gap-2 bg-transparent"
              disabled={updateAddressMutation.isPending}
            >
              <X className="size-4" />
              Cancel
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
