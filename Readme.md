# Nuuvify.CommonPack

[![Build Status - Main](https://dev.azure.com/nuuvers/Nuuvify/_apis/build/status/CI-Github-Nuuvify.CommonPack?repoName=lzocateli00%2FNuuvify.CommonPack&branchName=main)](https://dev.azure.com/nuuvers/Nuuvify/_build/latest?definitionId=23&repoName=lzocateli00%2FNuuvify.CommonPack&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvers/Nuuvify/_apis/build/status/CI-Github-Nuuvify.CommonPack?repoName=lzocateli00%2FNuuvify.CommonPack&branchName=qas)](https://dev.azure.com/nuuvers/Nuuvify/_build/latest?definitionId=23&repoName=lzocateli00%2FNuuvify.CommonPack&branchName=qas)

Coleção de bibliotecas .NET para desenvolvimento de aplicações robustas, escaláveis e de alta performance.

> 🎉 **Novidade!** Agora com suporte completo a **filtros dinâmicos com tipos complexos**!
> Use propriedades de navegação como `Customer.Address.City` diretamente nos seus filtros.
> [Ver exemplos ↓](#exemplo-avançado---filtros-dinâmicos-com-tipos-complexos)

## ✨ Características Principais

### 🚀 Performance e Escalabilidade
- **Async/Await** em todas as operações I/O
- **Connection pooling** otimizado para bancos de dados
- **Retry policies** configuráveis com Polly
- **Caching** inteligente com suporte a distribuído

### 🔒 Segurança
- **JWT Authentication** com refresh tokens
- **Autenticação Azure AD** integrada
- **Criptografia** de dados sensíveis
- **Rate limiting** e proteção contra ataques

### 📊 Observabilidade
- **Logging estruturado** com Microsoft.Extensions.Logging
- **Health checks** customizados
- **Métricas** e telemetria
- **Distributed tracing** pronto para produção

### �️ Arquitetura
- **Clean Architecture** e DDD patterns
- **SOLID principles** aplicados
- **Dependency Injection** nativa
- **Unit of Work** e Repository patterns

### 🔍 Filtros Dinâmicos Avançados
- **Tipos complexos** com navegação aninhada (`Customer.Address.City`)
- **Type-safe filters** com attributes declarativos
- **Performance otimizada** com compiled expressions
- **Operadores especializados** (Contains, StartsWith, Range, etc.)
- **Validação automática** de propriedades de navegação

### 🧪 Testabilidade
- **100% testável** com mocks e fakes
- **InMemory providers** para testes rápidos
- **Testcontainers** para testes de integração
- **Separation of Concerns** clara

### 🌐 Cloud-Native
- **Azure Service Bus** para mensageria
- **Azure Storage** para blobs e arquivos
- **Docker** e Kubernetes ready
- **Health checks** para orquestração

## �🆕 Novidades

### BackgroundService v2.0 - Diagnóstico Avançado
- **✨ Propriedades de diagnóstico contextuais** para Dead Letter Queue e Abandon
- **🔧 Arquitetura modular refatorada** com complexidade cognitiva reduzida
- **🔍 Troubleshooting aprimorado** com metadados detalhados
- **📊 Observabilidade avançada** para monitoramento e métricas

### UnitOfWork - Dynamic Queries v3.0
- **🔍 Filtros com tipos complexos** - Suporte a propriedades de navegação aninhadas
- **🎯 Notação de ponto (dot notation)** - Ex: `Customer.Address.City`
- **⚡ Performance otimizada** com compiled queries e parameterização EF Core
- **🛠️ Validação aprimorada** - Detecção automática de propriedades aninhadas
- **📝 Documentação completa** com exemplos de filtros complexos

### Email - SMTP Service
- **📧 Envio via MailKit** com templates HTML
- **📎 Anexos** (file e stream)
- **👥 Múltiplos destinatários** (To, Cc, Bcc)
- **🔄 Retry logic** integrado

## 📦 Pacotes Disponíveis

### Infraestrutura e Serviços

| Pacote                                             | Descrição                                      | Versão                                                                                                                                                                        | Downloads                                                                                                                                                                          |
| -------------------------------------------------- | ---------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.BackgroundService**           | 🆕 Serviços de background com Azure Service Bus | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)                     | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)                     |
| **Nuuvify.CommonPack.AzureServiceBus**             | Cliente para Azure Service Bus                 | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureServiceBus.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus/)                         | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.AzureServiceBus.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus/)                         |
| **Nuuvify.CommonPack.AzureServiceBus.Abstraction** | Abstrações para Azure Service Bus              | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureServiceBus.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus.Abstraction/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.AzureServiceBus.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus.Abstraction/) |
| **Nuuvify.CommonPack.AzureStorage**                | Cliente para Azure Storage                     | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureStorage.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureStorage/)                               | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.AzureStorage.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureStorage/)                               |
| **Nuuvify.CommonPack.AzureStorage.Abstraction**    | Abstrações para Azure Storage                  | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureStorage.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureStorage.Abstraction/)       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.AzureStorage.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureStorage.Abstraction/)       |

