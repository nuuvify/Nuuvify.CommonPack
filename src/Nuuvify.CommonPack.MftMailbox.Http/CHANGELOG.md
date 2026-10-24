# Changelog - Nuuvify.CommonPack.MftMailbox.Http

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Alterado

- Dependências atualizadas na linha compatível com .NET 8: `Microsoft.Extensions.Logging.Abstractions` 8.0.3.

## [2.9.0] - 2026-08-26

### Adicionado
- Adapter HTTPS Mailbox com envio/recebimento streaming, status e ACK/NACK.

### Alterado
- Adicionada opção de ordenação dos itens inbound retornados pelo endpoint de listagem.
- Upload multipart agora pode incluir metadados configuráveis de envelope e item.

### Corrigido
- ReceiveBatchAsync passou a respeitar itens explicitamente informados no envelope, conforme contrato da abstração inbound.
