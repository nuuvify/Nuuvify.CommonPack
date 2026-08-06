# Plano de Implementacao - Ciclo de Vida para Descarte Assincrono Idempotente no BackgroundService

## Gestao de Status

| Campo | Valor |
|---|---|
| Status | Concluido |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsavel | Time BackgroundService |
| Ultima revisao | 2026-08-06 |

### Historico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | Em andamento -> Concluido | Implementacao finalizada e validada por testes |

## Status de Execucao
Implementacao concluida no escopo definido para o pacote `Nuuvify.CommonPack.BackgroundService`.

### Itens concluidos
1. Redesenho do ciclo de vida com descarte assincrono idempotente implementado.
2. Fluxos de `StopAsync`, `Dispose` e `DisposeAsync` alinhados para encerramento seguro de recursos.
3. Remocao de `SuppressMessage` do pacote e exclusao de `GlobalSuppressions.cs`.
4. Validacao de regressao dos testes do pacote concluida com sucesso (51 testes aprovados).
5. Organizacao da classe em arquivos parciais para facilitar manutencao:
  - `ServiceBusBackgroundService.cs` (fluxo principal e contrato)
  - `ServiceBusBackgroundService.ExceptionHandling.cs` (tratamento de falhas)
  - `ServiceBusBackgroundService.DeadLetter.cs` (DLQ/requeue)
  - `ServiceBusBackgroundService.Lifecycle.cs` (ciclo de vida e descarte)

### Observacoes residuais
- Permanecem warnings nao bloqueantes fora do escopo funcional principal.
- O comportamento de excecoes nao mapeadas no `HandleMessageAsync` foi mantido conforme implementacao atual (propagacao).

## Foco Principal
Redesenhar o ciclo de vida de recursos do pacote `Nuuvify.CommonPack.BackgroundService` para descarte assincrono idempotente, garantindo que todas as funcionalidades que dependem de liberacao de recursos estejam implementadas de forma correta, previsivel e segura.

Este foco e o eixo central do plano. A remocao de `SuppressMessage` e consequencia da implementacao correta desse ciclo de vida.

## Objetivos Derivados
1. Garantir descarte sem bloqueio async->sync e sem risco de deadlock.
2. Garantir idempotencia de dispose em chamadas repetidas e em ordens diferentes (`StopAsync` antes/depois de `Dispose`).
3. Garantir ordem deterministica de liberacao de recursos do Service Bus.
4. Preservar semantica funcional (ack/nack/retry/dead-letter) durante e apos o redesenho.
5. Eliminar a necessidade de suppressao de `CA2213` pela correcao de design.

## Escopo
- Pacote alvo: `src/Nuuvify.CommonPack.BackgroundService`
- Consumidor de referencia para validacao: `TemplateDotnetWorker`
- Inclui remocao de:
  - `CA2213` nos campos IDisposable da classe base
  - `CA1031` em `GlobalSuppressions.cs`

## Arquivos Prioritarios
- `src/Nuuvify.CommonPack.BackgroundService/Services/ServiceBusBackgroundService.cs`
- `src/Nuuvify.CommonPack.BackgroundService/Services/ServiceBusBackgroundService.ExceptionHandling.cs`
- `src/Nuuvify.CommonPack.BackgroundService/Services/ServiceBusBackgroundService.DeadLetter.cs`
- `src/Nuuvify.CommonPack.BackgroundService/GlobalSuppressions.cs`
- `test/Nuuvify.CommonPack.BackgroundService.xTest/ServiceBusBackgroundServiceBaseTests.cs`
- `test/Nuuvify.CommonPack.BackgroundService.xTest/ServiceBusBackgroundServiceReceiveModeTests.cs`
- `test/Nuuvify.CommonPack.BackgroundService.xTest/ServiceBusBackgroundServiceTests.cs`

## Fases de Implementacao

### Fase 1 - Baseline de lifecycle e contrato observavel
1. Mapear e congelar comportamento atual de lifecycle e settlement:
   - Sucesso -> Complete
   - Falha de negocio -> Abandon ou DeadLetter (conforme flag)
   - ReceiveAndDelete -> sem settlement manual