### Comunicação e Integração

| Pacote                                    | Descrição                                      | Versão                                                                                                                                                      | Downloads                                                                                                                                                        |
| ----------------------------------------- | ---------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.StandardHttpClient** | Cliente HTTP otimizado com retry e resiliência | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.StandardHttpClient.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.StandardHttpClient/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.StandardHttpClient.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.StandardHttpClient/) |
| **Nuuvify.CommonPack.Email**              | Envio de e-mails via SMTP com MailKit          | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Email.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Email/)                           | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Email.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Email/)                           |
| **Nuuvify.CommonPack.Email.Abstraction**  | Abstrações para serviço de e-mail              | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Email.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Email.Abstraction/)   | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Email.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Email.Abstraction/)   |

### Segurança

| Pacote                                         | Descrição                                 | Versão                                                                                                                                                                | Downloads                                                                                                                                                                  |
| ---------------------------------------------- | ----------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.Security**                | Componentes de segurança e autenticação   | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Security.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security/)                               | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Security.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security/)                               |
| **Nuuvify.CommonPack.Security.Abstraction**    | Abstrações de segurança                   | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Security.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.Abstraction/)       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Security.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.Abstraction/)       |
| **Nuuvify.CommonPack.Security.JwtCredentials** | Gerenciamento de credenciais JWT          | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Security.JwtCredentials.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.JwtCredentials/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Security.JwtCredentials.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.JwtCredentials/) |
| **Nuuvify.CommonPack.Security.JwtStore.Ef**    | Armazenamento de JWT com Entity Framework | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Security.JwtStore.Ef.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.JwtStore.Ef/)       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Security.JwtStore.Ef.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security.JwtStore.Ef/)       |

### Persistência de Dados

| Pacote                                        | Descrição                              | Versão                                                                                                                                                              | Downloads                                                                                                                                                                |
| --------------------------------------------- | -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Nuuvify.CommonPack.UnitOfWork**             | Implementação do padrão Unit of Work   | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.UnitOfWork.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/)                         | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.UnitOfWork.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/)                         |
| **Nuuvify.CommonPack.UnitOfWork.Abstraction** | Abstrações do padrão Unit of Work      | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.UnitOfWork.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork.Abstraction/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.UnitOfWork.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork.Abstraction/) |
| **Nuuvify.CommonPack.Domain**                 | Classes base para entidades de domínio | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Domain.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Domain/)                                 | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Domain.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Domain/)                                 |
| **Nuuvify.CommonPack.AutoHistory**            | Auditoria automática de entidades      | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AutoHistory.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AutoHistory/)                       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.AutoHistory.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AutoHistory/)                       |

### Entity Framework Exceptions

