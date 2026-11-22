"use client";

import { useCallback, useState } from "react";

import { Form, Field as FormischField, reset, useForm } from "@formisch/react";
import { Edit2, Loader2, MapPin, Save, X } from "lucide-react";

import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";
import type { Buyer } from "@workspace/types/ordering/buyers";
import { Button } from "@workspace/ui/components/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@workspace/ui/components/field";
import { Input } from "@workspace/ui/components/input";
import {
  type UpdateAddressInput,
  updateAddressSchema,
} from "@workspace/validations/ordering/buyers";

type DeliveryAddressSectionProps = {
  buyer: Buyer;
};

type AddressFieldConfig = {
  name: keyof UpdateAddressInput;
  label: string;
  placeholder: string;
};

const ADDRESS_FIELDS: AddressFieldConfig[] = [
  { name: "city", label: "City", placeholder: "Enter city" },
  { name: "province", label: "Province", placeholder: "Enter province" },
];

function parseAddress(address: string | null | undefined): UpdateAddressInput {
  const parts = address?.split(", ") ?? [];
  return {
    street: parts[0] ?? "",
    city: parts[1] ?? "",
    province: parts[2] ?? "",
  };
}

export default function DeliveryAddressSection({
  buyer,
}: Readonly<DeliveryAddressSectionProps>) {
  const [isEditingAddress, setIsEditingAddress] = useState(false);

  const form = useForm({
    schema: updateAddressSchema,
    initialInput: {
      street: "",
      city: "",
      province: "",
    },
  });

  const updateAddressMutation = useUpdateBuyerAddress();

  const handleEditAddress = useCallback(() => {
    reset(form, { initialInput: parseAddress(buyer.address) });
    setIsEditingAddress(true);
  }, [buyer.address, form]);

  const handleCancelEdit = useCallback(() => {
    setIsEditingAddress(false);
    reset(form);
  }, [form]);

  const handleSaveAddress = useCallback(
    (data: UpdateAddressInput) => {
      updateAddressMutation.mutate(
        { request: data },
        { onSuccess: () => setIsEditingAddress(false) },
      );
    },
    [updateAddressMutation],
  );

  const isPending = updateAddressMutation.isPending;

  return (
    <div className="border-border/40 hover:bg-secondary/20 rounded-lg border p-8 transition-colors">
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="bg-primary/10 flex size-10 items-center justify-center rounded-lg">
            <MapPin className="text-primary size-5" aria-hidden="true" />
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
            <Edit2 className="size-4" aria-hidden="true" />
            <span>Edit</span>
          </Button>
        )}
      </div>

      {isEditingAddress ? (
        <Form
          of={form}
          onSubmit={handleSaveAddress}
          className="mt-6 space-y-4 pl-13"
        >
          <FieldGroup className="gap-4">
            <FormischField of={form} path={["street"]}>
              {(field) => (
                <Field data-invalid={!!field.errors}>
                  <FieldLabel htmlFor="address-street">
                    Street Address
                  </FieldLabel>
                  <Input
                    {...field.props}
                    id="address-street"
                    value={field.input ?? ""}
                    placeholder="Enter street address"
                    className="border-border/40"
                    aria-invalid={!!field.errors}
                  />
                  <FieldError
                    errors={field.errors?.map((message) => ({ message }))}
                  />
                </Field>
              )}
            </FormischField>
            <div className="grid grid-cols-2 gap-4">
              {ADDRESS_FIELDS.map(({ name, label, placeholder }) => (
                <FormischField key={name} of={form} path={[name]}>
                  {(field) => (
                    <Field data-invalid={!!field.errors}>
                      <FieldLabel htmlFor={`address-${name}`}>
                        {label}
                      </FieldLabel>
                      <Input
                        {...field.props}
                        id={`address-${name}`}
                        value={field.input ?? ""}
                        placeholder={placeholder}
                        className="border-border/40"
                        aria-invalid={!!field.errors}
                      />
                      <FieldError
                        errors={field.errors?.map((message) => ({ message }))}
                      />
                    </Field>
                  )}
                </FormischField>
              ))}
            </div>
          </FieldGroup>
          <div className="flex gap-3 pt-2">
            <Button
              type="submit"
              className="bg-primary hover:bg-primary/90 flex-1 gap-2"
              disabled={isPending}
            >
              {isPending ? (
                <Loader2 className="size-4 animate-spin" aria-hidden="true" />
              ) : (
                <Save className="size-4" aria-hidden="true" />
              )}
              {isPending ? "Saving..." : "Save Address"}
            </Button>
            <Button
              type="button"
              onClick={handleCancelEdit}
              variant="outline"
              className="gap-2 bg-transparent"
              disabled={isPending}
            >
              <X className="size-4" aria-hidden="true" />
              Cancel
            </Button>
          </div>
        </Form>
      ) : (
        <p className="text-muted-foreground pl-13 text-base leading-relaxed">
          {buyer.address || "No address set"}
        </p>
      )}
    </div>
  );
}
