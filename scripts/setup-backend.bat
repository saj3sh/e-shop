@echo off

REM backend setup script for windows

echo Setting up backend...

cd backend

echo Restoring NuGet packages...
dotnet restore

echo Building solution...
dotnet build

echo Creating database...
cd src\EShop.Api
dotnet ef database update

echo Backend setup complete!
echo Run 'dotnet run' in backend\src\EShop.Api to start the API

pause