2. Definir matriz de comportamento para cancelamento, lock loss, communication problem e falhas genericas.
3. Levantar cobertura de testes existente e lacunas de descarte.

### Fase 2 - Redesenho do ciclo de vida (Fase Principal)
1. Definir modelo de descarte assincrono idempotente para todos os recursos descartaveis.
2. Eliminar qualquer caminho de conversao async->sync no dispose.
3. Implementar protecao contra dupla liberacao e corrida de encerramento.
4. Garantir ordem deterministica de liberacao:
   - processor principal
   - dead-letter processor
   - sender
   - client
   - ActivitySource
5. Garantir consistencia entre `StopAsync`, `Dispose` e eventual `DisposeAsync`.
6. Validar que o redesenho nao altera contrato funcional de processamento.

### Fase 3 - Remocao de suppressions como consequencia do redesenho
1. Remover os 4 atributos `SuppressMessage` de `CA2213`.
2. Reavaliar e remover a entrada de `CA1031` em `GlobalSuppressions.cs` com tratamento explicito adequado.
3. Para excecoes nao mapeadas, manter fluxo observavel e diagnostico coerente com o contrato definido.

### Fase 4 - Robustez funcional apos redesenho
1. Revalidar semantica de `AbandonMessageIfFailed`.
2. Revalidar comportamento de `ReceiveAndDelete`.
3. Revalidar fluxo de DLQ e requeue para origem.
4. Revalidar cancelamento e encerramento gracioso com mensagens em voo.

### Fase 5 - Validacao no consumidor (TemplateDotnetWorker)
1. Validar contrato no worker derivado.
2. Validar padrao singleton + escopo por mensagem durante shutdown.
3. Confirmar ausencia de regressao em ack/nack/retry.

### Fase 6 - Fechamento
1. Build e testes do pacote BackgroundService.
2. Busca textual para confirmar zero ocorrencias de `SuppressMessage` no pacote.
3. Atualizar documentacao de pacote/changelog se existir mudanca observavel para consumidores.

## Testes Minimos Recomendados (foco em descarte)
- `StopAsync_ThenDispose_ShouldNotThrow_SequentialCalls`
- `Dispose_WhenCalledBeforeStopAsync_ShouldNotThrow`
- `Dispose_ShouldBeIdempotent_WhenCalledMultipleTimes`
- `StopAsync_ShouldDisposeResources_InDeterministicOrder`
- `Dispose_ShouldNotUseAsyncToSyncBlocking`
- `HandleMessageAsync_WhenReceiveAndDelete_ShouldNotSettle`
- `HandleMessageAsync_WhenBusinessFailure_ShouldRespectAbandonFlag`
- `HandleMessageAsync_WhenLockLost_ShouldRouteToExpectedFailurePath`
- `DeadLetterProcessing_WhenRequeueToOrigin_ShouldPreserveDiagnostics`

## Criterios de Aceite
1. O ciclo de vida de descarte assincrono idempotente esta implementado e validado por testes.
2. Nenhuma ocorrencia de `SuppressMessage` em `src/Nuuvify.CommonPack.BackgroundService`.
3. Sem regressao de semantica de settlement para cenarios cobertos.
4. Sem regressao de shutdown/cancelamento com mensagens em processamento.
5. Testes do pacote aprovados.
6. Validacao de compatibilidade no TemplateDotnetWorker concluida.

## Riscos e Mitigacoes
- Risco: mudanca observavel no tratamento de excecoes genericas.
  - Mitigacao: documentar claramente e validar comportamento no consumidor de referencia.
- Risco: regressao no ciclo de vida de recursos (dispose).
  - Mitigacao: testes dedicados de lifecycle, idempotencia e ordem de liberacao.
- Risco: alteracao em lock renewal e retries implicitos.
  - Mitigacao: testes de timeout/lock loss e revisao de configuracao de processor options.

## Observacoes de Versionamento
- Se houver alteracao observavel do contrato para consumidores externos, avaliar bump semantico apropriado e registrar guia de migracao.
