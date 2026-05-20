Write-Host "Build basliyor..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot\ECommerce.AppHost"
aspirate build --non-interactive

Write-Host "Kubernetes manifest'leri uygulanıyor..." -ForegroundColor Cyan
kubectl apply -k aspirate-output --prune=false

Write-Host "Siparişler yedekleniyor..." -ForegroundColor Yellow
$backupFile = "$env:TEMP\orders_backup.sql"
kubectl exec deployment/order-db -- pg_dump -U admin --data-only -t `"Orders`" -t `"OrderItems`" OrderDb > $backupFile 2>$null

if ((Get-Item $backupFile -ErrorAction SilentlyContinue).Length -gt 0) {
    Write-Host "Yedek alındı: $backupFile" -ForegroundColor Green
    $hasBackup = $true
} else {
    Write-Host "Yedek alınamadı veya sipariş yok." -ForegroundColor Yellow
    $hasBackup = $false
}

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

Write-Host "API'lar baslatılıyor, bekleniyor (30s)..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

if ($hasBackup) {
    Write-Host "Siparişler geri yükleniyor..." -ForegroundColor Yellow
    Get-Content $backupFile | kubectl exec -i deployment/order-db -- psql -U admin OrderDb
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Siparişler geri yüklendi." -ForegroundColor Green
    } else {
        Write-Host "UYARI: Siparişler geri yüklenemedi! Yedek dosyası: $backupFile" -ForegroundColor Red
    }
    Remove-Item $backupFile -ErrorAction SilentlyContinue
}

Write-Host "Tamamlandi!" -ForegroundColor Green
Set-Location $PSScriptRoot
