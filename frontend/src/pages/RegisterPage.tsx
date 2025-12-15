import { useState, useEffect } from "react";
import { useNavigate, useLocation, Link } from "react-router-dom";
import toast from "react-hot-toast";
import { apiClient } from "../lib/apiClient";
import { countryByCode } from "../lib/countries";
import { Button, Input, Select, Card, Alert } from "../components/ui";

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
    shippingCountryCode: "",
    billingAddress: "",
    billingCity: "",
    billingCountryCode: "",
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

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
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
        Email: formData.email,
        FirstName: formData.firstName,
        LastName: formData.lastName,
        Phone: formData.phone,
        ShippingAddress: formData.shippingAddress,
        ShippingCity: formData.shippingCity,
        ShippingCountryCode: formData.shippingCountryCode,
        BillingAddress: sameAsShipping
          ? formData.shippingAddress
          : formData.billingAddress,
        BillingCity: sameAsShipping
          ? formData.shippingCity
          : formData.billingCity,
        BillingCountryCode: sameAsShipping
          ? formData.shippingCountryCode
          : formData.billingCountryCode,
      };

      await apiClient.post("/auth/register", registrationData);
      toast.success("Registration successful!");
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
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center px-4 py-8">
      <Card className="max-w-2xl w-full" padding="lg">
        <div className="text-center mb-6">
          <h1 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent mb-2">
            Create an Account
          </h1>
          <p className="text-gray-600">Join EShop to start shopping</p>
        </div>

        {redirectMessage && (
          <Alert variant="info" className="mb-6">
            {redirectMessage}
          </Alert>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Personal Information */}
          <div>
            <h2 className="text-lg font-semibold mb-3 text-gray-900">
              Personal Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                type="text"
                name="firstName"
                label="First Name"
                value={formData.firstName}
                onChange={handleChange}
                required
              />
              <Input
                type="text"
                name="lastName"
                label="Last Name"
                value={formData.lastName}
                onChange={handleChange}
                required
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <Input
                type="email"
                name="email"
                label="Email"
                value={formData.email}
                onChange={handleChange}
                required
              />
              <Input
                type="tel"
                name="phone"
                label="Phone"
                value={formData.phone}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          {/* Shipping Address */}
          <div>
            <h2 className="text-lg font-semibold mb-3 text-gray-900">
              Shipping Address
            </h2>
            <div className="space-y-4">
              <Input
                type="text"
                name="shippingAddress"
                label="Street Address"
                value={formData.shippingAddress}
                onChange={handleChange}
                required
              />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  type="text"
                  name="shippingCity"
                  label="City"
                  value={formData.shippingCity}
                  onChange={handleChange}
                  required
                />
                <Select
                  name="shippingCountryCode"
                  label="Country"
                  value={formData.shippingCountryCode}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select a country</option>
                  {Object.entries(countryByCode).map(([code, name]) => (
                    <option key={code} value={code}>
                      {name}
                    </option>
                  ))}
                </Select>
              </div>
            </div>
          </div>

          {/* Billing Address */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <h2 className="text-lg font-semibold text-gray-900">
                Billing Address
              </h2>
              <label className="flex items-center text-sm cursor-pointer">
                <input
                  type="checkbox"
                  checked={sameAsShipping}
                  onChange={(e) => setSameAsShipping(e.target.checked)}
                  className="mr-2 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span className="text-gray-700">Same as shipping</span>
              </label>
            </div>

            {!sameAsShipping && (
              <div className="space-y-4">
                <Input
                  type="text"
                  name="billingAddress"
                  label="Street Address"
                  value={formData.billingAddress}
                  onChange={handleChange}
                  required={!sameAsShipping}
                />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input
                    type="text"
                    name="billingCity"
                    label="City"
                    value={formData.billingCity}
                    onChange={handleChange}
                    required={!sameAsShipping}
                  />
                  <Select
                    name="billingCountryCode"
                    label="Country"
                    value={formData.billingCountryCode}
                    onChange={handleChange}
                    required={!sameAsShipping}
                  >
                    <option value="">Select a country</option>
                    {Object.entries(countryByCode).map(([code, name]) => (
                      <option key={code} value={code}>
                        {name}
                      </option>
                    ))}
                  </Select>
                </div>
              </div>
            )}
          </div>

          {error && <Alert variant="error">{error}</Alert>}

          <Button
            type="submit"
            isLoading={loading}
            className="w-full"
            size="lg"
          >
            {loading ? "Creating account..." : "Create account"}
          </Button>
        </form>

        <p className="text-center text-sm text-gray-600 mt-6">
          Already have an account?{" "}
          <Link
            to="/login"
            className="text-blue-600 hover:text-blue-700 font-medium"
          >
            Login here
          </Link>
        </p>
      </Card>
    </div>
  );
};
