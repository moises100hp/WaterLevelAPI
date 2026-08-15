# Guia de rotas para o frontend em Blazor/C#

Este documento orienta a IA e o frontend em C# com Blazor sobre as rotas de usuários, autenticação, níveis de água e comandos de dispositivos disponíveis na API.

## Stack recomendada

- Blazor WebAssembly ou Blazor Server
- HttpClient injetado via DI
- token JWT salvo em localStorage/sessionStorage
- uso de `AuthenticationStateProvider` ou `ProtectedLocalStorage` para manter sessão

## Base URL

- Local: http://localhost:5042
- HTTPS: https://localhost:7086

As rotas abaixo usam o prefixo `/api`. Swagger fica disponível em `/swagger` no ambiente `Development`.

## Níveis de água

### 1) Registrar leitura
- Método: POST
- Rota: `/api/WaterLevel`
- Autenticação: pública no estado atual da API
- Body:

```json
{
  "deviceId": "esp32-01",
  "minLevel": 10,
  "maxLevel": 90,
  "currentLevel": 45.5
}
```

- Respostas esperadas:
  - 202 Accepted: leitura registrada
  - 400 Bad Request: `currentLevel` negativo, `maxLevel` menor ou igual a `minLevel`, ou dados inválidos
  - 500 Internal Server Error: falha de persistência

### 2) Consultar leitura mais recente
- Método: GET
- Rota: `/api/WaterLevel?deviceId=esp32-01`
- Autenticação: pública no estado atual da API
- Respostas esperadas:
  - 200 OK: leitura mais recente do dispositivo
  - 400 Bad Request: `deviceId` ausente ou vazio
  - 404 Not Found: nenhuma leitura encontrada

Exemplo de resposta:

```json
{
  "deviceId": "esp32-01",
  "minLevel": 10,
  "maxLevel": 90,
  "currentLevel": 45.5
}
```

## Comandos de dispositivos

### 1) Definir comando da bomba
- Método: POST
- Rota: `/api/DeviceCommand`
- Autenticação: pública no estado atual da API
- Body:

```json
{
  "deviceId": "esp32-01",
  "pumpOn": true
}
```

- O endpoint cria ou atualiza o comando do dispositivo e sempre grava `updatedAt` no servidor.
- Respostas esperadas:
  - 202 Accepted: comando gravado
  - 400 Bad Request: `deviceId` ausente ou vazio

### 2) Consultar comando da bomba
- Método: GET
- Rota: `/api/DeviceCommand?deviceId=esp32-01`
- Autenticação: pública no estado atual da API
- Respostas esperadas:
  - 200 OK: comando atual
  - 400 Bad Request: `deviceId` ausente ou vazio

Se ainda não houver comando para o dispositivo, a API retorna `pumpOn: false`.

Exemplo de resposta:

```json
{
  "deviceId": "esp32-01",
  "pumpOn": true,
  "updatedAt": "2026-08-15T12:00:00Z"
}
```

## Endpoints de usuário

### 1) Cadastro
- Método: POST
- Rota: /api/User/register
- Body:

```json
{
  "name": "Maria Silva",
  "email": "maria@email.com",
  "password": "123456"
}
```

- Novos cadastros recebem sempre o perfil `User`. O cliente não pode escolher `Admin` no payload.

- Respostas esperadas:
  - 202 Accepted: cadastro realizado com sucesso
  - 400 Bad Request: dados inválidos, e-mail duplicado ou senha curta

### 2) Login
- Método: POST
- Rota: /api/User/login
- Body:

```json
{
  "email": "maria@email.com",
  "password": "123456"
}
```

- Resposta esperada: 200 OK
- Retorno exemplo:

