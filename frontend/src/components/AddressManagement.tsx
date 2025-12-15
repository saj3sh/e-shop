import { useState, useEffect, useCallback } from "react";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { AddressSelector } from "./AddressSelector";
import { AddressForm } from "./AddressForm";

interface Address {
  id: string;
  line1: string;
  city: string;
  country: string;
  type: string;
}

interface AddressData {
  addresses: Address[];
  defaultShippingAddressId: string | null;
  defaultBillingAddressId: string | null;
}

interface AddressManagementProps {
  showSelector?: boolean;
  onShippingSelect?: (id: string | null) => void;
  onBillingSelect?: (id: string | null) => void;
  selectedShippingId?: string | null;
  selectedBillingId?: string | null;
}

export const AddressManagement = ({
  showSelector = false,
  onShippingSelect,
  onBillingSelect,
  selectedShippingId: externalShippingId,
  selectedBillingId: externalBillingId,
}: AddressManagementProps) => {
  const [addressData, setAddressData] = useState<AddressData | null>(null);
  const [loading, setLoading] = useState(true);
  const [showShippingForm, setShowShippingForm] = useState(false);
  const [showBillingForm, setShowBillingForm] = useState(false);
  const [internalShippingId, setInternalShippingId] = useState<string | null>(
    null
  );
  const [internalBillingId, setInternalBillingId] = useState<string | null>(
    null
  );

  // Use external state if in selector mode, otherwise internal state
  const selectedShippingId = showSelector
    ? externalShippingId!
    : internalShippingId;
  const selectedBillingId = showSelector
    ? externalBillingId!
    : internalBillingId;

  const setSelectedShippingId = showSelector
    ? onShippingSelect!
    : setInternalShippingId;
  const setSelectedBillingId = showSelector
    ? onBillingSelect!
    : setInternalBillingId;

  const fetchAddresses = useCallback(
    async (setDefaults = false) => {
      try {
        const response = await apiClient.get("/addresses");
        setAddressData(response.data);

        // Set defaults on initial load
        if (setDefaults) {
          if (showSelector) {
            // In selector mode (checkout), set defaults via external callbacks
            if (response.data.defaultShippingAddressId) {
              onShippingSelect?.(response.data.defaultShippingAddressId);
            }
            if (response.data.defaultBillingAddressId) {
              onBillingSelect?.(response.data.defaultBillingAddressId);
            }
          } else {
            // In management mode (user profile), use internal state
            setInternalShippingId(response.data.defaultShippingAddressId);
            setInternalBillingId(response.data.defaultBillingAddressId);
          }
        }
      } catch (err) {
        console.error("Failed to fetch addresses", err);
        toast.error("Failed to load addresses");
      } finally {
        setLoading(false);
      }
    },
    [showSelector, onShippingSelect, onBillingSelect]
  );

  useEffect(() => {
    fetchAddresses(true);
  }, [fetchAddresses]);

  const handleCreateAddress = async (address: {
    line1: string;
    city: string;
    country: string;
    type: "Shipping" | "Billing" | "Both";
  }) => {
    // Check for duplicate address with compatible type
    const existingAddress = addressData?.addresses.find(
      (existingAddr) =>
        existingAddr.line1.toLowerCase() === address.line1.toLowerCase() &&
        existingAddr.city.toLowerCase() === address.city.toLowerCase() &&
        existingAddr.country.toLowerCase() === address.country.toLowerCase()
    );

    if (existingAddress) {
      // Check if the existing address can be used for the requested type
      const canUseExisting =
        existingAddress.type === "Both" ||
        existingAddress.type === address.type;

      if (canUseExisting) {
        // Select the existing address instead of creating a duplicate
        if (address.type === "Shipping" || address.type === "Both") {
          setSelectedShippingId(existingAddress.id);
          setShowShippingForm(false);
        }
        if (address.type === "Billing" || address.type === "Both") {
          setSelectedBillingId(existingAddress.id);
          setShowBillingForm(false);
        }
        toast.info("Using existing address");
        return;
      }
      // If types don't match, we'll create a new address below
    }

    const response = await apiClient.post("/addresses", address);

    if (address.type === "Shipping" || address.type === "Both") {
      setSelectedShippingId(response.data.id);
      setShowShippingForm(false);
    }
    if (address.type === "Billing" || address.type === "Both") {
      setSelectedBillingId(response.data.id);
      setShowBillingForm(false);
    }

    await fetchAddresses(false);
    toast.success("Address added successfully");
  };

  const handleSetDefaultAddress = async (
    addressId: string,
    addressType: "Shipping" | "Billing"
  ) => {
    try {
      await apiClient.put(`/addresses/${addressId}/set-default`, {
        addressType,
      });
      await fetchAddresses(false);
      toast.success(`Default ${addressType.toLowerCase()} address updated`);
    } catch (err) {
      console.error("Failed to set default address", err);
      toast.error("Failed to set default address");
    }
  };

  const handleDeleteAddress = async (addressId: string) => {
    try {
      await apiClient.delete(`/addresses/${addressId}`);

      // If the deleted address was selected, clear the selection
      if (selectedShippingId === addressId) {
        setSelectedShippingId(null);
      }
      if (selectedBillingId === addressId) {
        setSelectedBillingId(null);
      }

      await fetchAddresses(false);
      toast.success("Address deleted successfully");
    } catch (err) {
      console.error("Failed to delete address", err);
      toast.error("Failed to delete address. Please try again.");
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading addresses...</div>;
  }

  return (
    <div className="space-y-6">
      {/* Shipping Address Section */}
      <div>
        <h3 className="text-lg font-semibold mb-3">
          {showSelector ? "Shipping Address" : "Shipping Addresses"}
        </h3>
        {showShippingForm ? (
          <AddressForm
            onSubmit={handleCreateAddress}
            onCancel={() => setShowShippingForm(false)}
            addressType="Shipping"
          />
        ) : (
          <AddressSelector
            addresses={addressData?.addresses || []}
            selectedAddressId={selectedShippingId}
            onAddressSelect={setSelectedShippingId}
            onCreateNew={() => setShowShippingForm(true)}
            onSetDefault={(id) => handleSetDefaultAddress(id, "Shipping")}
            onDelete={handleDeleteAddress}
            label={showSelector ? "Select shipping address" : ""}
            addressType="shipping"
            defaultAddressId={addressData?.defaultShippingAddressId || null}
          />
        )}
      </div>

      {/* Billing Address Section */}
      <div>
        <h3 className="text-lg font-semibold mb-3">
          {showSelector ? "Billing Address" : "Billing Addresses"}
        </h3>
        {showBillingForm ? (
          <AddressForm
            onSubmit={handleCreateAddress}
            onCancel={() => setShowBillingForm(false)}
            addressType="Billing"
            shippingAddress={addressData?.addresses.find(
              (a) =>
                (a.type === "Shipping" || a.type === "Both") &&
                (selectedShippingId
                  ? a.id === selectedShippingId
                  : a.id === addressData?.defaultShippingAddressId)
            )}
          />
        ) : (
          <AddressSelector
            addresses={addressData?.addresses || []}
            selectedAddressId={selectedBillingId}
            onAddressSelect={setSelectedBillingId}
            onCreateNew={() => setShowBillingForm(true)}
            onSetDefault={(id) => handleSetDefaultAddress(id, "Billing")}
            onDelete={handleDeleteAddress}
            label={showSelector ? "Select billing address" : ""}
            addressType="billing"
            defaultAddressId={addressData?.defaultBillingAddressId || null}
          />
        )}
      </div>
    </div>
  );
};
