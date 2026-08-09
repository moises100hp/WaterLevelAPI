# Especificação de Rotas da API para Arduino ESP32

## Base URL
- Local de desenvolvimento (conforme `launchSettings.json`):
  - `http://localhost:5042/api/waterlevel`

> Altere `localhost:5042` para o endereço/IP do servidor quando publicado ou usado em rede local.

---

## 1. Registrar nível de água
- Método: `POST`
- Endpoint: `/api/waterlevel`
- Content-Type: `application/json`

### Corpo (JSON)
```json
{
  "deviceId": "DEVICE_123",
  "minLevel": 10.0,
  "maxLevel": 90.0,
  "currentLevel": 45.5
}
```

### Uso esperado
- Grava ou atualiza o nível do dispositivo.
- Retorno: `202 Accepted` em caso de sucesso.
- Erros:
  - `400 Bad Request` se os dados estiverem inválidos.
  - `500 Internal Server Error` para falha de servidor.

---

## 2. Obter nível atual
- Método: `GET`
- Endpoint: `/api/waterlevel/current`
- Query string: `deviceId`

### Exemplo de URL
```
http://localhost:5042/api/waterlevel/current?deviceId=DEVICE_123
```

### Retorno esperado
- `202 Accepted` com JSON do dispositivo:
```json
{
  "deviceId": "DEVICE_123",
  "minLevel": 10.0,
  "maxLevel": 90.0,
  "currentLevel": 45.5
}
```

### Uso esperado
- Retorna os valores de nível para o `deviceId` solicitado.
- Se o dispositivo não existir, pode retornar um objeto vazio ou erro dependendo da lógica do servidor.

---

## 3. Obter status do dispositivo
- Método: `GET`
- Endpoint: `/api/waterlevel/status-device`
- Query string: `deviceId`

### Exemplo de URL
```
http://localhost:5042/api/waterlevel/status-device?deviceId=DEVICE_123
```

### Retorno esperado
- `202 Accepted` com JSON:
```json
{
  "deviceId": "DEVICE_123",
  "ligarDispositivo": true
}
```

### Uso esperado
- Retorna se há uma ação pendente para ligar o dispositivo.
- Usado para sincronizar o ESP32 com o estado desejado do servidor.

---

## 4. Alterar status do dispositivo
- Método: `POST`
- Endpoint: `/api/waterlevel/status-device`
- Content-Type: `application/json`

### Corpo (JSON)
```json
{
  "deviceId": "DEVICE_123",
  "ligarDispositivo": true
}
```

### Uso esperado
- Define se o dispositivo deve ser ligado ou desligado.
- Retorno: `202 Accepted` em caso de sucesso.
- Erros:
  - `400 Bad Request` se os dados estiverem inválidos.
  - `500 Internal Server Error` para falha de servidor.

---

## Observações para Arduino ESP32
- Use `HTTPClient` para requisições HTTP.
- Defina `Content-Type: application/json` no cabeçalho para `POST`.
- Para `GET`, envie o `deviceId` como query string.
- Exemplo de fluxo comum:
  1. ESP32 lê sensor de nível.
  2. Envia `POST /api/waterlevel` com `currentLevel`.
  3. Lê `GET /api/waterlevel/status-device?deviceId=...` para saber se deve ligar/desligar o dispositivo.
  4. Atualiza o estado local conforme a resposta.

---

## Dicas de JSON e políticas
- Sempre use string no `deviceId`.
- `minLevel`, `maxLevel` e `currentLevel` são números de ponto flutuante.
- `ligarDispositivo` é booleano (`true` / `false`).
