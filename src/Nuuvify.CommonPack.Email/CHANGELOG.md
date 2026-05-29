# Changelog - Nuuvify.CommonPack.Email

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

## [Sem versão registrada] - 2025-10-08

### Adicionado
- Release inicial do `Nuuvify.CommonPack.Email`.
- Envio SMTP assíncrono via MailKit com suporte a `CancellationToken`.
- Suporte a SSL/TLS configurável (None, SSL2, SSL3, TLS, TLS11, TLS12, TLS13).
- Sistema de templates HTML com substituição de variáveis por placeholders `{{VARIAVEL}}`.
- Suporte a múltiplos destinatários por tipo: To, Cc, Bcc via enum `PessoasEmail`.
- Suporte a múltiplos remetentes.
- Anexos via caminho de arquivo e via `Stream`, com suporte a tipos MIME (Text, Image, Application).
- Padrão de notificação para rastreamento de erros e avisos sem exceção.
- Método `ResetMailInstance()` para reutilização de instância em loops de envio em lote.
- Configuração via `IOptions<EmailServerConfiguration>` e integração com DI.


