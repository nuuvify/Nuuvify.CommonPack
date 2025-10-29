# Nuuvify.CommonPack

[![Build Status - Main](https://dev.azure.com/nuuvers/Nuuvify/_apis/build/status/CI-Github-Nuuvify.CommonPack?repoName=lzocateli00%2FNuuvify.CommonPack&branchName=main)](https://dev.azure.com/nuuvers/Nuuvify/_build/latest?definitionId=23&repoName=lzocateli00%2FNuuvify.CommonPack&branchName=main)
[![Build Status - QAS](https://dev.azure.com/nuuvers/Nuuvify/_apis/build/status/CI-Github-Nuuvify.CommonPack?repoName=lzocateli00%2FNuuvify.CommonPack&branchName=qas)](https://dev.azure.com/nuuvers/Nuuvify/_build/latest?definitionId=23&repoName=lzocateli00%2FNuuvify.CommonPack&branchName=qas)

Coleção de bibliotecas .NET para desenvolvimento de aplicações robustas, escaláveis e de alta performance.

## 🆕 Novidades

### BackgroundService v2.0 - Diagnóstico Avançado
- **✨ Propriedades de diagnóstico contextuais** para Dead Letter Queue e Abandon
- **🔧 Arquitetura modular refatorada** com complexidade cognitiva reduzida
- **🔍 Troubleshooting aprimorado** com metadados detalhados
- **📊 Observabilidade avançada** para monitoramento e métricas

## 📦 Pacotes Disponíveis

| Pacote                                    | Descrição                                      | Versão                                                                                                                                                      | Downloads                                                                                                                                                        |
| ----------------------------------------- | ---------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Nuuvify.CommonPack.BackgroundService**  | 🆕 Serviços de background com Azure Service Bus | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)   | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)   |
| **Nuuvify.CommonPack.StandardHttpClient** | Cliente HTTP otimizado                         | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.StandardHttpClient.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.StandardHttpClient/) | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.StandardHttpClient.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.StandardHttpClient/) |
| **Nuuvify.CommonPack.Security**           | Componentes de segurança                       | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Security.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security/)                     | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Security.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Security/)                     |
| **Nuuvify.CommonPack.Extensions**         | Métodos de extensão                            | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Extensions.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Extensions/)                 | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Extensions.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Extensions/)                 |
| **Nuuvify.CommonPack.UnitOfWork**         | Padrão Unit of Work                            | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.UnitOfWork.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/)                 | [![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.UnitOfWork.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/)                 |

##  Documentação

Cada pacote possui documentação detalhada:

- 📚 [BackgroundService](src/Nuuvify.CommonPack.BackgroundService/README.md) **🆕 Com diagnóstico avançado**
- 📚 [StandardHttpClient](src/Nuuvify.CommonPack.StandardHttpClient/README.md)
- 📚 [Security](src/Nuuvify.CommonPack.Security/README.md)
- 📚 [Extensions](src/Nuuvify.CommonPack.Extensions/README.md)
- 📚 [UnitOfWork](src/Nuuvify.CommonPack.UnitOfWork/README.md)

## 🧪 Testes

O projeto possui uma estratégia de testes abrangente separada em dois tipos:

### Testes Unitários (InMemory)
**Projeto**: `Nuuvify.CommonPack.UnitOfWork.InMemory.xTest`

- ⚡ **Rápidos**: Execução em < 1 segundo
- 💚 **Leves**: Usa EF Core InMemory provider
- ✅ **CI/CD Friendly**: Sem dependências externas (não requer Docker)
- 🎯 **Uso**: TDD, desenvolvimento rápido, validação de lógica de negócio

```powershell
# Executar testes unitários
dotnet test --filter "Category=Unit"
```

### Testes de Integração (SQL Server via Docker)
**Projeto**: `Nuuvify.CommonPack.UnitOfWork.Integration.xTest`

- 🐘 **SQL Server Real**: Usa Testcontainers com SQL Server 2022
- ✅ **Alta Fidelidade**: Testa queries SQL específicas e collations
- ⚠️ **Requer Docker**: Docker Desktop deve estar em execução
- 🎯 **Uso**: Validação de features específicas do SQL Server, testes de regressão

```powershell
# Executar testes de integração (requer Docker)
dotnet test --filter "Category=Integration"
```

📖 **Documentação Completa**: [Integration Tests README](test/Nuuvify.CommonPack.UnitOfWork.Integration.xTest/README.md)

### Comparação Rápida

| Aspecto    | Unit (InMemory) | Integration (SQL Server) |
| ---------- | --------------- | ------------------------ |
| Velocidade | ⚡ < 1s          | 🐢 2-4s                   |
| Requisitos | ✅ Nenhum        | ⚠️ Docker                 |
| Fidelidade | ⚠️ Simulado      | ✅ SQL Real               |
| CI/CD      | ✅ Sempre        | ⚠️ Depende do Docker      |

## 🔗 Links

- 📦 [Pacotes NuGet](https://www.nuget.org/packages?q=nuuvify)
- 📋 [Changelog](CHANGELOG.md)
- 🐛 [Issues](https://github.com/nuuvify/CommonPack/issues)
