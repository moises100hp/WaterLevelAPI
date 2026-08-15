# Guia de rotas para o frontend em Blazor/C#

Este documento orienta a IA e o frontend em C# com Blazor sobre as rotas de autenticação e usuários disponíveis na API.

## Stack recomendada

- Blazor WebAssembly ou Blazor Server
- HttpClient injetado via DI
- token JWT salvo em localStorage/sessionStorage
- uso de `AuthenticationStateProvider` ou `ProtectedLocalStorage` para manter sessão

## Base URL

- Local: http://localhost:5000
- HTTPS: https://localhost:5001

## Endpoints de usuário

### 1) Cadastro
- Método: POST
- Rota: /api/User/register
- Body:

```json
{
  "name": "Maria Silva",
  "email": "maria@email.com",
  "password": "123456",
  "role": "User"
}
```

- Valores válidos para role:
  - `User`
  - `Admin`

- Respostas esperadas:
  - 202 Accepted: cadastro realizado com sucesso
  - 400 Bad Request: dados inválidos, e-mail duplicado, senha curta, perfil inválido

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

- Observação:
  - atualmente a API simula o envio por e-mail quando SMTP não está configurado

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
  "email": "maria@email.com",
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

## Regras recomendadas no frontend em Blazor

1. Após login bem-sucedido, guardar o token em localStorage ou sessionStorage.
2. Na inicialização da app, verificar se existe token e tentar carregar o perfil com `/api/User/profile`.
3. Em todas as rotas protegidas, incluir o header `Authorization: Bearer <token>` no `HttpClient`.
4. No logout, remover o token do armazenamento e limpar o estado de autenticação do Blazor.
5. Se o usuário for admin, habilitar telas extras conforme a role retornada no token.
6. Para mudança de senha, exigir senha atual + nova senha válida.
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

## Fluxo sugerido para a interface

1. Cadastro: `/api/User/register`
2. Login: `/api/User/login`
3. Redirecionar conforme `role`
4. Buscar perfil: `/api/User/profile`
5. Trocar senha: `/api/User/change-password`
6. Logout: `/api/User/logout` + limpar token local
