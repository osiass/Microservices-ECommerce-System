Write-Host "Build basliyor..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot\ECommerce.AppHost"
aspirate build --non-interactive

Write-Host "Kubernetes manifest'leri uygulanıyor..." -ForegroundColor Cyan
kubectl apply -k aspirate-output

Write-Host "API'lar yeniden baslatiliyor (veritabanlari dokunulmuyor)..." -ForegroundColor Cyan
kubectl rollout restart deployment/catalog-api
kubectl rollout restart deployment/basket-api
kubectl rollout restart deployment/discount-api
kubectl rollout restart deployment/identity-api
kubectl rollout restart deployment/inventory-api
kubectl rollout restart deployment/order-api
kubectl rollout restart deployment/payment-api
kubectl rollout restart deployment/notification-api
kubectl rollout restart deployment/gateway
kubectl rollout restart deployment/web-ui

Write-Host "Tamamlandi!" -ForegroundColor Green
Set-Location $PSScriptRoot
