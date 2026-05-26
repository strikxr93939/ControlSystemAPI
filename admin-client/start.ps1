# Запуск Admin Panel
# В терминале: powershell -ExecutionPolicy Bypass -File .\start.ps1

trap {
    Write-Host "`nОШИБКА: $_" -ForegroundColor Red
    Write-Host "Нажмите Enter для выхода..."
    Read-Host
    exit 1
}

Set-Location $PSScriptRoot

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "ОШИБКА: Node.js не найден! Скачайте с https://nodejs.org" -ForegroundColor Red
    Read-Host "Нажмите Enter для выхода"
    exit 1
}

Write-Host "Node: $(node --version) | npm: $(npm --version)" -ForegroundColor Gray

# Фикс OpenSSL для Node 17+ (react-scripts 5.x)
$env:NODE_OPTIONS = "--openssl-legacy-provider"
$env:PORT = "3000"
$env:BROWSER = "none"

Write-Host ""
Write-Host "Установка зависимостей..." -ForegroundColor Cyan
npm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "npm install упал. Попробуйте: npm install --legacy-peer-deps" -ForegroundColor Yellow
    Read-Host "Нажмите Enter для выхода"
    exit 1
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host " Admin Panel → http://localhost:3000" -ForegroundColor Green
Write-Host " Swagger → http://localhost:5186/swagger" -ForegroundColor Green
Write-Host " Ctrl+C для остановки" -ForegroundColor Gray
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

npm start

Write-Host "Сервер остановлен. Нажмите Enter..."
Read-Host