$ErrorActionPreference = "Stop"

# Start the app
Write-Host "Starting app..."
$appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru
Start-Sleep -Seconds 10 # Wait for it to start

try {
    $baseUrl = "http://localhost:5154"
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

    # 1. Get Login Page (to get antiforgery token)
    Write-Host "Getting login page..."
    $loginPage = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $session
    $tokenMatch = $loginPage.Content -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"'
    $token = $matches[1]

    # 2. Login
    Write-Host "Logging in..."
    $loginBody = @{
        Email = "test@example.com"
        Password = "Password123!"
        __RequestVerificationToken = $token
    }
    $loginResult = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $loginBody -WebSession $session
    
    # 3. Get Dashboard (to get new antiforgery token for expense)
    Write-Host "Getting dashboard..."
    $dashboardPage = Invoke-WebRequest -Uri "$baseUrl/Dashboard" -WebSession $session
    $tokenMatch = $dashboardPage.Content -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"'
    $expenseToken = $matches[1]

    # 4. Create Expense (Life)
    Write-Host "Creating Life Expense..."
    $expenseBody1 = @{
        Amount = "15.50"
        Category = "Life"
        Description = "Test Life Expense from Script"
        __RequestVerificationToken = $expenseToken
    }
    $expenseResult1 = Invoke-WebRequest -Uri "$baseUrl/Expenses/Create" -Method Post -Body $expenseBody1 -WebSession $session -Headers @{"Accept"="application/json"; "X-Requested-With"="XMLHttpRequest"}
    Write-Host "Life Expense Result: $($expenseResult1.Content)"

    # 5. Create Expense (Whim, no description)
    Write-Host "Creating Whim Expense..."
    $expenseBody2 = @{
        Amount = "9.99"
        Category = "Whim"
        Description = ""
        __RequestVerificationToken = $expenseToken
    }
    $expenseResult2 = Invoke-WebRequest -Uri "$baseUrl/Expenses/Create" -Method Post -Body $expenseBody2 -WebSession $session -Headers @{"Accept"="application/json"; "X-Requested-With"="XMLHttpRequest"}
    Write-Host "Whim Expense Result: $($expenseResult2.Content)"

    # 6. Check Pocket Page
    Write-Host "Getting Pocket page..."
    $pocketPage = Invoke-WebRequest -Uri "$baseUrl/Pocket" -WebSession $session
    if ($pocketPage.Content -match "Test Life Expense from Script") {
        Write-Host "Pocket page contains the expense!"
    } else {
        Write-Host "Pocket page DOES NOT contain the expense!"
    }
}
finally {
    Write-Host "Stopping app..."
    Stop-Process -Id $appProcess.Id -Force
}
