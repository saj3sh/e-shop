#!/bin/bash

# creates initial ef core migration

cd backend/src/EShop.Api

echo "creating initial migration..."
dotnet ef migrations add InitialCreate --project ../EShop.Infrastructure

echo "migration created successfully!"
