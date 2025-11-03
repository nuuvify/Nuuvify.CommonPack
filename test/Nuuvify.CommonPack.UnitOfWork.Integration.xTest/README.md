# Nuuvify.CommonPack.UnitOfWork - Integration Tests

Este projeto contém testes de integração para o pacote `Nuuvify.CommonPack.UnitOfWork`, utilizando SQL Server real em containers Docker através do **Testcontainers**.

## 📋 Pré-requisitos

### Docker Desktop
Os testes de integração **requerem Docker** em execução na máquina:

- **Windows/macOS**: [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- **Linux**: [Docker Engine](https://docs.docker.com/engine/install/)

Certifique-se de que o Docker está em execução antes de executar os testes:

```powershell
# Verificar se o Docker está rodando
docker --version
docker ps
```

### SQL Server Container
Na primeira execução, o Testcontainers irá:
- Baixar a imagem `mcr.microsoft.com/mssql/server:2022-latest` (~1.5 GB)
- Criar e configurar um container SQL Server 2022
- Executar migrations e seed de dados de teste

**Importante**: A primeira execução pode levar alguns minutos devido ao download da imagem.

## 🏗️ Estrutura do Projeto

```
Nuuvify.CommonPack.UnitOfWork.Integration.xTest/
├── Configs/               # Configurações de teste
├── Fakers/                # Geradores de dados fake (Bogus)
│   └── ProductFaker.cs    # Gerador de produtos para testes
├── Fixtures/              # Fixtures do xUnit (setup/teardown)
│   └── SqlServerDbContextFixture.cs  # Configuração do container SQL Server
└── Tests/                 # Casos de teste
    └── ExamplesQueryOperatorsTest.cs # Testes dos operadores de query
```

## 🧪 Executando os Testes

### Executar todos os testes de integração

```powershell
# Via dotnet CLI
dotnet test --filter "Category=Integration"

# Ou diretamente no projeto
dotnet test Nuuvify.CommonPack.UnitOfWork.Integration.xTest.csproj
```

### Executar testes específicos

```powershell
# Executar um teste específico
dotnet test --filter "FullyQualifiedName~GetProductByNameAsync_WithMatchingName_ShouldReturnProducts"

# Executar testes com padrão no nome
dotnet test --filter "DisplayName~GetProduct"
```

### Executar com saída detalhada

```powershell
dotnet test --verbosity detailed --filter "Category=Integration"
```

## 📊 Cobertura dos Testes

Os testes de integração cobrem:

### 1. **Operador ContainsWithLikeForList**
- ✅ Busca case-insensitive com SQL Server COLLATE
- ✅ Filtros por múltiplos campos (Name, Description, SKU)
- ✅ Paginação com metadata corretos
- ✅ Ordenação (OrderBy/OrderByDescending)
- ✅ Combinação de filtros complexos
- ✅ Global search em múltiplos campos
- ✅ Validação de performance com dados reais

### 2. **Cenários Testados**
| Teste                                                                         | Descrição               |
| ----------------------------------------------------------------------------- | ----------------------- |
| `GetProductByNameAsync_WithMatchingName_ShouldReturnProducts`                 | Busca básica por nome   |
| `GetProductByNameAsync_CaseInsensitive_ShouldMatchRegardlessOfCase`           | Case-insensitive search |
| `GetProductByNameAsync_WithPagination_ShouldReturnCorrectPage`                | Paginação funcional     |
| `GetProductByNameAsync_WithSorting_ShouldReturnSortedResults`                 | Ordenação correta       |
| `GetProductsWithComplexFilterAsync_WithGlobalSearch_ShouldMatchAnyTerm`       | Busca global            |
| `GetProductsWithComplexFilterAsync_WithMultipleFilters_ShouldApplyAllFilters` | Múltiplos filtros       |
| `AllMethods_ShouldReturnCorrectPaginationMetadata`                            | Metadata de paginação   |
| 5 outros testes...                                                            | Validações adicionais   |

## ⚙️ Configuração do Testcontainers

### SqlServerDbContextFixture.cs

```csharp
// Container SQL Server 2022
Container = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithPassword("YourStrong@Passw0rd")
    .WithPortBinding(1433, true)
    .Build();
```

### Recursos do Container
- **Imagem**: SQL Server 2022 Linux
- **Senha SA**: Definida via builder
- **Porta**: Mapeamento automático (porta aleatória)
- **Lifecycle**: Criado/destruído automaticamente por teste
- **Isolamento**: Cada fixture tem seu próprio container

## 🔧 Troubleshooting

### Docker não está rodando
```
Error: Docker is not running
Solução: Inicie o Docker Desktop e aguarde ele estar completamente inicializado
```

### Porta já em uso
```
Error: Port 1433 is already in use
Solução: O Testcontainers usa porta aleatória automaticamente, não deve ocorrer
```

### Container demora para iniciar
```
Primeira execução: Download de ~1.5 GB pode levar 5-10 minutos
Execuções subsequentes: ~30-60 segundos para inicialização
```

### Timeout na conexão
```csharp
// Aumentar timeout se necessário (SqlServerDbContextFixture.cs)
.WithWaitStrategy(Wait.ForUnixContainer()
    .UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd", 
    "-S", "localhost", "-U", "sa", "-P", password, 
    "-Q", "SELECT 1"))
    .WithStartupTimeout(TimeSpan.FromMinutes(5)); // Aumentar aqui
```

## 🚀 Performance

### Tempos Esperados
| Operação                     | Tempo Aproximado |
| ---------------------------- | ---------------- |
| Primeira execução (download) | 5-10 minutos     |
| Inicialização do container   | 30-60 segundos   |
| Execução de 12 testes        | 2-4 segundos     |
| Teardown do container        | < 1 segundo      |

### Otimizações
- Container é **reutilizado** entre testes da mesma fixture
- Dados são **seedados uma vez** no `InitializeAsync()`
- Transações garantem **isolamento sem cleanup**

## 📚 Dependências

```xml
<PackageReference Include="Testcontainers.MsSql" Version="3.10.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="Bogus" Version="35.6.1" />
```

## 🔗 Links Úteis

- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [SQL Server Docker Images](https://hub.docker.com/_/microsoft-mssql-server)
- [xUnit Documentation](https://xunit.net/)
- [Bogus - Fake Data Generator](https://github.com/bchavez/Bogus)

## 📝 Notas

- **Categoria**: Todos os testes têm `[Trait("Category", "Integration")]`
- **CI/CD**: Configure variável de ambiente `DOCKER_HOST` se necessário
- **Custo**: Testes consomem recursos (CPU/RAM) durante execução
- **Limpeza**: Containers são automaticamente removidos após os testes
- **Logs**: Container logs disponíveis via `docker logs <container_id>` durante execução

## 🆚 Comparação com Testes InMemory

| Aspecto          | InMemory                 | Integration (SQL Server) |
| ---------------- | ------------------------ | ------------------------ |
| **Velocidade**   | ⚡ Muito rápido (< 1s)    | 🐢 Mais lento (2-4s)      |
| **Requisitos**   | ✅ Nenhum                 | ⚠️ Docker obrigatório     |
| **Fidelidade**   | ⚠️ Comportamento simulado | ✅ SQL Server real        |
| **CI/CD**        | ✅ Sempre executável      | ⚠️ Requer Docker no CI    |
| **Casos de Uso** | Unit tests, TDD rápido   | Testes de integração     |
| **Recursos**     | 💚 Mínimo (memória)       | 🟡 Médio (Docker)         |

## 🎯 Quando Usar Cada Tipo

### Use InMemory Tests quando:
- ✅ Desenvolvimento rápido com TDD
- ✅ CI/CD sem Docker disponível
- ✅ Validar lógica de negócio isolada
- ✅ Testar comportamento genérico do EF Core

### Use Integration Tests quando:
- ✅ Validar queries SQL específicas
- ✅ Testar features específicas do SQL Server (COLLATE, Full-Text, etc.)
- ✅ Verificar performance com dados reais
- ✅ Testes de regressão críticos antes de release

---

**Autor**: Nuuvify Development Team  
**Versão**: 3.2.0  
**Última Atualização**: 2024
