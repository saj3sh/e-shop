import { useState } from "react";
import { countryByCode } from "../lib/countries";

interface AddressFormProps {
  onSubmit: (address: {
    line1: string;
    city: string;
    country: string;
    type: "Shipping" | "Billing" | "Both";
  }) => Promise<void>;
  onCancel: () => void;
  addressType: "Shipping" | "Billing";
}

export const AddressForm = ({
  onSubmit,
  onCancel,
  addressType,
}: AddressFormProps) => {
  const [formData, setFormData] = useState({
    line1: "",
    city: "",
    country: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const countries = Object.entries(countryByCode).map(([code, name]) => ({
    code,
    name,
  }));

  const handleSubmit = async () => {
    setError("");

    if (!formData.line1 || !formData.city || !formData.country) {
      setError("All fields are required");
      return;
    }

    setLoading(true);
    try {
      await onSubmit({
        ...formData,
        type: addressType,
      });
    } catch (err: any) {
      setError(err.response?.data?.error || "Failed to create address");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="border rounded-md p-4 space-y-4">
      <h3 className="font-semibold">Add New {addressType} Address</h3>

      <div>
        <label className="block text-sm font-medium mb-1">
          Street Address *
        </label>
        <input
          type="text"
          value={formData.line1}
          onChange={(e) => setFormData({ ...formData, line1: e.target.value })}
          placeholder="123 Main Street"
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">City *</label>
        <input
          type="text"
          value={formData.city}
          onChange={(e) => setFormData({ ...formData, city: e.target.value })}
          placeholder="New York"
          className="w-full px-3 py-2 border rounded-md"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Country *</label>
        <select
          value={formData.country}
          onChange={(e) =>
            setFormData({ ...formData, country: e.target.value })
          }
          className="w-full px-3 py-2 border rounded-md"
        >
          <option value="">Select a country</option>
          {countries.map((country) => (
            <option key={country.code} value={country.code}>
              {country.name}
            </option>
          ))}
        </select>
      </div>

      {error && (
        <div className="bg-red-50 text-red-600 p-3 rounded-md text-sm">
          {error}
        </div>
      )}

      <div className="flex gap-2">
        <button
          type="button"
          onClick={handleSubmit}
          disabled={loading}
          className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-md font-semibold disabled:bg-gray-400"
        >
          {loading ? "saving..." : "save address"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-700 py-2 px-4 rounded-md font-semibold"
        >
          cancel
        </button>
      </div>
    </div>
  );
};
