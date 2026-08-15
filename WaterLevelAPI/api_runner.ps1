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

    Write-Host "METHOD: $Method"
    Write-Host "URL: $Url"
    Write-Host "STATUS: $statusCode"
    Write-Host "BODY: $content"
    Write-Host "----------------------------------------"
}

Write-Host "Iniciando o servidor na porta 5057..."
$proc = Start-Process dotnet -ArgumentList "run --no-launch-profile --urls http://localhost:5057" -WorkingDirectory "C:\Users\Moises\source\repos\WaterLevelAPI\WaterLevelAPI" -PassThru

# Aguarda ate estar pronto
$ready = $false
for ($i=0; $i -lt 30; $i++) {
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:5057/swagger/v1/swagger.json" -Method GET -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            $ready = $true
            break
        }
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $ready) {
    Write-Host "Falha ao iniciar o servidor"
    Stop-Process -Id $proc.Id -Force
    exit
}

Write-Host "Servidor ativo! Fazendo as chamadas..."

Send-Request -Method "GET" -Url "http://localhost:5057/swagger/v1/swagger.json"
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel"
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel?deviceId=esp32-test"
Send-Request -Method "POST" -Url "http://localhost:5057/api/WaterLevel" -Body '{"deviceId":"esp32-test","currentLevel":10}'
Send-Request -Method "GET" -Url "http://localhost:5057/api/WaterLevel?deviceId=esp32-test"
Send-Request -Method "GET" -Url "http://localhost:5057/api/DeviceCommand"
Send-Request -Method "GET" -Url "http://localhost:5057/api/DeviceCommand?deviceId=esp32-test"
Send-Request -Method "POST" -Url "http://localhost:5057/api/DeviceCommand" -Body '{"deviceId":"esp32-test","command":"open"}'
Send-Request -Method "POST" -Url "http://localhost:5057/api/User/register" -Body '{"name":"Teste API","email":"teste-api-20260815@example.com","password":"Teste123!"}'
Send-Request -Method "POST" -Url "http://localhost:5057/api/User/login" -Body '{"email":"teste-api-20260815@example.com","password":"Teste123!"}'
Send-Request -Method "GET" -Url "http://localhost:5057/api/User/profile"
Send-Request -Method "GET" -Url "http://localhost:5057/api/User/admin-only"

Write-Host "Parando o processo..."
Stop-Process -Id $proc.Id -Force
