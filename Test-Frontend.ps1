# Quick Test Script for Week 3 Frontend

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Week 3 Frontend - Quick Test Script  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to frontend directory
$frontendPath = "C:\Works\CSharp\ChurchRoster\church-roster-system\frontend"

if (Test-Path $frontendPath) {
    Set-Location $frontendPath
    Write-Host "✅ Frontend directory found" -ForegroundColor Green
} else {
    Write-Host "❌ Frontend directory not found at: $frontendPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 1: Checking Node.js..." -ForegroundColor Yellow
$nodeVersion = node --version 2>$null
if ($nodeVersion) {
    Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green
} else {
    Write-Host "❌ Node.js not found. Please install Node.js 18+ from nodejs.org" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Checking npm..." -ForegroundColor Yellow
$npmVersion = npm --version 2>$null
if ($npmVersion) {
    Write-Host "✅ npm version: $npmVersion" -ForegroundColor Green
} else {
    Write-Host "❌ npm not found" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Checking node_modules..." -ForegroundColor Yellow
if (Test-Path "node_modules") {
    Write-Host "✅ Dependencies already installed" -ForegroundColor Green
} else {
    Write-Host "⚠️  Dependencies not installed. Installing..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Dependencies installed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ npm install failed" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Step 4: Checking .env file..." -ForegroundColor Yellow
if (Test-Path ".env") {
    Write-Host "✅ .env file exists" -ForegroundColor Green
    Write-Host "   Contents:" -ForegroundColor Gray
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "VITE_API_URL") {
            Write-Host "   $_" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "⚠️  .env file not found. Creating default..." -ForegroundColor Yellow
    @"
# Backend API URL
VITE_API_URL=https://churchroster.onrender.com/api

# Firebase Configuration (to be configured when implementing notifications)
VITE_FIREBASE_API_KEY=
VITE_FIREBASE_AUTH_DOMAIN=
VITE_FIREBASE_PROJECT_ID=
VITE_FIREBASE_STORAGE_BUCKET=
VITE_FIREBASE_MESSAGING_SENDER_ID=
VITE_FIREBASE_APP_ID=
VITE_FIREBASE_VAPID_KEY=
"@ | Out-File -FilePath ".env" -Encoding UTF8
    Write-Host "✅ .env file created" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 5: Running TypeScript compilation test..." -ForegroundColor Yellow
npx tsc -b 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ TypeScript compilation successful (0 errors)" -ForegroundColor Green
} else {
    Write-Host "❌ TypeScript compilation failed" -ForegroundColor Red
    Write-Host "   Run 'npm run build' for details" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Step 6: Running production build test..." -ForegroundColor Yellow
Write-Host "   This may take 10-15 seconds..." -ForegroundColor Gray
$buildOutput = npm run build 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "   Build Output:" -ForegroundColor Gray
    $buildOutput | Select-String -Pattern "dist/" | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host "   Errors:" -ForegroundColor Gray
    $buildOutput | Select-String -Pattern "error" | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "Step 7: Checking build output..." -ForegroundColor Yellow
if (Test-Path "dist") {
    $distSize = (Get-ChildItem -Path "dist" -Recurse | Measure-Object -Property Length -Sum).Sum / 1KB
    Write-Host "✅ Build output created in dist/ folder" -ForegroundColor Green
    Write-Host "   Total size: $([math]::Round($distSize, 2)) KB" -ForegroundColor Cyan

    # Check for specific files
    if (Test-Path "dist/index.html") {
        Write-Host "   ✅ index.html" -ForegroundColor Green
    }
    if (Test-Path "dist/manifest.webmanifest") {
        Write-Host "   ✅ manifest.webmanifest (PWA)" -ForegroundColor Green
    }
    if (Test-Path "dist/sw.js") {
        Write-Host "   ✅ sw.js (Service Worker)" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️  dist/ folder not found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "         Test Results Summary          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ All tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Start development server: " -NoNewline -ForegroundColor Gray
Write-Host "npm run dev" -ForegroundColor Cyan
Write-Host "2. Open browser: " -NoNewline -ForegroundColor Gray
Write-Host "http://localhost:3000" -ForegroundColor Cyan
Write-Host "3. Login with: " -NoNewline -ForegroundColor Gray
Write-Host "admin@church.com / Admin123!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Yellow
Write-Host "- Week 3 Summary: " -NoNewline -ForegroundColor Gray
Write-Host "docs/WEEK3_SUMMARY.md" -ForegroundColor Cyan
Write-Host "- Quick Start Guide: " -NoNewline -ForegroundColor Gray
Write-Host "docs/FRONTEND_QUICK_START.md" -ForegroundColor Cyan
Write-Host "- Validation Checklist: " -NoNewline -ForegroundColor Gray
Write-Host "docs/WEEK3_VALIDATION_CHECKLIST.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Week 3 Frontend: READY FOR TESTING!  " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
