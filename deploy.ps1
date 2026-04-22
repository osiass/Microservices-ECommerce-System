Write-Host "Build basliyor..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot\ECommerce.AppHost"
aspirate build --non-interactive

Write-Host "Kubernetes yeniden baslatiliyor..." -ForegroundColor Cyan
kubectl rollout restart deployments

Write-Host "Tamamlandi!" -ForegroundColor Green
Set-Location $PSScriptRoot
