# Sadece ilk kurulumda bir kez çalıştır
Write-Host "Veritabanlari ilk kez kuruluyor..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot\ECommerce.AppHost"

kubectl apply -f aspirate-output/db-pvcs.yaml
kubectl apply -f aspirate-output/db-init-configmaps.yaml
kubectl apply -f aspirate-output/postgres-databases.yaml

Write-Host "Veritabanlari kuruldu!" -ForegroundColor Green
Set-Location $PSScriptRoot
