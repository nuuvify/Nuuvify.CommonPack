# Nuuvify.CommonPack.Domain

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![Build Status - Main](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=main)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvify/CommonPack/_apis/build/status/nuuvify.CommonPack?branchName=qas)](https://dev.azure.com/nuuvify/CommonPack/_build/latest?definitionId=YOUR_DEFINITION_ID&branchName=qas)

Biblioteca com implementações comuns para projetos baseados em Domain Driven Design (DDD). Fornece classes base, interfaces, Value Objects, enums e padrões essenciais para construção de domínios robustos.

## 📋 Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Dependências](#dependências)
- [Configuração](#configuração)
- [Uso](#uso)
  - [Classes Base](#classes-base)
  - [Value Objects](#value-objects)
  - [MediatR Integration](#mediatr-integration)
  - [Specifications Pattern](#specifications-pattern)
  - [Domain Events](#domain-events)
- [Exemplos Práticos](#exemplos-práticos)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

## Funcionalidades

- ✅ **Classes base para Domain** com BaseDomain e AggregateRoot
- ✅ **Value Objects prontos** (CPF, CNPJ, Email, Endereço, etc.)
- ✅ **Enums padronizados** para domínios corporativos
- ✅ **Integração com MediatR** para CQRS e Domain Events
- ✅ **Specifications Pattern** para validações complexas de domínio
- ✅ **Domain Events** com observabilidade e versionamento
- ✅ **AutoMapper integration** para mapeamentos automáticos
- ✅ **FluentValidator extensions** com validações customizadas
- ✅ **Notification Pattern** herdado de Nuuvify.CommonPack.Extensions
- ✅ **Localização pt-BR** para mensagens de validação
- ✅ **Compatibilidade .NET Standard 2.1** para máxima portabilidade

## Instalação

### Via Package Manager Console
```powershell
Install-Package Nuuvify.CommonPack.Domain
```

### Via .NET CLI
```bash
dotnet add package Nuuvify.CommonPack.Domain
```

### Via PackageReference
```xml
<PackageReference Include="Nuuvify.CommonPack.Domain" Version="X.X.X" />
```

## Dependências

### NuGet Packages

| Package        | Version | Descrição                                                                                          |
| -------------- | ------- | -------------------------------------------------------------------------------------------------- |
| **AutoMapper** | 12.0.1  | Biblioteca para mapeamento automático entre objetos, facilitando conversões entre DTOs e entidades |
| **MediatR**    | 12.2.0  | Implementação do padrão Mediator para .NET, essencial para CQRS e Domain Events                    |

### Project References

| Project                           | Descrição                                                                                                                 |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.Extensions** | Biblioteca base com Notification Pattern, extensões para collections, strings, validações e outros utilitários essenciais |

### Framework

- **.NET Standard 2.1**: Garante compatibilidade com .NET Core 3.0+, .NET 5+, .NET 6+, .NET 8+ e .NET Framework 4.7.2+

## Configuração

### 1. Dependency Injection

```csharp
// Program.cs ou Startup.cs
using Nuuvify.CommonPack.Domain;

// Registrar MediatR (necessário para Domain Events)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Registrar AutoMapper (necessário para BaseDomain)
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Registrar classes de domínio
builder.Services.AddScoped<MinhaClasseDeDominio>();
```

### 2. AutoMapper Profile (Exemplo)

```csharp
public class DomainProfile : Profile
{
    public DomainProfile()
    {
        CreateMap<PessoaEntity, PessoaDto>()
            .ForMember(dest => dest.Documento, opt => opt.MapFrom(src => src.DocumentoPessoa.ToString()));
    }
}
```

## Uso

### Classes Base

#### BaseDomain

Classe base para implementar lógica de domínio com suporte ao Notification Pattern:

```csharp
public class PessoaService : BaseDomain
{
    public PessoaService(IMediator mediator, IMapper mapper)
        : base(mediator, mapper)
    {
    }

    public async Task<PessoaDto> CriarPessoaAsync(CriarPessoaCommand command)
    {
        // Validar usando Notification Pattern
        if (string.IsNullOrEmpty(command.Nome))
        {
            AddNotification("Nome", "Nome é obrigatório");
        }

        if (!IsValid())
        {
            return null;
        }

        // Criar entidade
        var pessoa = new PessoaEntity(command.Nome, command.Documento);

        // Mapear para DTO
        return _mapper.Map<PessoaDto>(pessoa);
    }

    public override IList<NotificationR> ValidationResult()
    {
        return base.ValidationResult();
    }
}
```

#### AggregateRoot

Classe base para entidades que são raiz de agregação:

```csharp
public class PedidoEntity : AggregateRoot
{
    public DateTime DataPedido { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<ItemPedido> Itens { get; private set; }

    public PedidoEntity(DateTime dataPedido)
    {
        Id = Guid.NewGuid();
        DataPedido = dataPedido;
        Itens = new List<ItemPedido>();
    }

    public void AdicionarItem(string produto, decimal valor, int quantidade)
    {
        var item = new ItemPedido(produto, valor, quantidade);
        Itens.Add(item);
        RecalcularTotal();
    }

    private void RecalcularTotal()
    {
        ValorTotal = Itens.Sum(x => x.Valor * x.Quantidade);
    }
}
```

### Value Objects

#### CPF e CNPJ

```csharp
public class PessoaEntity : DomainEntity
{
    public DocumentoPessoa Documento { get; private set; }
    public string Nome { get; private set; }

    public PessoaEntity(string nome, string cpf, string cnpj, string tipoPessoa)
    {
        Nome = nome;

        var cpfObj = new Cpf(cpf);
        var cnpjObj = new Cnpj(cnpj);
        var tipo = new TipoPessoa(tipoPessoa, DateTime.Now);

        Documento = new DocumentoPessoa(cpfObj, cnpjObj, tipo);
    }

    public string ObterDocumentoFormatado()
    {
        if (Documento.TipoDaPessoa == "F")
        {
            var cpf = new Cpf(Documento.Cpf);
            return cpf.Mascara(); // "999.999.999-99"
        }
        else
        {
            var cnpj = new Cnpj(Documento.Cnpj);
            return cnpj.Mascara(); // "99.999.999/9999-99"
        }
    }
}
```

#### Email e Endereço

```csharp
public class ContatoEntity : DomainEntity
{
    public EmailPessoa Email { get; private set; }
    public Endereco EnderecoResidencial { get; private set; }

    public ContatoEntity(string email, string logradouro, string cidade, string uf, string cep)
    {
        Email = new EmailPessoa(email);

        EnderecoResidencial = new Endereco(
            tipoLogradouro: "Rua",
            logradouro: logradouro,
            codigoMunicipio: "3550308", // Código IBGE
            nomeMunicipio: cidade,
            uf: uf,
            bairro: "Centro",
            cep: cep,
            numero: "123",
            complemento: "Apto 101",
            siglaPais: "BR"
        );
    }

    public bool ValidarContato()
    {
        return Email.IsValid() && EnderecoResidencial.IsValid();
    }
}
```

### MediatR Integration

#### Comandos e Handlers

```csharp
public class CriarPedidoCommand : ICommandR
{
    public string Cliente { get; set; }
    public List<ItemPedidoDto> Itens { get; set; }
    public bool SaveChanges { get; set; } = true;
    public bool RemoveNotificationsBeginning { get; set; } = false;
}

public class CriarPedidoHandler : IRequestHandler<CriarPedidoCommand, ICommandResultR>
{
    private readonly IRepository<PedidoEntity> _repository;

    public CriarPedidoHandler(IRepository<PedidoEntity> repository)
    {
        _repository = repository;
    }

    public async Task<ICommandResultR> Handle(CriarPedidoCommand request, CancellationToken cancellationToken)
    {
        var pedido = new PedidoEntity(DateTime.Now);

        foreach (var item in request.Itens)
        {
            pedido.AdicionarItem(item.Produto, item.Valor, item.Quantidade);
        }

        await _repository.AddAsync(pedido);

        if (request.SaveChanges)
        {
            await _repository.SaveChangesAsync();
        }

        return new NotificationResult { Success = true };
    }
}
```

### Specifications Pattern

```csharp
public class PedidoValidoSpecification : BaseSpecification<PedidoEntity>
{
    public override async Task IsSatisfactory(PedidoEntity entity)
    {
        if (entity.ValorTotal <= 0)
        {
            AddNotification("ValorTotal", "Pedido deve ter valor maior que zero");
        }

        if (!entity.Itens.Any())
        {
            AddNotification("Itens", "Pedido deve ter pelo menos um item");
        }

        foreach (var item in entity.Itens)
        {
            if (item.Quantidade <= 0)
            {
                AddNotification("Quantidade", $"Quantidade do item {item.Produto} deve ser maior que zero");
            }
        }

        await Task.CompletedTask;
    }
}

// Uso da Specification
public class PedidoService : BaseDomain
{
    public async Task<bool> ValidarPedidoAsync(PedidoEntity pedido)
    {
        var spec = new PedidoValidoSpecification();
        await spec.IsSatisfactory(pedido);

        if (!spec.ValidationResult().Any())
        {
            return true;
        }

        AddNotifications(spec.ValidationResult());
        return false;
    }
}
```

### Domain Events

```csharp
public class PedidoCriadoEvent : DomainEvent<Guid>
{
    public string Cliente { get; }
    public decimal Valor { get; }
    public int QuantidadeItens { get; }

    public PedidoCriadoEvent(Guid pedidoId, string cliente, decimal valor, int quantidadeItens, string version)
        : base(pedidoId, version)
    {
        Cliente = cliente;
        Valor = valor;
        QuantidadeItens = quantidadeItens;
    }
}

// Handler para o Domain Event
public class PedidoCriadoEventHandler : INotificationHandler<PedidoCriadoEvent>
{
    private readonly ILogger<PedidoCriadoEventHandler> _logger;

    public PedidoCriadoEventHandler(ILogger<PedidoCriadoEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PedidoCriadoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Pedido criado: {notification.SourceId} - Cliente: {notification.Cliente} - Valor: {notification.Valor}");

        // Implementar lógica adicional (enviar email, atualizar estoque, etc.)

        return Task.CompletedTask;
    }
}
```

## Exemplos Práticos

### Exemplo 1: Sistema de Cadastro de Pessoas

```csharp
public class CadastrarPessoaService : BaseDomain
{
    public CadastrarPessoaService(IMediator mediator, IMapper mapper)
        : base(mediator, mapper)
    {
    }

    public async Task<PessoaDto> CadastrarAsync(CadastrarPessoaCommand command)
    {
        // Validar dados básicos
        if (string.IsNullOrEmpty(command.Nome))
        {
            AddNotification("Nome", "Nome é obrigatório");
        }

        // Criar e validar CPF/CNPJ
        var cpf = new Cpf(command.Cpf);
        var cnpj = new Cnpj(command.Cnpj);
        var tipoPessoa = new TipoPessoa(command.TipoPessoa, command.DataNascimento);

        var documento = new DocumentoPessoa(cpf, cnpj, tipoPessoa);

        if (!documento.IsValid())
        {
            AddNotifications(documento.Notifications);
        }

        // Validar email
        var email = new EmailPessoa(command.Email);
        if (!email.IsValid())
        {
            AddNotifications(email.Notifications);
        }

        // Se há erros, retornar
        if (!IsValid())
        {
            return null;
        }

        // Criar entidade
        var pessoa = new PessoaEntity
        {
            Nome = command.Nome,
            Documento = documento,
            Email = email
        };

        // Disparar Domain Event
        var evento = new PessoaCadastradaEvent(pessoa.Id, pessoa.Nome, "v1.0");
        await _mediator.Publish(evento);

        // Retornar DTO
        return _mapper.Map<PessoaDto>(pessoa);
    }
}
```

### Exemplo 2: Validação com Specifications

```csharp
public class ClienteElegivelParaCreditoSpec : BaseSpecification<ClienteEntity>
{
    private readonly decimal _valorSolicitado;

    public ClienteElegivelParaCreditoSpec(decimal valorSolicitado)
    {
        _valorSolicitado = valorSolicitado;
    }

    public override async Task IsSatisfactory(ClienteEntity entity)
    {
        // Validar idade
        var idade = DateTime.Now.Year - entity.DataNascimento.Year;
        if (idade < 18)
        {
            AddNotification("Idade", "Cliente deve ser maior de idade");
        }

        // Validar renda
        if (entity.RendaMensal < _valorSolicitado * 0.3m)
        {
            AddNotification("Renda", "Renda insuficiente para o valor solicitado");
        }

        // Validar histórico (simulação)
        if (entity.TemRestricoes)
        {
            AddNotification("Restricoes", "Cliente possui restrições no CPF");
        }

        await Task.CompletedTask;
    }
}

// Uso no serviço
public class CreditoService : BaseDomain
{
    public async Task<bool> AprovarCreditoAsync(ClienteEntity cliente, decimal valor)
    {
        var spec = new ClienteElegivelParaCreditoSpec(valor);
        await spec.IsSatisfactory(cliente);

        if (spec.ValidationResult().Any())
        {
            AddNotifications(spec.ValidationResult());
            return false;
        }

        // Aprovar crédito
        return true;
    }
}
```

### Exemplo 3: Domain Events com Observabilidade

```csharp
public class ProcessoPagamentoEvent : DomainEvent<Guid>
{
    public decimal Valor { get; }
    public string StatusPagamento { get; }
    public string FormaPagamento { get; }

    public ProcessoPagamentoEvent(Guid pedidoId, decimal valor, string status, string forma, string correlationId)
        : base(pedidoId, correlationId)
    {
        Valor = valor;
        StatusPagamento = status;
        FormaPagamento = forma;
    }
}

// Múltiplos handlers para o mesmo evento
public class LogPagamentoHandler : INotificationHandler<ProcessoPagamentoEvent>
{
    public Task Handle(ProcessoPagamentoEvent notification, CancellationToken cancellationToken)
    {
        // Log estruturado
        Log.Information("Pagamento processado {PedidoId} {Valor} {Status} {CorrelationId}",
            notification.SourceId,
            notification.Valor,
            notification.StatusPagamento,
            notification.Version);

        return Task.CompletedTask;
    }
}

public class EmailPagamentoHandler : INotificationHandler<ProcessoPagamentoEvent>
{
    public Task Handle(ProcessoPagamentoEvent notification, CancellationToken cancellationToken)
    {
        // Enviar email de confirmação
        if (notification.StatusPagamento == "Aprovado")
        {
            // Enviar email de sucesso
        }

        return Task.CompletedTask;
    }
}
```

## API Reference

### Classes Base

#### BaseDomain
- `BaseDomain(IMediator mediator, IMapper mapper)`: Constructor com dependências
- `virtual IList<NotificationR> ValidationResult()`: Retorna lista de notificações

#### AggregateRoot
- Herda de `DomainEntity`
- Utilizada como ponto de entrada da raiz de agregação

#### DomainEvent<TSourceId>
- `DomainEvent(TSourceId sourceId, string version)`: Constructor com ID e versão
- `TSourceId SourceId`: ID da entidade que gerou o evento
- `DateTimeOffset When`: Data/hora da criação do evento
- `string Version`: Versão/correlação para rastreamento

### Interfaces

#### IBaseDomain
- `IList<NotificationR> ValidationResult()`: Contrato para validação

#### ISpecification<TEntity>
- `Task IsSatisfactory(TEntity entity)`: Método de validação da specification
- `IList<NotificationR> ValidationResult()`: Resultado da validação

#### ICommandR
- `bool SaveChanges`: Controla se deve salvar no repositório
- `bool RemoveNotificationsBeginning`: Remove notificações no início do processamento

#### ICommandResultR
- Interface de marcação para resultados de comandos

### Value Objects

#### CPF
- `Cpf(string numero)`: Constructor com validação
- `string Codigo`: CPF sem formatação
- `string Mascara()`: Retorna CPF formatado (999.999.999-99)
- `const int maxCPF = 11`: Tamanho máximo

#### CNPJ
- `Cnpj(string numero)`: Constructor com validação
- `string Codigo`: CNPJ sem formatação
- `string Mascara()`: Retorna CNPJ formatado (99.999.999/9999-99)
- `const int maxCNPJ = 14`: Tamanho máximo

#### EmailPessoa
- `EmailPessoa(string endereco)`: Constructor com validação
- `string Endereco`: Email validado
- `const int maxEndereco = 256`: Tamanho máximo

#### Endereco
- Constructor com todos os campos obrigatórios
- Propriedades: TipoLogradouro, Logradouro, CodigoMunicipio, NomeMunicipio, UF, Bairro, Cep, Numero, Complemento, SiglaPais
- Constantes estáticas para tamanhos mínimos e máximos

### Enums Principais

#### EnumAtivoInativo
- `A`: Ativo
- `I`: Inativo
- `N`: Todos (para consultas)

#### EnumFisicaJuridica
- `F`: Pessoa Física
- `J`: Pessoa Jurídica

#### EnumSimNao
- `S`: Sim
- `N`: Não

## Troubleshooting

### Problemas Comuns

#### MediatR não encontrado
**Problema**: `InvalidOperationException` ao tentar injetar IMediator

**Solução**: Registrar MediatR no container DI
```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

#### AutoMapper não configurado
**Problema**: `AutoMapperMappingException` em BaseDomain

**Solução**: Configurar profiles do AutoMapper
```csharp
builder.Services.AddAutoMapper(typeof(Program));
```

#### Value Objects inválidos
**Problema**: CPF/CNPJ retornando null mesmo com dados corretos

**Causa**: Validação muito restritiva ou dados com formatação

**Solução**: Verificar se os dados estão sem formatação
```csharp
// ✅ Correto
var cpf = new Cpf("12345678901");

// ❌ Incorreto - com formatação
var cpf = new Cpf("123.456.789-01");
```

#### Domain Events não funcionando
**Problema**: Handlers não são executados

**Solução**: Verificar registro do MediatR e implementação dos handlers
```csharp
// Registrar assembly com os handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    Assembly.GetExecutingAssembly(),
    Assembly.GetAssembly(typeof(SeuDomainEventHandler))
));
```

### Logs e Debugging

Para debugging das validações:

```csharp
public class MinhaClasseDominio : BaseDomain
{
    public void ValidarComLog()
    {
        // Seu código de validação

        var erros = ValidationResult();
        foreach (var erro in erros)
        {
            Console.WriteLine($"Campo: {erro.Property}, Mensagem: {erro.Message}");
        }
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
