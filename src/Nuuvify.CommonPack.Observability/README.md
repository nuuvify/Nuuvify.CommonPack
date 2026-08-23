# Nuuvify.CommonPack.Observability

[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Observability.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Observability)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Observability.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Observability)

Implementação em .NET 8 para armazenar contexto de operação isolado por fluxo
assíncrono. A biblioteca resolve a propagação controlada de identificadores de
correlação, trace e operação entre métodos que participam da mesma execução,
sem usar estado global mutável ou depender de ASP.NET Core.

## Índice

- [Quando usar](#quando-usar)
- [O que resolve](#o-que-resolve)
- [O que não resolve](#o-que-não-resolve)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Exemplo de uso](#exemplo-de-uso)
- [Boas práticas](#boas-práticas)
- [Compatibilidade](#compatibilidade)
- [Troubleshooting](#troubleshooting)

## Quando usar

Use este pacote na camada de infraestrutura ou no composition root quando uma
API, worker, job ou consumidor de mensagens precisar disponibilizar um contexto
de operação para logging, telemetria e propagação de correlação.

Use-o especialmente quando:

- várias chamadas assíncronas precisam ler o mesmo contexto da operação;
- execuções concorrentes não podem compartilhar correlation IDs;
- o host deve integrar contexto próprio com `Activity.Current` sem acoplar o
    domínio ao framework de hospedagem.

Não é necessário adicioná-lo a uma aplicação que já possui um mecanismo
equivalente e corretamente isolado.

## O que resolve

- fornece `OperationContextAccessor`, baseado em `AsyncLocal`;
- permite escopos aninhados com restauração automática por
    `OperationContextScope`;
- evita que o contexto de uma mensagem ou requisição seja reutilizado por outra
    execução assíncrona;
- mantém o contrato de contexto separado de HTTP, claims, headers e secrets.

## O que não resolve

Esta biblioteca não cria correlation IDs para protocolos automaticamente, não
configura OpenTelemetry, não coleta logs e não valida tokens. O adapter do host
deve decidir como obter os identificadores e quando abrir o escopo.

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.Observability" Version="2.8.0" />
```

O pacote depende de `Nuuvify.CommonPack.Observability.Abstraction`, instalado
automaticamente pelo NuGet.

## Configuração

Registre um único accessor por processo usando o container de DI:

```csharp
services.AddSingleton<IOperationContextAccessor, OperationContextAccessor>();
```

O accessor é stateless fora do fluxo assíncrono atual. Não registre um novo
accessor para cada chamada; abra escopos de operação no limite da requisição,
mensagem ou job.

## Exemplo de uso

```csharp
using Nuuvify.CommonPack.Observability;
using Nuuvify.CommonPack.Observability.Abstraction;

public sealed class MessageProcessor
{
        private readonly IOperationContextAccessor _accessor;

        public MessageProcessor(IOperationContextAccessor accessor)
        {
                _accessor = accessor;
        }

        public async Task ProcessAsync(string correlationId, CancellationToken cancellationToken)
        {
                var operation = new OperationContext(
                        correlationId: correlationId,
                        operationId: Guid.NewGuid().ToString("N"));

                using var scope = new OperationContextScope(_accessor, operation);
                await PersistAuditAsync(_accessor.Current, cancellationToken);
        }

        private static Task PersistAuditAsync(
                OperationContext context,
                CancellationToken cancellationToken)
        {
                return Task.CompletedTask;
        }
}
```

Ao sair do `using`, o contexto anterior é restaurado. Isso permite composição
segura de operações aninhadas e facilita testes determinísticos.

## Boas práticas

- abra o escopo no limite da operação, não no construtor de um singleton;
- use identificadores fornecidos pelo protocolo quando forem confiáveis e gere
    um fallback no adapter quando necessário;
- mantenha metadata limitada a dados técnicos e não sensíveis;
- propague `CancellationToken` para o trabalho iniciado dentro do escopo;
- use `ActivitySource` para trace distribuído e deixe este pacote cuidar apenas
    do contexto neutro.

## Compatibilidade

- alvo: `net8.0`;
- depende de `Nuuvify.CommonPack.Observability.Abstraction`;
- não depende de ASP.NET Core, hosting, logging ou banco de dados.

## Troubleshooting

### O contexto aparece vazio

Verifique se o accessor foi registrado no DI e se o código consumidor executa
dentro de um `OperationContextScope` ativo.

### Duas mensagens compartilham o mesmo correlation ID

Não mantenha um `OperationContext` em campo de singleton. Crie um contexto
novo por mensagem ou requisição e abra um escopo independente.

### O contexto não chega a uma tarefa assíncrona

Confirme que a tarefa faz parte do fluxo assíncrono atual e não foi criada com
um contexto artificialmente suprimido. Evite copiar o contexto para estado
global ou cache compartilhado.