| Pacote                                      | Descrição                            | Versão                                                                                                                                                          | Downloads                                                                                                                                                            |
| ------------------------------------------- | ------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.EF.Exceptions.Common** | Tratamento de exceções EF Core comum | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.EF.Exceptions.Common.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Common/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.EF.Exceptions.Common.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Common/) |
| **Nuuvify.CommonPack.EF.Exceptions.Db2**    | Tratamento de exceções para IBM DB2  | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.EF.Exceptions.Db2.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Db2/)       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.EF.Exceptions.Db2.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Db2/)       |
| **Nuuvify.CommonPack.EF.Exceptions.Oracle** | Tratamento de exceções para Oracle   | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.EF.Exceptions.Oracle.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Oracle/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.EF.Exceptions.Oracle.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.EF.Exceptions.Oracle/) |

### Middleware e APIs

| Pacote                                        | Descrição                                  | Versão                                                                                                                                                              | Downloads                                                                                                                                                                |
| --------------------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Nuuvify.CommonPack.Middleware**             | Middlewares customizados para ASP.NET Core | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Middleware.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Middleware/)                         | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Middleware.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Middleware/)                         |
| **Nuuvify.CommonPack.Middleware.Abstraction** | Abstrações de middlewares                  | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Middleware.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Middleware.Abstraction/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Middleware.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Middleware.Abstraction/) |
| **Nuuvify.CommonPack.OpenApi**                | Configurações OpenAPI/Swagger              | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.OpenApi.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.OpenApi/)                               | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.OpenApi.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.OpenApi/)                               |
| **Nuuvify.CommonPack.HealthCheck**            | Health checks customizados                 | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.HealthCheck.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.HealthCheck/)                       | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.HealthCheck.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.HealthCheck/)                       |

### Extensões e Utilitários

| Pacote                            | Descrição                                  | Versão                                                                                                                                      | Downloads                                                                                                                                        |
| --------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Nuuvify.CommonPack.Extensions** | Métodos de extensão e Notification Pattern | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Extensions.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Extensions/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Extensions.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Extensions/) |

## 📚 Documentação

Cada pacote possui documentação detalhada em seu respectivo diretório:

### Infraestrutura e Serviços
- 📚 [BackgroundService](src/Nuuvify.CommonPack.BackgroundService/README.md) - **🆕 Com diagnóstico avançado**
- 📚 [AzureServiceBus](src/Nuuvify.CommonPack.AzureServiceBus/README.md)
- 📚 [AzureServiceBus.Abstraction](src/Nuuvify.CommonPack.AzureServiceBus.Abstraction/README.md)
- 📚 [AzureStorage](src/Nuuvify.CommonPack.AzureStorage/README.md)
- 📚 [AzureStorage.Abstraction](src/Nuuvify.CommonPack.AzureStorage.Abstraction/README.md)

### Comunicação e Integração
- 📚 [StandardHttpClient](src/Nuuvify.CommonPack.StandardHttpClient/README.md)
- 📚 [Email](src/Nuuvify.CommonPack.Email/README.md) - **Envio de e-mails via SMTP**
- 📚 [Email.Abstraction](src/Nuuvify.CommonPack.Email.Abstraction/README.md)

### Segurança
- 📚 [Security](src/Nuuvify.CommonPack.Security/README.md)
- 📚 [Security.Abstraction](src/Nuuvify.CommonPack.Security.Abstraction/README.md)
- 📚 [Security.JwtCredentials](src/Nuuvify.CommonPack.Security.JwtCredentials/README.md)
- 📚 [Security.JwtStore.Ef](src/Nuuvify.CommonPack.Security.JwtStore.Ef/README.md)

### Persistência de Dados
- 📚 [UnitOfWork](src/Nuuvify.CommonPack.UnitOfWork/README.md) - **Com queries dinâmicas**
- 📚 [UnitOfWork.Abstraction](src/Nuuvify.CommonPack.UnitOfWork.Abstraction/README.md) - **✨ Com filtros complexos**
- 📚 [Domain](src/Nuuvify.CommonPack.Domain/README.md)
- 📚 [AutoHistory](src/Nuuvify.CommonPack.AutoHistory/README.md)

