# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Aguardando

- Suporte para operadores IN e NOT IN
- Integração com AutoMapper para projeções automáticas
- Suporte para agregações dinâmicas (SUM, COUNT, AVG)
- Cache de queries compiladas para melhor performance

## [3.1.0] - 2024-01-15

### ✨ Adicionado

- **🆕 NOVO OPERADOR**: `ContainsWithLikeForList` - Busca OR em listas de strings
  - Permite busca global com múltiplos termos: `WHERE (Field.Contains(@v1) OR Field.Contains(@v2))`
  - Ideal para implementar busca global, tags, categorias múltiplas
  - Suporte completo a `IEnumerable<string>`, `List<string>`, `Collection<string>`
  - Validação robusta de nulls e listas vazias
  
- **Enums tipados** para melhor type safety:
  - `ExpressionParameterName` - Nomes de parâmetros padronizados
  - `MethodName` - Nomes de métodos de expressão
  - Extensões de string para conversão automática
  
- **Documentação XML** completa em todos os métodos públicos
- **Properties.cs** para configuração centralizada de `InternalsVisibleTo`
- **README.md** abrangente com exemplos de todos os operadores
- **Examples** folder com classes demonstrativas

### 🔧 Melhorado

- Método `GetClosureOverConstant` otimizado com:
  - Melhor tratamento de tipos nullable
  - Validação mais robusta de expressões
  - Suporte aprimorado para conversões de tipo
  - Documentação XML detalhada

- Performance geral da biblioteca:
  - Expressões compiladas de forma mais eficiente
  - Redução de alocações desnecessárias
  - Validações otimizadas

### 🐛 Corrigido

- Correção em operadores nullable que não funcionavam corretamente em alguns cenários
- Melhoria na validação de parâmetros de entrada
- Correção de warnings de nullable reference types

### 🔄 Alterado

- **BREAKING**: Movida configuração `InternalsVisibleTo` do `.csproj` para `Properties.cs`
  - Melhora organização e permite configurações mais flexíveis
  - Segue padrão estabelecido nos outros projetos CommonPack

### 📚 Documentação

- README.md completamente reescrito com:
  - Exemplos de todos os 13 operadores
  - Casos de uso reais com modelos completos
  - Seções de performance e otimização
  - Guias de troubleshooting
  - Referência completa da API
  
- CHANGELOG.md seguindo padrão Keep a Changelog
- Examples com classes demonstrativas de uso

## [3.0.1] - 2023-12-10

### 🐛 Corrigido

- Correção de bug em paginação com ordenação múltipla
- Melhoria na validação de expressões lambda
- Correção de memory leak em queries de longa duração

### 📚 Documentação

- Atualização de exemplos no README
- Correção de links quebrados na documentação

## [3.0.0] - 2023-11-20

### ✨ Adicionado

- Suporte completo ao .NET 8.0
- Novos operadores de comparação:
  - `GreaterThanOrEqualWhenNullable`
  - `LessThanOrEqualWhenNullable`  
  - `EqualsWhenNullable`
- Suporte a ordenação múltipla com sintaxe simples
- Sistema de validação aprimorado para expressões

### 🔄 Alterado

- **BREAKING**: Atualização para .NET 8.0 como target principal
- **BREAKING**: Refatoração das interfaces para melhor extensibilidade
- Melhoria significativa na performance das queries dinâmicas

### ⚠️ Removido

- **BREAKING**: Descontinuado suporte ao .NET Standard 2.0
- Removidos métodos obsoletos da versão 2.x

## [2.5.0] - 2023-09-15

### ✨ Adicionado

- Operador `StartsWith` para buscas por prefixo
- Suporte a case-insensitive em operadores de texto
- Modificadores `UseOr` e `UseNot` em QueryOperator
- Sistema de interceptors para queries

### 🔧 Melhorado

- Performance das queries com Contains otimizada
- Redução de 40% no tempo de compilação de expressões
- Melhor tratamento de caracteres especiais em buscas

