"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";
import {
  MapPin,
  Edit2,
  Save,
  X,
  Loader2,
  ChevronRight,
  User,
} from "lucide-react";
import Link from "next/link";
import type { UpdateAddressRequest } from "@workspace/types/ordering/buyers";
import useBuyer from "@workspace/api-hooks/ordering/buyers/useBuyer";
import useUpdateBuyerAddress from "@workspace/api-hooks/ordering/buyers/useUpdateBuyerAddress";

export default function AccountPage() {
  const {
    data: buyer,
    isLoading: isLoadingBuyer,
    error: buyerError,
  } = useBuyer();
  const updateAddressMutation = useUpdateBuyerAddress();

  const [isEditingAddress, setIsEditingAddress] = useState(false);
  const [addressForm, setAddressForm] = useState<UpdateAddressRequest>({
    street: "",
    city: "",
    province: "",
  });

  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const router = useRouter();

  const handleEditAddress = () => {
    // Parse current address or start fresh
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

  const handleLogout = async () => {
    setIsLoggingOut(true);
    // Simulate logout API call
    await new Promise((resolve) => setTimeout(resolve, 1000));

    // Redirect to home page
    router.push("/");
  };

  // Show loading state while fetching buyer data
  if (isLoadingBuyer) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
          <div className="flex items-center justify-center py-32">
            <Loader2 className="size-8 animate-spin text-primary" />
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  // Show error state if buyer data failed to load
  if (buyerError || !buyer) {
    return (
      <div className="min-h-screen flex flex-col bg-background">
        <Header />
        <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
          <div className="text-center py-32">
            <h2 className="text-2xl font-serif mb-4">Error Loading Account</h2>
            <p className="text-muted-foreground mb-6">
              We couldn't load your account information. Please try again later.
            </p>
            <Button onClick={() => window.location.reload()} variant="outline">
              Retry
            </Button>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="flex-1 container mx-auto px-4 md:px-8 py-16 max-w-5xl">
        <div className="mb-12 md:mb-16">
          <h1 className="font-serif text-5xl md:text-6xl font-light mb-3 text-balance">
            Your Account
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl leading-relaxed">
            Manage your profile information, delivery address, and view your
            order history
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-8">
            {/* Profile Section */}
            <div className="border border-border/40 rounded-lg p-6">
              <div className="flex items-center gap-4 mb-6">
                <div className="size-16 bg-primary/10 rounded-full flex items-center justify-center">
                  <User className="size-8 text-primary" />
                </div>
                <div>
                  <h3 className="font-serif text-xl font-semibold">
                    {buyer.name || "Guest User"}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    Customer ID: <span className="font-mono">{buyer.id}</span>
                  </p>
                </div>
              </div>
            </div>

            {/* Delivery Address Section */}
            <div className="border border-border/40 rounded-lg p-8 hover:bg-secondary/20 transition-colors">
              <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                  <div className="size-10 bg-primary/10 rounded-lg flex items-center justify-center">
                    <MapPin className="size-5 text-primary" />
                  </div>
                  <h3 className="font-serif text-xl font-semibold">
                    Delivery Address
                  </h3>
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
                    <Label
                      htmlFor="street"
                      className="text-sm font-medium mb-2 block"
                    >
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
                      <Label
                        htmlFor="city"
                        className="text-sm font-medium mb-2 block"
                      >
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
                      {updateAddressMutation.isPending
                        ? "Saving..."
                        : "Save Address"}
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
          </div>

          <div className="lg:col-span-1">
            <div className="sticky top-24 space-y-6">
              {/* Navigation Card */}
              <div className="border border-border/40 rounded-lg overflow-hidden divide-y divide-border/40">
                <Link href="/account/orders" className="block">
                  <div className="p-4 hover:bg-secondary/20 transition-colors flex items-center justify-between">
                    <span className="font-medium">Order History</span>
                    <ChevronRight className="size-4 text-muted-foreground" />
                  </div>
                </Link>
              </div>

              {/* Logout Button */}
              <Button
                onClick={handleLogout}
                disabled={isLoggingOut}
                className="w-full border border-border/40 bg-transparent hover:bg-secondary/20 text-foreground justify-center gap-2 h-12 font-medium"
              >
                {isLoggingOut ? (
                  <>
                    <Loader2 className="size-4 animate-spin" />
                    Logging out...
                  </>
                ) : (
                  "Logout"
                )}
              </Button>
            </div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
