# E-Shop Application

> Full-stack e-commerce platform with ASP.NET Core backend and React TypeScript frontend. Features JWT authentication, DDD architecture with domain events, order management with auto-generated tracking numbers, and IMemoryCache implementation for optimal performance.

## Overview

A full-stack e-commerce application built with ASP.NET Core and React with TypeScript. The application allows users to browse products, manage their cart, place orders, and track order status. Admin users can manage orders and mark them as complete.

## Technical Stack

### Backend

- **Framework**: ASP.NET Core (NET 10.0)
- **Language**: C#
- **Database**: SQLite3
- **Architecture**: Domain-Driven Design (DDD) with Clean Architecture
  - Domain Layer: Core business logic and entities
  - Application Layer: Business logic and CQRS patterns
  - Infrastructure Layer: Data persistence, caching, and external services
  - API Layer: RESTful endpoints and authentication

### Frontend

- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **State Management**: Zustand
- **Routing**: React Router v6
- **HTTP Client**: Axios with React Query
- **UI Components**: Custom components with Headless UI patterns

### Key Features

- ✅ **JWT-based Authorization** with access tokens and refresh tokens for secure authentication
- ✅ Email-based authentication (no password required)
- ✅ **Activity Logging** using Domain-Driven Events for comprehensive audit trails
- ✅ **IMemoryCache Implementation** for high-performance data caching
- ✅ Product browsing with pagination (25 items per page)
- ✅ Product search functionality
- ✅ Shopping cart management
- ✅ Order placement with address selection/creation
- ✅ Automatic tracking number generation (Format: "Unq" + 9 digits + country code)
- ✅ User dashboard with order history
- ✅ Admin dashboard for order management

## Prerequisites

### Required Software

