$ErrorActionPreference = 'Stop'

$baseUrl = 'http://localhost:5042'

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers,
        [object]$Body,
        [string]$Label
    )

    Write-Host "`n=== $Label ==="
    $bodyJson = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 10 } else { $null }
    try {
        $response = if ($null -ne $Body) {
            Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers -ContentType 'application/json' -Body $bodyJson
        }
        else {
            Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers
        }

        $response | ConvertTo-Json -Depth 10
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $message = $_.ErrorDetails.Message
        Write-Host "HTTP $statusCode"
        if ($message) { Write-Host $message }
    }
}

$email = "route.autotest.$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))@test.com"

Write-Host "API base: $baseUrl"
Write-Host "Email de teste: $email"

$registerBody = @{
    name = 'Route Auto Test'
    email = $email
    password = '123456'
    role = 'Admin'
}

Invoke-Api -Method Post -Uri "$baseUrl/api/User/register" -Body $registerBody -Label 'Register'

$loginResponse = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/User/login" -ContentType 'application/json' -Body (@{
    email = $email
    password = '123456'
} | ConvertTo-Json -Depth 10)

$token = $loginResponse.token
$headers = @{ Authorization = "Bearer $token" }

Invoke-Api -Method Get -Uri "$baseUrl/api/User/profile" -Headers $headers -Label 'Profile'
Invoke-Api -Method Get -Uri "$baseUrl/api/User/admin-only" -Headers $headers -Label 'Admin only'

Invoke-Api -Method Post -Uri "$baseUrl/api/User/change-password" -Headers $headers -Body @{
    email = $email
    currentPassword = '123456'
    newPassword = '654321'
} -Label 'Change password'

$loginWithNewPassword = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/User/login" -ContentType 'application/json' -Body (@{
    email = $email
    password = '654321'
} | ConvertTo-Json -Depth 10)

$headers2 = @{ Authorization = "Bearer $($loginWithNewPassword.token)" }
Invoke-Api -Method Get -Uri "$baseUrl/api/User/admin-only" -Headers $headers2 -Label 'Admin only with new token'

$resetEmail = "route.reset.$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))@test.com"
$resetBody = @{ name = 'Reset User'; email = $resetEmail; password = 'reset123'; role = 'User' }
Invoke-Api -Method Post -Uri "$baseUrl/api/User/register" -Body $resetBody -Label 'Register reset user'
Invoke-Api -Method Post -Uri "$baseUrl/api/User/forgot-password" -Body @{ email = $resetEmail } -Label 'Forgot password'

Invoke-Api -Method Post -Uri "$baseUrl/api/WaterLevel" -Body @{
    deviceId = 'route-auto-001'
    minLevel = 10
    maxLevel = 90
    currentLevel = 45
} -Label 'Register water level'

Invoke-Api -Method Get -Uri "$baseUrl/api/WaterLevel?deviceId=route-auto-001" -Label 'Get current water level'

Invoke-Api -Method Post -Uri "$baseUrl/api/DeviceCommand" -Body @{
    deviceId = 'route-auto-001'
    pumpOn = $true
    updatedAt = (Get-Date).ToString('o')
} -Label 'Set device command'

Invoke-Api -Method Get -Uri "$baseUrl/api/DeviceCommand?deviceId=route-auto-001" -Label 'Get device command'

Invoke-Api -Method Post -Uri "$baseUrl/api/User/logout" -Headers $headers2 -Label 'Logout'

try {
    Invoke-RestMethod -Method Get -Uri "$baseUrl/api/User/profile" -Headers @{ Authorization = 'Bearer invalid' }
}
catch {
    Write-Host "`n=== Invalid token check ==="
    Write-Host "HTTP $($_.Exception.Response.StatusCode.value__)"
    if ($_.ErrorDetails.Message) { Write-Host $_.ErrorDetails.Message }
}

Write-Host "`nValidação concluída."