### Entity Framework Exceptions
- 📚 [EF.Exceptions.Common](src/Nuuvify.CommonPack.EF.Exceptions.Common/README.md)
- 📚 [EF.Exceptions.Db2](src/Nuuvify.CommonPack.EF.Exceptions.Db2/README.md)
- 📚 [EF.Exceptions.Oracle](src/Nuuvify.CommonPack.EF.Exceptions.Oracle/README.md)

### Middleware e APIs
- 📚 [Middleware](src/Nuuvify.CommonPack.Middleware/README.md)
- 📚 [Middleware.Abstraction](src/Nuuvify.CommonPack.Middleware.Abstraction/README.md)
- 📚 [OpenApi](src/Nuuvify.CommonPack.OpenApi/README.md)
- 📚 [HealthCheck](src/Nuuvify.CommonPack.HealthCheck/README.md)

### Extensões e Utilitários
- 📚 [Extensions](src/Nuuvify.CommonPack.Extensions/README.md) - **Notification Pattern e extensões**

### Exemplos
- 📚 [Samples](Samples/README.md) - Exemplos práticos de uso
- 📚 [OrderProcessingWorker](Samples/OrderProcessingWorker/README.md) - Worker com Azure Service Bus

## 🚀 Início Rápido

### Instalação

```bash
# Unit of Work com Entity Framework
dotnet add package Nuuvify.CommonPack.UnitOfWork

# Cliente HTTP com resiliência
dotnet add package Nuuvify.CommonPack.StandardHttpClient

# Envio de e-mails
dotnet add package Nuuvify.CommonPack.Email

# Segurança e JWT
dotnet add package Nuuvify.CommonPack.Security

# Background services com Azure Service Bus
dotnet add package Nuuvify.CommonPack.BackgroundService
```

### Exemplo Básico - Unit of Work

```csharp
// Configuração no Program.cs
builder.Services.AddNuuvifyDbContext<MyDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection"));

// Uso no serviço
public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await _unitOfWork.Repository<Product>().AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _unitOfWork.Repository<Product>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
```

### Exemplo Avançado - Filtros Dinâmicos com Tipos Complexos

```csharp
// Modelo de filtro com propriedades de navegação
public class OrderFilterModel : IQueryableCustom
{
    // Filtro simples
    [QueryOperator(WhereOperator.GreaterThanOrEqualTo)]
    public decimal? MinTotal { get; set; }

    // Filtros com tipos complexos usando notação de ponto
    [QueryOperator(WhereOperator.Contains, HasName = "Customer.Name", CaseSensitive = false)]
    public string CustomerName { get; set; }

    [QueryOperator(WhereOperator.Equals, HasName = "Customer.Address.City")]
    public string City { get; set; }

    [QueryOperator(WhereOperator.ContainsWithLikeForList, HasName = "Items.Product.Category", CaseSensitive = false)]
    public List<string> ProductCategories { get; set; }

    // Filtro aninhado de múltiplos níveis
    [QueryOperator(WhereOperator.GreaterThan, HasName = "Customer.Profile.CreatedDate")]
    public DateTime? CustomerSince { get; set; }
}

// Uso do serviço com filtros complexos
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<IPagedList<Order>> GetFilteredOrdersAsync(OrderFilterModel filter)
    {
        return await _unitOfWork.Repository<Order>()
            .Filter(filter)  // ✨ Aplica automaticamente todos os filtros
            .OrderByDescending(o => o.OrderDate)
            .GetPagedListAsync(pageIndex: 1, pageSize: 20);
    }

    // SQL gerado automaticamente:
    // WHERE Orders.Total >= @p0
    //   AND UPPER(Customer.Name) LIKE '%' + UPPER(@p1) + '%'
    //   AND Customer.Address.City = @p2
    //   AND (Items.Product.Category LIKE '%' + @p3 + '%' OR Items.Product.Category LIKE '%' + @p4 + '%')
    //   AND Customer.Profile.CreatedDate > @p5
}

// Exemplo de uso
var filter = new OrderFilterModel
{
    MinTotal = 100.00m,
    CustomerName = "João",
    City = "São Paulo",
    ProductCategories = new List<string> { "Electronics", "Computers" },
    CustomerSince = DateTime.Now.AddYears(-2)
};

var orders = await orderService.GetFilteredOrdersAsync(filter);
```

