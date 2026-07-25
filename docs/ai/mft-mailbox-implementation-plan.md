# Plano de Implementação - DLL .NET Abstraída para MFT Mailbox

## 1. Objetivo

Implementar uma DLL reutilizável no ecossistema Nuuvify para integração com MFT Mailbox, com foco em:

- abstração por interface .NET para clientes consumidores
- suporte a SFTP e HTTPS Mailbox
- fluxo bidirecional (outbound e inbound)
- suporte a ACK/NACK por metadado e status
- processamento de arquivos grandes e múltiplos arquivos por lote

Meta de capacidade inicial:

- até 1 GB por arquivo
- até 500 arquivos por execução

## 2. Princípios de Arquitetura

- Ports and Adapters (Hexagonal): contratos no core e adapters por protocolo
- SRP e baixo acoplamento: cada componente com responsabilidade única
- Fail-safe e resiliência: retry, timeout, circuit breaker e idempotência
- Streaming-first: evitar carregar arquivos inteiros em memória
- Observabilidade fim a fim: correlationId, métricas e trilha de auditoria
- Security by default: segredos externos, logs mascarados e validação de integridade

## 3. Escopo Funcional da Primeira Versão

### 3.1 Protocolos

- SFTP
- HTTPS Mailbox API

### 3.2 Casos de uso

- envio de arquivo único
- envio em lote
- recebimento de arquivo único
- recebimento em lote
- consulta de status de transferência
- confirmação ACK/NACK
- arquivamento de sucesso e falha

### 3.3 Não escopo inicial

- transformação de conteúdo de payload
- compressão e criptografia de payload com formato proprietário
- UI de monitoramento

## 4. Arquitetura Alvo da DLL

## 4.1 Pacotes/projetos sugeridos

- Nuuvify.CommonPack.MftMailbox.Abstraction
- Nuuvify.CommonPack.MftMailbox
- Nuuvify.CommonPack.MftMailbox.Sftp
- Nuuvify.CommonPack.MftMailbox.Http
- Nuuvify.CommonPack.MftMailbox.Tests

## 4.2 Contratos principais (Abstraction)

- IMftTransferClient
- IMftInboundClient
- IMftStatusClient
- IAckNackClient
- IMftClientFactory
- IMftIdempotencyStore
- ITransferAuditSink

## 4.3 Modelos principais

- TransferEnvelope
- TransferItem
- TransferBatchResult
- TransferItemResult
- TransferStatus
- AckNackCommand

## 4.4 Estratégia de seleção de adapter

- configuração define protocolo por integração
- factory resolve adapter correto (SFTP/HTTPS)
- clientes consumidores usam somente interfaces da camada Abstraction

## 5. Fluxos Operacionais

## 5.1 Outbound (envio)

1. validar metadados e regras de nome
2. gerar idempotency key
3. calcular checksum (sha-256)
4. upload por streaming
5. commit atômico (tmp + rename para SFTP quando aplicável)
6. registrar auditoria
7. registrar status
8. emitir resultado por item e consolidado por lote

## 5.2 Inbound (recebimento)

1. descobrir arquivos/pedidos pendentes
2. lock lógico por item
3. download por streaming
4. validar integridade (checksum/tamanho)
5. devolver stream ao consumidor
6. confirmar ACK/NACK
7. mover para sucesso/erro conforme política

## 5.3 ACK/NACK

- estratégia pluggable para diferentes implementações de MFT
- suporte a ACK/NACK por API, metadata ou arquivo marcador
- modo síncrono e assíncrono (polling com backoff)

## 6. Requisitos Não Funcionais

- sem leitura integral de arquivo em memória por padrão
- limite de concorrência configurável
- timeout por arquivo e timeout por lote
- retry exponencial com jitter para falhas transientes
- circuit breaker para indisponibilidade externa
- cancelamento por CancellationToken em toda cadeia
- logs estruturados sem vazamento de segredo ou payload sensível

## 7. Segurança

### 7.1 SFTP

- autenticação por chave (preferencial)
- host key pinning obrigatório
- pasta de drop segregada por integração

### 7.2 HTTPS

- autenticação forte (token/certificado conforme ambiente)
- opção de mTLS
- validação rígida de TLS/certificados

### 7.3 Segredos e dados

- segredos em provider seguro (não em código)
- mascaramento de dados sensíveis em logs
- checksum para verificação de integridade ponta a ponta

## 8. Estratégia de Testes

## 8.1 Unitários

- contratos e validações
- idempotência
- retry, timeout e circuit breaker
- ACK/NACK

## 8.2 Integração

- adapter SFTP com cenários de upload/download/rename
- adapter HTTPS com cenários de upload/download/status
- lote com múltiplos arquivos

## 8.3 Carga e robustez

- arquivos até 1 GB
- lotes até 500 arquivos
- medição de memória, throughput e erro por tipo
- falhas induzidas (rede, timeout, indisponibilidade)

## 9. Plano por Fases

## Fase 0 - Discovery e contrato técnico

- consolidar DSL de metadados
- definir modelo de ACK/NACK alvo
- definir matriz de erro funcional/transiente

Entregáveis:

- ADR da arquitetura
- contrato de interfaces e modelos

## Fase 1 - Núcleo Abstraction + domínio de transferência

- criar interfaces e modelos base
- criar validações e pipeline interno de execução

Entregáveis:

- pacote Abstraction
- testes unitários de contratos

## Fase 2 - Adapter SFTP

- implementar upload/download streaming
- commit atômico via tmp + rename
- validação de host key

Entregáveis:

- adapter SFTP funcional
- testes de integração SFTP

## Fase 3 - Adapter HTTPS Mailbox

- implementar upload/download/status
- implementar autenticação configurável
- implementar polling de status com backoff

Entregáveis:

- adapter HTTPS funcional
- testes de integração HTTPS

## Fase 4 - ACK/NACK e idempotência

- consolidar estratégia de confirmação
- idempotência por chave determinística
- trilha de auditoria por item/lote

Entregáveis:

- componentes de ACK/NACK e idempotência
- testes de regressão

## Fase 5 - Hardening e publicação

- testes de carga e resiliência
- tuning de limites e defaults
- empacotar e publicar NuGet interno

Entregáveis:

- pacote pronto para consumo
- guia de onboarding para times clientes

## 10. Critérios de Pronto

- interfaces estáveis para clientes
- suporte validado a SFTP e HTTPS
- fluxo bidirecional com ACK/NACK validado
- execução de lote grande dentro dos limites de memória definidos
- cobertura de testes adequada para cenários críticos
- pacote NuGet interno publicado e documentado

## 11. Riscos e Mitigações

- variação de comportamento entre provedores MFT
  - mitigação: strategy por protocolo e contrato mínimo comum
- instabilidade de rede em lote grande
  - mitigação: retry com jitter, timeout e idempotência
- regressão de performance com arquivos grandes
  - mitigação: streaming obrigatório e testes de carga contínuos

## 12. Próximos Passos Imediatos

1. criar ADR de arquitetura e assinatura das interfaces da camada Abstraction
2. definir contrato oficial de ACK/NACK com o time de integração
3. iniciar Fase 1 com pacote Abstraction e suite base de testes
