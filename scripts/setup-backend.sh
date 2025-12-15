#!/bin/bash

# backend setup script

echo "setting up backend..."

cd backend

echo "restoring nuget packages..."
dotnet restore

echo "building solution..."
dotnet build

echo "creating database..."
cd src/EShop.Api
dotnet ef database update

echo "backend setup complete!"
echo "run 'dotnet run' in backend/src/EShop.Api to start the api"