### Exemplo Básico - Email

```csharp
// Configuração no appsettings.json
{
  "EmailConfig": {
    "EmailServerConfiguration": {
      "ServerHost": "smtp.gmail.com",
      "Port": 587,
      "Security": "StartTls",
      "AccountUserName": "seu-email@gmail.com",
      "AccountPassword": "sua-senha"
    }
  }
}

// Configuração no Program.cs
builder.Services.AddScoped<IEmail, Email>();
builder.Services.Configure<EmailServerConfiguration>(
    builder.Configuration.GetSection("EmailConfig:EmailServerConfiguration"));

// Uso no serviço
public class NotificationService
{
    private readonly IEmail _emailService;

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var recipients = new Dictionary<string, string> { { email, name } };
        var senders = new Dictionary<string, string> { { "noreply@empresa.com", "Empresa" } };

        await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: "Bem-vindo!",
            message: $"<h1>Olá {name}!</h1><p>Seja bem-vindo à nossa plataforma.</p>"
        );
    }
}
```

### Exemplo Básico - HTTP Client

```csharp
// Configuração no Program.cs
builder.Services.AddStandardHttpClient<MyApiClient>(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
});

// Uso no serviço
public class MyApiClient
{
    private readonly HttpClient _httpClient;

    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Product>($"/products/{id}");
    }
}
```

## 🧪 Testes

> ✅ **Recentemente corrigido**: Problemas de `ObjectDisposedException` em testes foram resolvidos com gerenciamento adequado do ciclo de vida do EF Core.

O projeto possui uma estratégia de testes abrangente com **12 projetos de teste** cobrindo todos os pacotes principais:

### Projetos de Teste Disponíveis

| Projeto de Teste                                    | Tipo        | Descrição                                    |
| --------------------------------------------------- | ----------- | -------------------------------------------- |
| **Nuuvify.CommonPack.UnitOfWork.InMemory.xTest**    | Unit        | Testes unitários com InMemory provider       |
| **Nuuvify.CommonPack.UnitOfWork.Integration.xTest** | Integration | Testes de integração com SQL Server (Docker) |
| **Nuuvify.CommonPack.Extensions.xTest**             | Unit        | Testes de extensões e Notification Pattern   |
| **Nuuvify.CommonPack.Email.xTest**                  | Unit        | Testes do serviço de e-mail                  |
| **Nuuvify.CommonPack.Security.xTest**               | Unit        | Testes de segurança e JWT                    |
| **Nuuvify.CommonPack.StandardHttpClient.xTest**     | Unit        | Testes do cliente HTTP                       |
| **Nuuvify.CommonPack.BackgroundService.xTest**      | Unit        | Testes de background services                |
| **Nuuvify.CommonPack.AzureServiceBus.xTest**        | Unit        | Testes de Azure Service Bus                  |
| **Nuuvify.CommonPack.AzureStorage.xTest**           | Unit        | Testes de Azure Storage                      |
| **Nuuvify.CommonPack.Middleware.xTest**             | Unit        | Testes de middlewares                        |
| **Nuuvify.CommonPack.HealthCheck.xTest**            | Unit        | Testes de health checks                      |
| **Nuuvify.CommonPack.Domain.xTest**                 | Unit        | Testes de entidades de domínio               |

### Testes Unitários (InMemory)
**Foco**: Velocidade e feedback rápido

