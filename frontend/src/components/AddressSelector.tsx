import { useState } from "react";

interface Address {
  id: string;
  line1: string;
  city: string;
  country: string;
  type: string;
}

interface AddressSelectorProps {
  addresses: Address[];
  selectedAddressId: string | null;
  onAddressSelect: (addressId: string | null) => void;
  onCreateNew: () => void;
  onSetDefault?: (addressId: string) => void;
  label: string;
  addressType: "shipping" | "billing";
  defaultAddressId: string | null;
}

export const AddressSelector = ({
  addresses,
  selectedAddressId,
  onAddressSelect,
  onCreateNew,
  onSetDefault,
  label,
  addressType,
  defaultAddressId,
}: AddressSelectorProps) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const filteredAddresses = addresses.filter(
    (addr) =>
      addr.type.toLowerCase() === addressType ||
      addr.type.toLowerCase() === "both"
  );

  const selectedAddress = filteredAddresses.find(
    (addr) => addr.id === selectedAddressId
  );

  return (
    <div className="space-y-3">
      <label className="block text-sm font-medium">{label}</label>

      {selectedAddress && !isExpanded ? (
        <div className="border rounded-md p-4 bg-gray-50">
          <div className="flex justify-between items-start">
            <div>
              <p className="font-medium">{selectedAddress.line1}</p>
              <p className="text-sm text-gray-600">
                {selectedAddress.city}, {selectedAddress.country}
              </p>
            </div>
            <button
              type="button"
              onClick={() => setIsExpanded(true)}
              className="text-sm text-blue-600 hover:text-blue-700"
            >
              change
            </button>
          </div>
        </div>
      ) : (
        <div className="space-y-2">
          {filteredAddresses.length === 0 ? (
            <p className="text-sm text-gray-600">
              no {addressType} addresses found
            </p>
          ) : (
            filteredAddresses.map((address) => (
              <div
                key={address.id}
                className={`border rounded-md p-4 transition-all ${
                  selectedAddressId === address.id
                    ? "border-blue-600 bg-blue-50"
                    : "border-gray-200 hover:border-gray-300"
                }`}
              >
                <div
                  onClick={() => {
                    onAddressSelect(address.id);
                    setIsExpanded(false);
                  }}
                  className="cursor-pointer"
                >
                  <p className="font-medium">{address.line1}</p>
                  <p className="text-sm text-gray-600">
                    {address.city}, {address.country}
                  </p>
                </div>
                {onSetDefault && defaultAddressId !== address.id && (
                  <button
                    type="button"
                    onClick={(e) => {
                      e.stopPropagation();
                      onSetDefault(address.id);
                    }}
                    className="mt-2 text-xs text-blue-600 hover:text-blue-700 underline"
                  >
                    set as default
                  </button>
                )}
                {defaultAddressId === address.id && (
                  <span className="mt-2 text-xs text-green-600 font-medium inline-block">
                    ✓ default
                  </span>
                )}
              </div>
            ))
          )}

          <button
            type="button"
            onClick={onCreateNew}
            className="w-full border-2 border-dashed border-gray-300 rounded-md p-4 text-sm text-gray-600 hover:border-gray-400 hover:text-gray-700 transition-all"
          >
            + add new {addressType} address
          </button>
        </div>
      )}
    </div>
  );
};
