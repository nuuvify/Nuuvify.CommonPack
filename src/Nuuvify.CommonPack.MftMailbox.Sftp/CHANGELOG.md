# Changelog - Nuuvify.CommonPack.MftMailbox.Sftp

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Alterado

- Dependências atualizadas na linha compatível com .NET 8: `Microsoft.Extensions.Logging.Abstractions` 8.0.3.

## [2.9.0] - 2026-08-26

### Adicionado
- Adapter SFTP com upload/download streaming, commit atomico e ACK/NACK.

### Alterado
- ACK/NACK agora suporta modos configuráveis: sem ação, metadados, arquivo marcador e modo combinado.
- Conteúdo do arquivo marcador ACK/NACK agora permite configuração de encoding (incluindo code pages usadas em integração com mainframe).
- Recepção inbound por listagem automática agora respeita ordenação configurável por nome de arquivo.