- ⚡ **Rápidos**: Execução em < 1 segundo
- 💚 **Leves**: Usa EF Core InMemory provider
- ✅ **CI/CD Friendly**: Sem dependências externas (não requer Docker)
- 🎯 **Uso**: TDD, desenvolvimento rápido, validação de lógica de negócio

```powershell
# Executar todos os testes unitários
dotnet test --filter "Category=Unit"

# Executar testes de um projeto específico
dotnet test test/Nuuvify.CommonPack.Extensions.xTest
```

### Testes de Integração (SQL Server via Docker)
**Foco**: Fidelidade e validação real

- 🐘 **SQL Server Real**: Usa Testcontainers com SQL Server 2022
- ✅ **Alta Fidelidade**: Testa queries SQL específicas e collations
- ⚠️ **Requer Docker**: Docker Desktop deve estar em execução
- 🎯 **Uso**: Validação de features específicas do SQL Server, testes de regressão

```powershell
# Executar testes de integração (requer Docker)
dotnet test --filter "Category=Integration"

# Executar projeto específico de integração
dotnet test test/Nuuvify.CommonPack.UnitOfWork.Integration.xTest
```

### Executar Todos os Testes

```powershell
# Executar todos os testes
dotnet test

# Executar com cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Executar com relatório detalhado
dotnet test --logger "console;verbosity=detailed"
```

📖 **Documentação Completa**: [Integration Tests README](test/Nuuvify.CommonPack.UnitOfWork.Integration.xTest/README.md)

### Comparação Rápida

| Aspecto    | Unit (InMemory) | Integration (SQL Server) |
| ---------- | --------------- | ------------------------ |
| Velocidade | ⚡ < 1s          | 🐢 2-4s                   |
| Requisitos | ✅ Nenhum        | ⚠️ Docker                 |
| Fidelidade | ⚠️ Simulado      | ✅ SQL Real               |
| CI/CD      | ✅ Sempre        | ⚠️ Depende do Docker      |
| Cobertura  | 🎯 Lógica        | 🎯 Infraestrutura         |
| Feedback   | 🚀 Instantâneo   | ⏱️ Alguns segundos        |

### Estratégia de Testes Recomendada

1. **Durante o desenvolvimento**: Use testes unitários (InMemory) para feedback rápido
2. **Antes do commit**: Execute todos os testes unitários
3. **Antes do merge**: Execute testes de integração com Docker
4. **Pipeline CI/CD**: Execute ambos em paralelo quando possível

### Cobertura de Código

O projeto mantém alta cobertura de código:
- **Testes Unitários**: > 80% de cobertura
- **Testes de Integração**: Valida cenários críticos
- **SonarCloud**: Análise contínua de qualidade

## 🤝 Contribuindo

Contribuições são bem-vindas! Veja como você pode ajudar:

### Como Contribuir

1. **Fork** o repositório
2. **Clone** seu fork: `git clone https://github.com/seu-usuario/Nuuvify.CommonPack.git`
3. **Crie uma branch**: `git checkout -b feature/minha-feature`
4. **Faça suas alterações** seguindo as convenções do projeto
5. **Execute os testes**: `dotnet test`
6. **Commit** suas mudanças: `git commit -m 'feat: adiciona nova feature'`
7. **Push** para a branch: `git push origin feature/minha-feature`
8. **Abra um Pull Request**

### Convenções

- ✅ **Conventional Commits**: Use commits semânticos (feat, fix, docs, etc.)
- ✅ **EditorConfig**: Siga as regras do `.editorconfig`
- ✅ **Testes**: Escreva testes para novas funcionalidades
- ✅ **Documentação**: Atualize o README quando necessário
- ✅ **Code Review**: Aguarde aprovação antes do merge

### Diretrizes de Código

- Siga os princípios **SOLID**
- Escreva código **limpo e legível**
- Adicione **comentários** onde necessário
- Mantenha **alta cobertura de testes**
- Use **async/await** para operações I/O

