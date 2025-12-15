import { useState, useEffect } from "react";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { apiClient } from "../lib/apiClient";

export const RegisterPage = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const prefilledEmail = (location.state as any)?.email || "";
  const redirectMessage = (location.state as any)?.message || "";

  const [formData, setFormData] = useState({
    email: prefilledEmail,
    firstName: "",
    lastName: "",
    phone: "",
    shippingAddress: "",
    shippingCity: "",
    shippingCountry: "",
    billingAddress: "",
    billingCity: "",
    billingCountry: "",
  });
  const [sameAsShipping, setSameAsShipping] = useState(true);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Update email if pre-filled from navigation state
  useEffect(() => {
    if (prefilledEmail) {
      setFormData((prev) => ({ ...prev, email: prefilledEmail }));
    }
  }, [prefilledEmail]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const registrationData = {
        ...formData,
        billingAddress: sameAsShipping
          ? formData.shippingAddress
          : formData.billingAddress,
        billingCity: sameAsShipping
          ? formData.shippingCity
          : formData.billingCity,
        billingCountry: sameAsShipping
          ? formData.shippingCountry
          : formData.billingCountry,
      };

      await apiClient.post("/auth/register", registrationData);
      navigate("/login", {
        state: { message: "registration successful! please login" },
      });
    } catch (err: any) {
      setError(err.response?.data?.error || "registration failed, try again");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4 py-8">
      <div className="max-w-2xl w-full bg-white rounded-lg shadow-md p-8">
        <h1 className="text-2xl font-bold text-center mb-2">
          Create an Account
        </h1>
        <p className="text-gray-600 text-center mb-6">
          join eshop to start shopping
        </p>

        {redirectMessage && (
          <div className="bg-blue-50 border border-blue-200 text-blue-700 px-4 py-3 rounded-md text-sm">
            {redirectMessage}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Personal Information */}
          <div>
            <h2 className="text-lg font-semibold mb-3">Personal Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  First Name *
                </label>
                <input
                  type="text"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last Name *
                </label>
                <input
                  type="text"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email *
                </label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Phone *
                </label>
                <input
                  type="tel"
                  name="phone"
                  value={formData.phone}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>

          {/* Shipping Address */}
          <div>
            <h2 className="text-lg font-semibold mb-3">Shipping Address</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Street Address *
                </label>
                <input
                  type="text"
                  name="shippingAddress"
                  value={formData.shippingAddress}
                  onChange={handleChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    City *
                  </label>
                  <input
                    type="text"
                    name="shippingCity"
                    value={formData.shippingCity}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Country *
                  </label>
                  <input
                    type="text"
                    name="shippingCountry"
                    value={formData.shippingCountry}
                    onChange={handleChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Billing Address */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <h2 className="text-lg font-semibold">Billing Address</h2>
              <label className="flex items-center text-sm">
                <input
                  type="checkbox"
                  checked={sameAsShipping}
                  onChange={(e) => setSameAsShipping(e.target.checked)}
                  className="mr-2"
                />
                same as shipping
              </label>
            </div>

            {!sameAsShipping && (
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Street Address *
                  </label>
                  <input
                    type="text"
                    name="billingAddress"
                    value={formData.billingAddress}
                    onChange={handleChange}
                    required={!sameAsShipping}
                    className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      City *
                    </label>
                    <input
                      type="text"
                      name="billingCity"
                      value={formData.billingCity}
                      onChange={handleChange}
                      required={!sameAsShipping}
                      className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Country *
                    </label>
                    <input
                      type="text"
                      name="billingCountry"
                      value={formData.billingCountry}
                      onChange={handleChange}
                      required={!sameAsShipping}
                      className="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                </div>
              </div>
            )}
          </div>

          {error && (
            <div className="bg-red-50 text-red-600 p-3 rounded-md text-sm">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? "creating account..." : "create account"}
          </button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          already have an account?{" "}
          <Link to="/login" className="text-blue-600 hover:underline">
            login here
          </Link>
        </p>
      </div>
    </div>
  );
};
