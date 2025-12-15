@echo off

REM frontend setup script for windows

echo Setting up frontend...

cd frontend

echo Installing npm packages...
npm ci

echo Frontend setup complete!
echo Run 'npm run dev' in frontend\ to start the dev server

pause