### 🐛 Corrigido

- Correção em filtros com valores null
- Melhoria na estabilidade com DbContext concorrente

## [2.4.2] - 2023-08-01

### 🐛 Corrigido

- Correção crítica em paginação com filtros complexos
- Melhoria na validação de PageSize para prevenir valores inválidos
- Correção de race condition em cenários multi-thread

### 🔧 Melhorado

- Otimização de memória em queries grandes
- Melhoria nos logs de debug

## [2.4.1] - 2023-07-10

### 🐛 Corrigido

- Correção de NullReferenceException em filtros vazios
- Melhoria na serialização de modelos de filtro
- Correção de comportamento inconsistente em ordenação

## [2.4.0] - 2023-06-20

### ✨ Adicionado

- Suporte completo a Entity Framework Core 7.0
- Novo sistema de paginação com metadados avançados
- Operadores de comparação numérica: `GreaterThan`, `LessThan`, etc.
- Suporte a filtros em propriedades navegacionais

### 🔧 Melhorado

- Melhoria na performance de queries complexas em 30%
- Otimização do sistema de cache interno
- Melhor integração com DI container

### 🐛 Corrigido

- Correção em filtros com DateTime e fusos horários
- Melhoria na compatibilidade com diferentes providers de banco

## [2.3.0] - 2023-04-15

### ✨ Adicionado

- Sistema de filtros dinâmicos com QueryOperator attribute
- Operadores básicos: `Equals`, `NotEquals`, `Contains`
- Paginação básica com PageIndex e PageSize
- Suporte inicial a ordenação

### 🔧 Melhorado

- Refatoração completa da arquitetura interna
- Melhor separação de responsabilidades
- Documentação inicial das APIs públicas

## [2.2.0] - 2023-03-01

### ✨ Adicionado

- Implementação inicial do padrão Unit of Work
- Integração básica com Entity Framework Core
- Suporte a operações CRUD genéricas
- Sistema básico de transações

### 🔧 Melhorado

- Estrutura de projeto organizada
- Testes unitários básicos
- CI/CD pipeline configurado

## [2.1.0] - 2023-02-10

### ✨ Adicionado

- Interfaces abstratas para Unit of Work
- Estrutura base para filtros dinâmicos
- Documentação inicial do projeto

## [2.0.0] - 2023-01-15

### ✨ Adicionado

- Versão inicial do projeto
- Estrutura básica de classes
- Configuração do projeto .NET

### 📚 Documentação

- README inicial
- Estrutura de versionamento definida

---

## Convenções do Changelog

### Tipos de Mudanças

- **✨ Adicionado** para novas funcionalidades
- **🔧 Melhorado** para mudanças em funcionalidades existentes
- **🔄 Alterado** para mudanças que afetam a API existente
- **🐛 Corrigido** para correção de bugs
- **⚠️ Removido** para funcionalidades removidas
- **🔒 Segurança** para correções de vulnerabilidades
- **📚 Documentação** para mudanças apenas na documentação

### Versionamento Semântico

- **MAJOR** (X.0.0): Mudanças incompatíveis na API
- **MINOR** (x.Y.0): Novas funcionalidades mantendo compatibilidade
- **PATCH** (x.y.Z): Correções de bugs mantendo compatibilidade

### Links de Comparação

[Unreleased]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v3.1.0...HEAD
[3.1.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v3.0.1...v3.1.0
[3.0.1]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v3.0.0...v3.0.1
[3.0.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.5.0...v3.0.0
[2.5.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.4.2...v2.5.0
[2.4.2]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.4.1...v2.4.2
[2.4.1]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.4.0...v2.4.1
[2.4.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.3.0...v2.4.0
[2.3.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.2.0...v2.3.0
[2.2.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/nuuvify/Nuuvify.CommonPack/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/nuuvify/Nuuvify.CommonPack/releases/tag/v2.0.0