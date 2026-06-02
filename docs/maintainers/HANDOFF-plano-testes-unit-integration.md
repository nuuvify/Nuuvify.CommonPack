# Handoff - Plano de continuidade (Unit x Integration)

Data: 2026-06-02
Contexto: segregacao e estabilizacao da execucao de testes Unit e Integration no repositorio Nuuvify.CommonPack.

## Objetivo da frente
- Classificar corretamente os testes como Unit ou Integration.
- Garantir que testes Unit nao dependam de recursos externos.
- Definir e validar fluxo para execucao de Integration com prerequisitos.
- Estabilizar execucao de Unit sem falhas de infraestrutura.

## Status atual
### Concluido
- Revisao geral de testes para classificacao Unit/Integration.
- Inclusao/ajuste de categorizacao via `Trait("Category", "Unit")` e `Trait("Category", "Integration")` nos projetos auditados.
- Atualizacao do script principal de testes para suportar filtro por categoria:
  - Arquivo: `test/run-tests.ps1`
  - Parametro com suporte a `All`, `Unit`, `Integration`.
- Ajuste de 3 testes em BackgroundService para alinhar com contrato atual em ReceiveAndDelete:
  - `ShouldThrow...` alterado para validacao de `Should.NotThrowAsync(...)`.
  - Arquivo: `test/Nuuvify.CommonPack.BackgroundService.xTest/ServiceBusBackgroundServiceReceiveModeTests.cs`
- Preservacao do arquivo solicitado pelo usuario sem alteracoes:
  - `test/Test-UnitExecute.ps1`

### Validado
- Teste focal de BackgroundService (ReceiveMode tests) aprovado apos ajuste.
- Categorizacao de testes ativos sem pendencias na checagem realizada anteriormente.

## Problema em aberto
A execucao ampla com `--filter "Category=Unit"` apresentou `exit code 1` em algumas tentativas, mas por conflito de infraestrutura (arquivo bloqueado), nao por falha de assert de teste.

Erros observados:
- `MSB3021` / `MSB3027` (copy failed por arquivo em uso).
- `MSB4018` (GenerateMvcTestManifestTask com arquivo em uso).

Projetos mais impactados por lock:
- `test/Nuuvify.CommonPack.AzureServiceBus.xTest/`
- `test/Nuuvify.CommonPack.UnitOfWork.InMemory.xTest/`
- Tambem houve lock em artefatos de `UnitOfWork.Integration.xTest` durante execucoes concorrentes.

## Plano de continuidade (proxima sessao)
1. Encerrar processos residuais antes de testar:
   - `testhost`
   - `dotnet` orphan (se houver)
2. Limpar artefatos de build/teste:
   - `dotnet clean Nuuvify.CommonPack.sln`
   - opcional: task `remove-obj`
3. Executar Unit de forma serial para reduzir lock:
   - `dotnet test Nuuvify.CommonPack.sln --filter "Category=Unit" -v minimal -m:1`
4. Se ainda houver lock, executar por projeto em sequencia:
   - Iterar todos os `test/*.csproj` com `dotnet test <proj> --filter "Category=Unit" -m:1`
   - Consolidar projetos com falha de infraestrutura separadamente de falha de teste.
5. Registrar evidencias finais:
   - Lista de projetos Unit aprovados.
   - Lista de projetos sem testes Unit (mensagem "Nenhum teste corresponde ao filtro" e esperado).
   - Lista de falhas reais (se existir alguma apos serializacao).

## Definicao de pronto para essa frente
- Execucao Unit final com status estavel (sem falhas de assert e sem lock de infraestrutura).
- Script e categorizacao mantidos funcionando para `All`, `Unit`, `Integration`.
- Documento de prerequisitos de Integration consolidado para execucao local (Docker, recursos externos e credenciais, conforme projeto).

## Comandos uteis (recuperacao rapida)
### Unit (serial)
`dotnet test Nuuvify.CommonPack.sln --filter "Category=Unit" -v minimal -m:1`

### Integration (quando ambiente estiver pronto)
`dotnet test Nuuvify.CommonPack.sln --filter "Category=Integration" -v minimal -m:1`

### Build
`dotnet build Nuuvify.CommonPack.sln`

### Script de apoio
`pwsh ./test/run-tests.ps1 -TestCategory Unit`

## Observacoes para quem assumir
- O ajuste funcional em BackgroundService ja foi feito e validado no escopo focal.
- O principal ponto pendente e estabilidade de execucao agregada (concorrencia/locks), nao comportamento funcional.
- Nao alterar `test/Test-UnitExecute.ps1`.
