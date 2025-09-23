# Nuuvify.CommonPack.Security.Abstraction

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Build Status - Main](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=main)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=qas)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=qas)

Biblioteca de abstrações para segurança e autenticação JWT. Esta biblioteca contém interfaces, extensões e classes auxiliares que definem os contratos para implementações de segurança em aplicações .NET.

## 📋 Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Dependências](#dependências)
- [Configuração](#configuração)
- [Uso](#uso)
  - [IUserAuthenticated](#iuserauthenticated)
  - [IJwtBuilder](#ijwtbuilder)
  - [IUserAccountRepository](#iuseraccountrepository)
  - [ClaimsPrincipalExtensions](#claimsprincipalextensions)
  - [CredentialToken](#credentialtoken)
- [Exemplos Práticos](#exemplos-práticos)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

## Funcionalidades

- ✅ **Abstrações para autenticação JWT** com interfaces bem definidas
- ✅ **Gerenciamento de usuários autenticados** com verificação de roles e claims
- ✅ **Construção de tokens JWT** com builder pattern flexível
- ✅ **Extensões para ClaimsPrincipal** com métodos utilitários
- ✅ **Modelos de dados padronizados** para usuários e roles
- ✅ **Validação de tokens** com verificação de expiração
- ✅ **Suporte a refresh tokens** para renovação automática
- ✅ **Repository pattern** para gerenciamento de contas de usuário
- ✅ **Compatibilidade com .NET Standard 2.1** para máxima portabilidade

## Instalação

### Via Package Manager Console
```powershell
Install-Package Nuuvify.CommonPack.Security.Abstraction
```

### Via .NET CLI
```bash
dotnet add package Nuuvify.CommonPack.Security.Abstraction
```

### Via PackageReference
```xml
<PackageReference Include="Nuuvify.CommonPack.Security.Abstraction" Version="X.X.X" />
```

## Dependências

### NuGet Packages

| Package                               | Version | Descrição                                              |
| ------------------------------------- | ------- | ------------------------------------------------------ |
| **System.ComponentModel.Annotations** | 5.0.0   | Fornece atributos de validação e anotações para models |

### Project References

| Project                           | Descrição                                                                                                 |
| --------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.Extensions** | Biblioteca com padrão de notificação, extensões para collections, strings e outros utilitários essenciais |

### Framework

- **.NET Standard 2.1**: Garante compatibilidade com .NET Core 3.0+, .NET 5+, .NET 6+, .NET 8+ e .NET Framework 4.7.2+

## Configuração

Esta biblioteca contém apenas abstrações, portanto não requer configuração específica. As implementações concretas devem ser registradas via Dependency Injection:

```csharp
// Program.cs ou Startup.cs
builder.Services.AddScoped<IUserAuthenticated, UserAuthenticated>();
builder.Services.AddScoped<IJwtBuilder, JwtBuilder>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
```

## Uso

### IUserAuthenticated

Interface para gerenciamento de usuários autenticados com verificação de autenticação, autorização e acesso a claims:

```csharp
public class MyController : ControllerBase
{
    private readonly IUserAuthenticated _userAuth;

    public MyController(IUserAuthenticated userAuth)
    {
        _userAuth = userAuth;
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        if (!_userAuth.IsAuthenticated())
        {
            return Unauthorized();
        }

        var username = _userAuth.Username();
        var email = _userAuth.GetClaimValue(ClaimTypes.Email);

        return Ok(new { username, email });
    }

    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        if (!_userAuth.IsAuthorized("Admin", "SuperUser"))
        {
            return Forbid();
        }

        return Ok("Admin content");
    }
}
```

### IJwtBuilder

Interface para construção de tokens JWT com padrão builder fluente:

```csharp
public class TokenService
{
    private readonly IJwtBuilder _jwtBuilder;

    public TokenService(IJwtBuilder jwtBuilder)
    {
        _jwtBuilder = jwtBuilder;
    }

    public async Task<CredentialToken> GenerateTokenAsync(PersonWithRolesQueryResult user)
    {
        var token = _jwtBuilder
            .WithJwtClaims()
            .WithJwtUserClaims(user)
            .GetUserToken();

        return token;
    }

    public bool ValidateToken(string token)
    {
        return _jwtBuilder.CheckTokenIsValid(token);
    }
}
```

### IUserAccountRepository

Interface para acesso a dados de contas de usuário com operações assíncronas:

```csharp
public class UserService
{
    private readonly IUserAccountRepository _userRepo;

    public UserService(IUserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<bool> HasPermissionAsync(string login, string permission)
    {
        return await _userRepo.PersonIsMemberOf(login, permission);
    }

    public async Task<IEnumerable<PersonRoleQueryResult>> GetUserRolesAsync(string login)
    {
        return await _userRepo.GetUserRoles(login);
    }
}
```

### ClaimsPrincipalExtensions

Métodos de extensão para facilitar extração de informações do ClaimsPrincipal:

```csharp
public class ProfileService
{
    public UserProfileDto GetCurrentUserProfile(ClaimsPrincipal principal)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal));

        return new UserProfileDto
        {
            Login = principal.GetLogin(),
            Name = principal.GetName(),
            Email = principal.GetEmail()
        };
    }
}

// Uso em Controllers
[HttpGet("me")]
public IActionResult GetCurrentUser()
{
    var profile = new UserProfileDto
    {
        Login = User.GetLogin(),
        Name = User.GetName(),
        Email = User.GetEmail()
    };

    return Ok(profile);
}
```

### CredentialToken

Classe para gerenciamento de tokens com validação automática de expiração:

```csharp
public class AuthService
{
    public CredentialToken CreateToken(string loginId, string password)
    {
        var credential = new CredentialToken
        {
            LoginId = loginId,
            Password = password,
            ExpiresIn = 3600 // 1 hora em segundos
        };

        // Token e Expires são calculados automaticamente
        return credential;
    }

    public bool IsTokenValid(CredentialToken credential)
    {
        // Verifica se expira em 5 minutos (padrão)
        return credential.IsValidToken();

        // Ou verifica com margem customizada
        return credential.IsValidToken(-10); // Expira em 10 minutos
    }
}
```

## Exemplos Práticos

### Exemplo 1: Middleware de Autenticação

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUserAuthenticated _userAuth;

    public AuthenticationMiddleware(RequestDelegate next, IUserAuthenticated userAuth)
    {
        _next = next;
        _userAuth = userAuth;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/secure"))
        {
            if (!_userAuth.IsAuthenticated(out string token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token required");
                return;
            }

            // Token disponível para uso posterior
            context.Items["AuthToken"] = token;
        }

        await _next(context);
    }
}
```

### Exemplo 2: Autorização Baseada em Roles

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserAuthenticated _userAuth;

    public AdminController(IUserAuthenticated userAuth)
    {
        _userAuth = userAuth;
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        if (!_userAuth.IsInRole("Admin"))
        {
            return Forbid("Acesso negado. Role 'Admin' necessária.");
        }

        // Lógica para buscar usuários
        return Ok();
    }

    [HttpDelete("users/{id}")]
    public IActionResult DeleteUser(int id)
    {
        if (!_userAuth.IsAuthorized("SuperAdmin", "UserManager"))
        {
            return Forbid("Acesso negado. Permissões insuficientes.");
        }

        // Lógica para deletar usuário
        return NoContent();
    }
}
```

### Exemplo 3: Service para Geração de Tokens

```csharp
public class TokenGenerationService
{
    private readonly IJwtBuilder _jwtBuilder;
    private readonly IUserAccountRepository _userRepo;

    public TokenGenerationService(IJwtBuilder jwtBuilder, IUserAccountRepository userRepo)
    {
        _jwtBuilder = jwtBuilder;
        _userRepo = userRepo;
    }

    public async Task<CredentialToken> AuthenticateAsync(string login, string password)
    {
        // Validar credenciais (implementação específica)
        if (!await ValidateCredentialsAsync(login, password))
        {
            return null;
        }

        // Obter roles do usuário
        var roles = await _userRepo.GetUserRoles(login);

        var user = new PersonWithRolesQueryResult
        {
            Login = login,
            Email = $"{login}@company.com",
            Name = "User Name",
            Groups = roles
        };

        // Gerar token
        return _jwtBuilder
            .WithJwtClaims()
            .WithJwtUserClaims(user)
            .GetUserToken();
    }

    private async Task<bool> ValidateCredentialsAsync(string login, string password)
    {
        // Implementar validação de credenciais
        await Task.Delay(100); // Simular async operation
        return !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password);
    }
}
```

## API Reference

### Interfaces

#### IUserAuthenticated
- `string Username()`: Obtém o login do usuário autenticado
- `bool IsAuthenticated()`: Verifica se existe um usuário autenticado
- `bool IsAuthenticated(out string token)`: Verifica autenticação e obtém o token
- `bool IsAuthorized(params string[] groups)`: Verifica se o usuário pertence aos grupos informados
- `string GetClaimValue(string claimName)`: Obtém o valor de uma claim específica
- `IEnumerable<Claim> GetClaims()`: Retorna todas as claims do usuário
- `bool IsInRole(string role)`: Verifica se o usuário está no role especificado

#### IJwtBuilder
- `long ToUnixEpochDate(DateTime dateTime)`: Converte DateTime para Unix timestamp
- `IJwtBuilder WithJwtClaims()`: Adiciona claims padrão do JWT
- `IJwtBuilder WithJwtUserClaims(PersonWithRolesQueryResult personGroups)`: Adiciona claims específicas do usuário
- `ClaimsIdentity GetClaimsIdentity()`: Obtém a identidade com claims configuradas
- `string BuildToken()`: Constrói o token JWT como string
- `CredentialToken GetUserToken()`: Obtém o token como objeto CredentialToken
- `bool CheckTokenIsValid(string token)`: Valida se um token é válido

#### IUserAccountRepository
- `Task<IEnumerable<PersonRoleQueryResult>> GetUserRoles(string login)`: Obtém roles do usuário
- `Task<bool> PersonIsMemberOf(string login, string claimType)`: Verifica se usuário pertence a um grupo

### Extensions

#### ClaimsPrincipalExtensions
- `string GetLogin(this ClaimsPrincipal principal)`: Obtém o login do ClaimsPrincipal
- `string GetName(this ClaimsPrincipal principal)`: Obtém o nome do ClaimsPrincipal
- `string GetEmail(this ClaimsPrincipal principal)`: Obtém o email do ClaimsPrincipal

### Models

#### CredentialToken
**Propriedades:**
- `string LoginId`: Login da aplicação ou usuário
- `string Password`: Senha do usuário
- `string Token`: JWT gerado
- `string RefreshToken`: Token para renovação
- `DateTimeOffset Expires`: Data de expiração do token
- `DateTimeOffset Created`: Data de criação do token
- `long ExpiresIn`: Tempo de vida em segundos (calcula Expires automaticamente)
- `IDictionary<string, string> Warnings`: Notificações/avisos

**Métodos:**
- `bool IsValidToken(int minutes = -5)`: Verifica se o token é válido considerando margem de minutos

#### PersonQueryResult
- `string Email`: Email do usuário
- `string Login`: Login do usuário
- `string Name`: Nome da pessoa

#### PersonWithRolesQueryResult : PersonQueryResult
- `IEnumerable<PersonRoleQueryResult> Groups`: Grupos/roles do usuário

## Troubleshooting

### Problemas Comuns

#### ArgumentNullException em ClaimsPrincipalExtensions
**Problema**: `ArgumentNullException` ao usar extensões do ClaimsPrincipal
```csharp
// ❌ Incorreto
var login = principal.GetLogin(); // principal pode ser null
```

**Solução**: Sempre verificar se o principal não é null
```csharp
// ✅ Correto
if (principal != null)
{
    var login = principal.GetLogin();
}
```

#### Token Inválido Mesmo Dentro do Prazo
**Problema**: `IsValidToken()` retorna false mesmo com token válido

**Causa**: Margem de segurança padrão de 5 minutos

**Solução**: Ajustar a margem de validação
```csharp
// ✅ Sem margem de segurança
bool isValid = credential.IsValidToken(0);

// ✅ Margem personalizada (10 minutos)
bool isValid = credential.IsValidToken(-10);
```

#### Configuração de Dependency Injection
**Problema**: Interfaces não resolvidas pelo DI container

**Solução**: Registrar as implementações concretas
```csharp
// ✅ Registrar implementações
services.AddScoped<IUserAuthenticated, UserAuthenticated>();
services.AddScoped<IJwtBuilder, JwtBuilder>();
services.AddScoped<IUserAccountRepository, UserAccountRepository>();
```

### Logs e Debugging

Para debugging, implemente logging nas implementações concretas:

```csharp
public class UserAuthenticated : IUserAuthenticated
{
    private readonly ILogger<UserAuthenticated> _logger;

    public UserAuthenticated(ILogger<UserAuthenticated> logger)
    {
        _logger = logger;
    }

    public bool IsAuthenticated()
    {
        _logger.LogDebug("Verificando autenticação do usuário");
        // Implementation...
    }
}
```

## Changelog

Ver arquivo [CHANGELOG.md](CHANGELOG.md) para histórico detalhado de alterações.

---

## 📞 Suporte

Para dúvidas, issues ou contribuições:
- 🐛 **Issues**: [GitHub Issues](https://github.com/nuuvify/CommonPack/issues)
- 📧 **Email**: [suporte@zocate.li](mailto:suporte@zocate.li)
- 📖 **Documentação**: [Wiki do Projeto](https://github.com/nuuvify/CommonPack/wiki)

---
**Nuuvify CommonPack** - Construindo soluções robustas para .NET 🚀