```json
{
  "id": 1,
  "name": "Maria Silva",
  "email": "maria@email.com",
  "role": "Admin",
  "createdAt": "2026-08-15T00:00:00Z",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

- Importante:
  - salvar o token recebido em localStorage ou sessionStorage
  - enviar o token em todas as rotas protegidas no header `Authorization: Bearer <token>`

### 3) Esqueci minha senha
- Método: POST
- Rota: /api/User/forgot-password
- Body:

```json
{
  "email": "maria@email.com"
}
```

- Resposta esperada:
  - 202 Accepted: senha temporária enviada para o e-mail informado
  - 400 Bad Request: e-mail inválido ou usuário não encontrado
  - 500 Internal Server Error: SMTP não configurado ou falha no envio

- Observação:
  - o backend exige `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER` e `SMTP_PASSWORD`; a senha temporária não é impressa em logs

### 4) Trocar senha autenticada
- Método: POST
- Rota: /api/User/change-password
- Header obrigatório:

```http
Authorization: Bearer <token>
```

- Body:

```json
{
  "currentPassword": "senhaAtual123",
  "newPassword": "novaSenha456"
}
```

- Respostas esperadas:
  - 200 OK: senha alterada com sucesso
  - 400 Bad Request: senha atual incorreta ou nova senha inválida
  - 401 Unauthorized: token ausente ou inválido

### 5) Perfil do usuário autenticado
- Método: GET
- Rota: /api/User/profile
- Header obrigatório:

```http
Authorization: Bearer <token>
```

- Resposta esperada: 200 OK

```json
{
  "id": 1,
  "name": "Maria Silva",
  "email": "maria@email.com",
  "role": "Admin",
  "createdAt": "2026-08-15T00:00:00Z"
}
```

### 6) Logout
- Método: POST
- Rota: /api/User/logout
- Header obrigatório:

```http
Authorization: Bearer <token>
```

- Resposta esperada: 200 OK
- Importante:
  - como o JWT é stateless, o logout real deve ser feito no front removendo o token do armazenamento local, e o backend apenas confirma a operação

### 7) Rota administrativa
- Método: GET
- Rota: /api/User/admin-only
- Header obrigatório:

```http
Authorization: Bearer <token>
```

- Requer perfil: `Admin`
- Resposta esperada: 200 OK com mensagem de sucesso
- Se o usuário não for admin, a API retorna 403 Forbidden

### 8) Listar usuários
- Método: GET
- Rota: /api/User/users
- Header obrigatório:

```http
Authorization: Bearer <token>
```

- Requer perfil: `Admin`
- Resposta esperada: 200 OK

```json
[
  {
    "id": 1,
    "name": "Maria Silva",
    "email": "maria@email.com",
    "role": "Admin",
    "status": "Ativo"
  }
]
```

- Se o usuário não for admin, a API retorna 403 Forbidden

## Campos e autorização

- `deviceId` identifica o dispositivo e deve ser enviado nas rotas de nível e comando.
- As rotas de telemetria e comando não exigem JWT atualmente para permitir comunicação direta com ESP32. O frontend não deve interpretar isso como autorização por usuário; qualquer cliente que alcance a API pode chamá-las.
- As rotas `/api/User/change-password`, `/api/User/profile`, `/api/User/logout`, `/api/User/admin-only` e `/api/User/users` exigem `Authorization: Bearer <token>`.
- O JWT expira após oito horas e o logout não revoga tokens já emitidos; o frontend deve remover o token localmente.

## Regras recomendadas no frontend em Blazor

1. Após login bem-sucedido, guardar o token em localStorage ou sessionStorage.
2. Na inicialização da app, verificar se existe token e tentar carregar o perfil com `/api/User/profile`.
3. Em todas as rotas protegidas, incluir o header `Authorization: Bearer <token>` no `HttpClient`.
4. No logout, remover o token do armazenamento e limpar o estado de autenticação do Blazor.
5. Se o usuário for admin, habilitar telas extras conforme a role retornada no token.
6. Para mudança de senha, enviar somente senha atual + nova senha válida; a conta é obtida do token autenticado.
7. Em Blazor WebAssembly, usar `AuthenticationStateProvider` + `IAccessTokenProvider` ou um serviço próprio para centralizar a autenticação.
8. Em Blazor Server, normalmente o token é gerado na API e o front guarda no navegador; a sessão continua sendo stateless pela API.

## Exemplo em C# para HttpClient do Blazor

```csharp
public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/User/login", request);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
```

## Exemplo de DTO em C#

```csharp
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Token { get; set; } = string.Empty;
}
```

## Como tratar o token no Blazor

- Salvar:

```csharp
await ProtectedLocalStorage.SetAsync("authToken", response.Token);
```

- Recuperar:

```csharp
var tokenResult = await ProtectedLocalStorage.GetAsync<string>("authToken");
```

- Remover no logout:

```csharp
await ProtectedLocalStorage.DeleteAsync("authToken");
```

## Regras de autorização por perfil no Blazor

- Usuário comum: `User`
- Administrador: `Admin`

Exemplo de rota protegida em componente:

```csharp
@attribute [Authorize(Roles = "Admin")]
```

Ou em código:

```csharp
if (!user.IsInRole("Admin"))
{
    NavigationManager.NavigateTo("/unauthorized");
}
```

## Observações de segurança

- o JWT inclui `role` em `ClaimTypes.Role`
- o backend não armazena sessão no servidor para JWT
- a senha não deve nunca ser devolvida pela API
- a senha temporária gerada para recuperação deve ser trocada no primeiro login
- não inclua `role` no cadastro esperando criar um administrador; esse campo é ignorado e o usuário é criado como `User`
- não inclua `email` na troca de senha; a identidade vem do claim `NameIdentifier` do JWT

## Fluxo sugerido para a interface

1. Cadastro: `/api/User/register`
2. Login: `/api/User/login`
3. Redirecionar conforme `role`
4. Buscar perfil: `/api/User/profile`
5. Registrar e consultar telemetria em `/api/WaterLevel`
6. Ler e alterar comando em `/api/DeviceCommand`
7. Trocar senha: `/api/User/change-password`
8. Logout: `/api/User/logout` + limpar token local