### Configuração de Credenciais Git

Se você receber o erro abaixo ao tentar fazer push, significa que há credenciais de outra conta em cache:

```
remote: Permission to nuuvify/Nuuvify.CommonPack.git denied to usuario_anterior.
fatal: unable to access 'https://github.com/nuuvify/Nuuvify.CommonPack.git/': The requested URL returned error: 403
```

**1. Configure o usuário local do repositório:**

```powershell
git config --local user.name "lzocateli"
git config --local user.email "lzocateli@outlook.com"
```

**2. Troque o remote para HTTPS (se estiver usando SSH):**

```powershell
git remote set-url origin https://github.com/nuuvify/Nuuvify.CommonPack.git
```

**3. Liste e remova credenciais antigas do Windows:**

```powershell
# Listar credenciais relacionadas ao GitHub
cmdkey /list | Select-String -Pattern "github"

# Remover cada entrada encontrada (ajuste o target conforme listado)
cmdkey /delete:git:https://github.com
```

**4. Limpe também pelo Git Credential Manager:**

```powershell
# Cole as 3 linhas abaixo e pressione Enter duas vezes
git credential reject
protocol=https
host=github.com
```

**5. Se houver credenciais persistentes no Gerenciador de Credenciais do Windows:**

- Pressione `Win + R` → digite `control /name Microsoft.CredentialManager` → Enter
- Clique em **Credenciais do Windows**
- Localize entradas com `github` no nome (ex: `GitHub for Visual Studio - https://usuario_anterior@github.com/`)
- Clique na entrada e depois em **Remover**

**6. (Opcional) Se usar GitHub CLI:**

```powershell
gh auth logout
gh auth login
# Selecione: GitHub.com → HTTPS → Autentique no navegador com a conta correta
```

**7. Faça o push — o Git Credential Manager abrirá o navegador para autenticar:**

```powershell
git push origin sua-branch
```

> ⚠️ **Dica:** Certifique-se de estar logado no GitHub com a conta correta (`lzocateli`) antes de autorizar no navegador. Se o navegador abrir já logado com outra conta, faça logout no GitHub primeiro.

**8. Confirme que está tudo certo:**

```powershell
# Verificar configuração local
git config user.name
git config user.email

# Se usar gh cli
gh auth status
```

## 📊 Status do Projeto

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=coverage)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)

## 🔗 Links Úteis

### Documentação e Recursos
- 📦 [Pacotes NuGet](https://www.nuget.org/packages?q=nuuvify)
- 📋 [**Changelog**](CHANGELOG.md) - **🆕 Filtros com tipos complexos!**
- 📖 [Wiki](https://github.com/nuuvify/CommonPack/wiki)
- 💡 [Samples e Exemplos](Samples/README.md)

### Desenvolvimento
- 🐛 [Issues](https://github.com/nuuvify/CommonPack/issues)
- 🔄 [Pull Requests](https://github.com/nuuvify/CommonPack/pulls)
- 🚀 [Releases](https://github.com/nuuvify/CommonPack/releases)
- 📊 [SonarCloud](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)

### Comunidade
- 💬 [Discussions](https://github.com/nuuvify/CommonPack/discussions)
- 📧 [Email](mailto:suporte@zocate.li)
- 🌐 [Website](https://www.nuuvify.com)

## 📄 Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

## 👥 Autores e Colaboradores

Desenvolvido e mantido pela equipe **Nuuvify**.

Agradecimentos especiais a todos os [colaboradores](https://github.com/nuuvify/CommonPack/graphs/contributors) que ajudaram a tornar este projeto melhor!

---

**Nuuvify CommonPack** - Construindo soluções robustas para .NET 🚀

[![Made with ❤️ by Nuuvify](https://img.shields.io/badge/Made%20with%20%E2%9D%A4%EF%B8%8F%20by-Nuuvify-blue)](https://www.nuuvify.com)