- **.NET SDK 10.0 or later** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18.x or later** - [Download](https://nodejs.org/)
- **npm 9.x or later** (included with Node.js)

### Verification Commands

```bash
# Check .NET version
dotnet --version

# Check Node.js version
node --version

# Check npm version
npm --version
```

## Installation & Setup

### 1. Extract the Project

Extract the provided ZIP file to a location on your Windows machine.

### 2. Backend Setup

#### Navigate to Backend Directory

```bash
cd backend
```

#### Restore Dependencies

```bash
dotnet restore
```

#### Database Initialization

The database will be automatically created and seeded with data from the Excel dataset on first run. The SQLite database file will be created at:

```
backend/src/EShop.Api/data/app.db
```

**Note**: The application expects the dataset file at `backend/data/dataset.xlsx`. Ensure this file is present before running the application.

#### Run the Backend

```bash
dotnet run --project src/EShop.Api/EShop.Api.csproj
```

The API will start on:

- **HTTPS**: https://localhost:7296
- **HTTP**: http://localhost:5296

You should see output indicating:

- Database creation/migration
- Data seeding from Excel file
- API endpoints ready

#### API Documentation

Once running, access the Swagger documentation at:

```
https://localhost:7296/swagger
```

### 3. Frontend Setup

#### Navigate to Frontend Directory

Open a new terminal/command prompt window:

```bash
cd frontend
```

#### Install Dependencies

```bash
npm install
```

This will install all required packages including:

- React and React DOM
- TypeScript
- Vite (build tool)
- Tailwind CSS
- React Router
- Axios and React Query
- Zustand (state management)
- All development dependencies

#### Configure API Endpoint

The frontend is pre-configured to connect to the backend at `http://localhost:5296`. If you need to change this, edit:

```
frontend/src/lib/apiClient.ts
```

#### Run the Frontend

```bash
npm run dev
```

The application will start on:

```
http://localhost:5173
```

### 4. Access the Application

Open your web browser and navigate to:

```
http://localhost:5173
```

## User Accounts

### Admin Account

- **Email**: admin@eshop.com
- **Permissions**: Can view all orders and mark them as complete

### Regular Users

- Users can create new accounts directly from the login page
- Only email is required (no password)
- Users can also log in with any email from the seeded customer data

### Sample Customer Emails (from dataset)

Any customer email from the imported dataset can be used to log in. The admin account is pre-configured.

## Application Features

### User Features

1. **Registration & Login**

   - Email-only authentication
   - New users can register with full customer information
   - Address management during registration

2. **Product Browsing**

   - View all products with pagination (25 per page)
   - Search products by name
   - View detailed product information
   - Add products to cart

3. **Shopping Cart**

   - Add/remove items
   - Update quantities
   - View total price
   - Proceed to checkout

4. **Checkout Process**

   - Select or add delivery address
   - Review order details
   - Place order

5. **Order Management**

   - View order history
   - Track order status
   - View tracking numbers (Format: UnqXXXXXXXXXCC)
   - Check order details

6. **User Profile**
   - View personal information
   - Manage addresses
   - View order summary

### Admin Features

1. **Order Dashboard**

   - View all incomplete orders
   - Filter and search orders
   - Mark orders as complete
   - View order details and customer information

2. **Activity Logs**
   - View system activity logs
   - Track user actions
   - Monitor order status changes

## Project Structure

```
e-shop/
├── backend/
│   ├── data/
│   │   └── dataset.xlsx          # Excel data source
│   ├── src/
│   │   ├── EShop.Api/            # API layer
│   │   │   ├── Controllers/      # REST endpoints
│   │   │   ├── data/             # SQLite database
│   │   │   └── Program.cs        # Application entry point
│   │   ├── EShop.Application/    # Business logic
│   │   │   ├── Auth/             # Authentication logic
│   │   │   ├── Products/         # Product features
│   │   │   ├── Orders/           # Order management
│   │   │   └── Customers/        # Customer management
│   │   ├── EShop.Domain/         # Domain models
│   │   │   ├── Products/         # Product entities
│   │   │   ├── Orders/           # Order entities
│   │   │   └── Customers/        # Customer entities
│   │   └── EShop.Infrastructure/ # Data access
│   │       ├── Persistence/      # EF Core & SQLite
│   │       ├── Caching/          # In-memory cache
│   │       └── Security/         # JWT authentication
│   └── tests/
│       └── EShop.Tests/          # Unit tests
└── frontend/
    ├── src/
    │   ├── app/                  # App configuration
    │   ├── components/           # React components
    │   ├── pages/                # Page components
    │   ├── stores/               # Zustand stores
    │   └── lib/                  # Utilities & API client
    ├── package.json              # Dependencies
    └── vite.config.ts            # Vite configuration
```

## Database Schema

The application uses SQLite3 with the following main entities:

### Tables

- **Customers**: User account information
- **Addresses**: Customer delivery addresses with country codes
- **Products**: Product catalog with pricing
- **Orders**: Order headers with status and tracking numbers
- **OrderItems**: Individual items in each order
- **ActivityLogs**: Audit trail of system activities

### Key Relationships

- Customers have many Addresses
- Customers have many Orders
- Orders have many OrderItems
- Orders reference Addresses for delivery
- OrderItems reference Products

### Tracking Number Format

- Pattern: `UnqXXXXXXXXXCC`
- Example: `Unq123456789US`
- Where:
  - `Unq` = Fixed prefix
  - `XXXXXXXXX` = 9 random digits
  - `CC` = Two-letter country code from delivery address

## API Endpoints

### Authentication

- `POST /api/auth/login` - Email-based login
- `POST /api/auth/register` - Create new account
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user

### Products

- `GET /api/products` - List products (paginated, searchable)
- `GET /api/products/{id}` - Get product details

### Orders

- `GET /api/orders` - Get user's orders
- `GET /api/orders/{id}` - Get order details
- `POST /api/orders` - Create new order

### Customers

- `GET /api/customers/me` - Get current user profile
- `PUT /api/customers/me` - Update user profile

### Addresses

- `GET /api/addresses` - Get user addresses
- `POST /api/addresses` - Create new address
- `PUT /api/addresses/{id}` - Update address
- `DELETE /api/addresses/{id}` - Delete address

### Admin

- `GET /api/admin/orders` - Get all incomplete orders
- `PATCH /api/admin/orders/{id}/complete` - Mark order complete

### Activity Logs

- `GET /api/activitylogs` - Get activity logs (admin only)

## Configuration

### Backend Configuration

Edit `backend/src/EShop.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./data/app.db"
  },
  "AdminEmail": "admin@eshop.com",
  "DataImportPath": "../../data/dataset.xlsx",
  "Cache": {
    "ProductCacheMinutes": 5,
    "SearchCacheMinutes": 2
  }
}
```

### Frontend Configuration

Edit `frontend/src/lib/apiClient.ts` to change API base URL if needed.

## Building for Production

### Backend

```bash
cd backend
dotnet publish src/EShop.Api/EShop.Api.csproj -c Release -o ./publish
```

### Frontend

```bash
cd frontend
npm run build
```

Build output will be in `frontend/dist/`

## Troubleshooting

### Backend Issues

**Issue**: Database not created

- **Solution**: Ensure the `backend/data/dataset.xlsx` file exists
- Check file path in `appsettings.json`
- Delete `backend/src/EShop.Api/data/app.db` and restart to regenerate

**Issue**: Port already in use

- **Solution**: Modify ports in `backend/src/EShop.Api/Properties/launchSettings.json`

**Issue**: .NET SDK not found

- **Solution**: Install .NET SDK 10.0 or later from [Microsoft's website](https://dotnet.microsoft.com/download)

### Frontend Issues

**Issue**: Dependencies installation fails

- **Solution**: Delete `node_modules` folder and `package-lock.json`, then run `npm install` again

**Issue**: Cannot connect to API

- **Solution**: Ensure backend is running on http://localhost:5296
- Check CORS settings in backend
- Verify API URL in `frontend/src/lib/apiClient.ts`

**Issue**: Port 5173 already in use

- **Solution**: Vite will automatically try the next available port
- Or specify a different port: `npm run dev -- --port 3000`

### General Issues

**Issue**: Application runs but no data appears

- **Solution**: Check that data import from Excel completed successfully
- Look for error messages in backend console output
- Verify `backend/data/dataset.xlsx` is present and valid

## Technical Highlights

### Security & Authentication

- **JWT-based Authorization**: Secure token-based authentication system
  - Access tokens for API authorization (15-minute expiration)
  - Refresh tokens for seamless token renewal (7-day expiration)
  - Token expiration and automatic renewal mechanism
- **Activity Logging via Domain Events**: Event-driven architecture for tracking user actions which can be extended in future to give real time notifications to customer regarding orders and tracking information.

### Performance

- **IMemoryCache Implementation**: ASP.NET Core's built-in caching for optimal performance
  - Product catalog caching (5-minute TTL)
  - Search results caching (2-minute TTL)
  - Customer data caching (10-minute TTL)
  - Order data caching (5-minute TTL)
  - Automatic cache invalidation on data updates
- Pagination for large datasets (25 items per page)
- Optimized database queries with Entity Framework Core
- React Query for efficient data fetching and caching on frontend

### Performance

- In-memory caching for frequently accessed data
- Pagination for large datasets (25 items per page)
- Optimized database queries with Entity Framework Core
- React Query for efficient data fetching and caching

### Code Quality

- Clean Architecture with separation of concerns
- CQRS pattern for complex operations
- Domain-Driven Design principles
- TypeScript for type safety
- Comprehensive error handling

## Development Notes

### Backend Architecture

- **Clean Architecture**: Separation of concerns across layers
- **Domain-Driven Design**: Rich domain models with business logic
- **CQRS Pattern**: Command/Query separation for complex operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management

### Frontend Architecture

- **Component-Based**: Reusable React components
- **State Management**: Zustand for global state (auth, cart)
- **Server State**: React Query for API data management
- **Type Safety**: Full TypeScript coverage
- **Responsive Design**: Tailwind CSS for mobile-first design

## Testing

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```
