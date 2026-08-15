function Send-Request {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [string]$ContentType = "application/json"
    )
    $params = @{
        Uri = $Url
        Method = $Method
        ErrorAction = "Stop"
    }
    if ($Body) {
        $params["Body"] = $Body
        $params["ContentType"] = $ContentType
    }

    $statusCode = $null
    $content = $null

    try {
        $response = Invoke-WebRequest @params
        $statusCode = $response.StatusCode
        $content = $response.Content
    } catch {
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $content = $reader.ReadToEnd()
        } else {
            $statusCode = "ERROR"
            $content = $_.Exception.Message
        }
    }

    Write-Host "Method: $Method"
    Write-Host "URL: $Url"
    Write-Host "Status: $statusCode"
    Write-Host "Body: $content"
    Write-Host "--------------------"
}

cd "C:\Users\Moises\source\repos\WaterLevelAPI\WaterLevelAPI"
Write-Host "Iniciando o servidor dotnet run na porta 5057..."
$proc = Start-Process dotnet -ArgumentList "run --no-launch-profile --urls http://localhost:5057" -WorkingDirectory "C:\Users\Moises\source\repos\WaterLevelAPI\WaterLevelAPI" -PassThru

# Aguardar ate GET http://localhost:5057/swagger/v1/swagger.json responder
Write-Host "Aguardando o endpoint swagger.json ficar disponivel..."
$maxAttempts = 30
$attempt = 0
$ready = $false
while (-not $ready -and $attempt -lt $maxAttempts) {
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:5057/swagger/v1/swagger.json" -Method GET -ErrorAction Stop
        if ($resp -and $resp.StatusCode -eq 200) {
            $ready = $true
        }
    } catch {
        # espera
    }
    if (-not $ready) {
         Start-Sleep -Seconds 1
         $attempt++
    }
}

if (-not $ready) {
    Write-Error "O servidor nao respondeu na porta 5057 a tempo."
    Stop-Process -Id $proc.Id -Force
    exit 1
}

Write-Host "Servidor pronto! Iniciando as requisicoes requisitadas..."
Write-Host ""

# 1. GET /swagger/v1/swagger.json
Send-Request -Method "GET" -Url "http://localhost:5057/swagger/v1/swagger.json"

# 2. GET /api/WaterLevel sem query
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel"

# 3. GET /api/WaterLevel?deviceId=esp32-test
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel?deviceId=esp32-test"

# 4. POST /api/WaterLevel com Content-Type application/json e body {"deviceId":"esp32-test","currentLevel":10}
Send-Request -Method "POST" -Url "http://localhost:5057/api/WaterLevel" -Body '{"deviceId":"esp32-test","currentLevel":10}'

# 5. GET /api/WaterLevel?deviceId=esp32-test
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel?deviceId=esp32-test"

# 6. GET /api/DeviceCommand sem query
Send-Request -Method "GET" -Url "http://localhost:5057/api/DeviceCommand"

# 7. GET /api/DeviceCommand?deviceId=esp32-test
Send-Request -Method "GET" -Url "http://localhost:5057/api/DeviceCommand?deviceId=esp32-test"

# 8. POST /api/DeviceCommand com body {"deviceId":"esp32-test","command":"open"}
Send-Request -Method "POST" -Url "http://localhost:5057/api/DeviceCommand" -Body '{"deviceId":"esp32-test","command":"open"}'

# 9. POST /api/User/register com body {"name":"Teste API","email":"teste-api-20260815@example.com","password":"Teste123!"}
Send-Request -Method "POST" -Url "http://localhost:5057/api/User/register" -Body '{"name":"Teste API","email":"teste-api-20260815@example.com","password":"Teste123!"}'

# 10. POST /api/User/login com as mesmas credenciais
Send-Request -Method "POST" -Url "http://localhost:5057/api/User/login" -Body '{"email":"teste-api-20260815@example.com","password":"Teste123!"}'

# 11. GET /api/User/profile sem Authorization
Send-Request -Method "GET" -Url "http://localhost:5057/api/User/profile"

# 12. GET /api/User/admin-only sem Authorization
Send-Request -Method "GET" -Url "http://localhost:5057/api/User/admin-only"

Write-Host "Finalizando o processo..."
# Finalizar os processos rodando na porta 5057 de forma limpa
$connection = Get-NetTCPConnection -LocalPort 5057 -ErrorAction SilentlyContinue
if ($connection) {
    foreach ($conn in $connection) {
        $pidToKill = $conn.OwningProcess
        if ($pidToKill) {
            Write-Host "Finalizando PID $pidToKill na porta 5057..."
            Stop-Process -Id $pidToKill -Force -ErrorAction SilentlyContinue
        }
    }
}
Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Write-Host "Processo finalizado com sucesso."
