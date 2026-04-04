# Church Roster - Start Backend and Frontend

Write-Host "=== Church Roster System Startup ===" -ForegroundColor Cyan
Write-Host ""

# Start Backend
Write-Host "1️⃣ Starting Backend API..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\backend\ChurchRoster.Api'; Write-Host '🚀 Starting Backend API on http://localhost:8080' -ForegroundColor Green; dotnet run"

Start-Sleep -Seconds 2

# Start Frontend
Write-Host "2️⃣ Starting Frontend..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\frontend'; Write-Host '🚀 Starting Frontend on http://localhost:5173' -ForegroundColor Green; npm run dev"

Write-Host ""
Write-Host "✅ Both servers starting in separate windows..." -ForegroundColor Green
Write-Host ""
Write-Host "📝 Access your app at:" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "   Backend:  http://localhost:8080" -ForegroundColor White
Write-Host "   Test:     http://localhost:5173/test-accept.html" -ForegroundColor White
Write-Host ""
Write-Host "💡 Keep both terminal windows open while developing" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit this window..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
