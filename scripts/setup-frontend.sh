#!/bin/bash

# frontend setup script

echo "setting up frontend..."

cd frontend

echo "installing npm packages..."
npm ci

echo "frontend setup complete!"
echo "run 'npm run dev' in frontend/ to start the dev server"
