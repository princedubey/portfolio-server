# Azure Deployment Script
Write-Host "Building the application..." -ForegroundColor Green
dotnet build --configuration Release

Write-Host "Publishing the application..." -ForegroundColor Green
dotnet publish --configuration Release --output ./publish

Write-Host "Creating deployment package..." -ForegroundColor Green
Compress-Archive -Path ./publish/* -DestinationPath ./deployment.zip -Force

Write-Host "Deployment package created: deployment.zip" -ForegroundColor Green
Write-Host "You can now deploy this zip file to Azure App Service" -ForegroundColor Yellow 