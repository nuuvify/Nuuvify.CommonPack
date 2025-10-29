# Changelog - Nuuvify.CommonPack.Email

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- [ ] Suporte a templates Razor para e-mails dinâmicos
- [ ] Rate limiting para envio em lote
- [ ] Queue system para processamento assíncrono
- [ ] Métricas e telemetria de envio
- [ ] Suporte a provedores alternativos (SendGrid SDK, AWS SES SDK)
- [ ] Internacionalização (i18n) de templates
- [ ] Editor visual de templates
- [ ] Webhooks para eventos de e-mail (entregue, aberto, clicado)
- [ ] Suporte a imagens inline (embedded images)
- [ ] Validação avançada de e-mails

---

## [1.0.0] - Current Version

### Features

#### Core Email Functionality
- ✅ **Envio SMTP via MailKit**: Implementação robusta usando MailKit para máxima compatibilidade
- ✅ **Async/Await**: Suporte completo a operações assíncronas com `EnviarAsync()`
- ✅ **CancellationToken**: Permite cancelamento de operações de envio
- ✅ **SSL/TLS Support**: Protocolos de segurança configuráveis (None, SSL2, SSL3, TLS, TLS11, TLS12, TLS13)
- ✅ **Autenticação SMTP**: Suporte a autenticação com username/password
- ✅ **Encoding UTF-8**: Suporte completo a caracteres especiais e acentuação

#### Template System
- ✅ **HTML Templates**: Processamento de templates HTML com substituição de variáveis
- ✅ **Variable Substitution**: Sistema de placeholders `{{VARIAVEL}}` para personalização
- ✅ **File-based Templates**: Suporte a arquivos `.html` externos
- ✅ **Dynamic Content**: Geração dinâmica de conteúdo via `GetEmailTemplate()`

#### Attachment Support
- ✅ **File Attachments**: Anexar arquivos via caminho completo
- ✅ **Stream Attachments**: Anexar arquivos via `Stream` para cenários dinâmicos
- ✅ **Multiple MIME Types**: Suporte a Text, Image, Application
- ✅ **Multiple Subtypes**: Text, GIF, JPG, PNG, PDF
- ✅ **Attachment Management**: Métodos para adicionar, contar e remover anexos
- ✅ **Fluent API**: `WithAttachment()` com retorno de `IEmail` para chaining

#### Recipient Management
- ✅ **Multiple Recipients**: Suporte a múltiplos destinatários via `Dictionary<string, string>`
- ✅ **Recipient Types**: To, Cc, Bcc (via enum `PessoasEmail`)
- ✅ **Multiple Senders**: Suporte a múltiplos remetentes
- ✅ **Email Personalization**: Personalização individual por destinatário

#### Configuration
- ✅ **appsettings.json Integration**: Configuração via Options Pattern
- ✅ **EmailServerConfiguration**: Modelo tipado para configuração SMTP
- ✅ **Dependency Injection**: Registro via `IServiceCollection`
- ✅ **Environment-based Config**: Suporte a múltiplos ambientes (Development, Production, etc.)

#### Error Handling & Logging
- ✅ **Notification Pattern**: Sistema de notificações para erros de validação
- ✅ **LogMessage Collection**: Mensagens informativas separadas de erros
- ✅ **IsValid() Method**: Verificação de validação após operações
- ✅ **Detailed Error Messages**: Mensagens descritivas com propriedade e contexto
- ✅ **DateTime Tracking**: Timestamp em cada notificação

#### Instance Management
- ✅ **ResetMailInstance()**: Limpeza de instância para reutilização em loops
- ✅ **RemoveAllAttachments()**: Limpeza de anexos
- ✅ **RemoveAllNotifications()**: Limpeza de notificações de erro
- ✅ **RemoveAllLogMessage()**: Limpeza de mensagens de log

### Technical Details

#### Target Framework
- ✅ **.NET Standard 2.1**: Compatível com .NET Core 3.0+, .NET 5+, .NET 6+, .NET 8+, .NET Framework 4.7.2+

#### Dependencies
| Package                                                  | Purpose                         |
| -------------------------------------------------------- | ------------------------------- |
| **MailKit**                                              | Cliente SMTP completo e robusto |
| **Microsoft.Extensions.Options.ConfigurationExtensions** | Suporte ao Options Pattern      |
| **Nuuvify.CommonPack.Email.Abstraction**                 | Interfaces e contratos          |

#### Design Patterns
- ✅ **Repository Pattern**: Abstração via `IEmail` interface
- ✅ **Options Pattern**: Configuração via `IOptions<EmailServerConfiguration>`
- ✅ **Notification Pattern**: Rastreamento de erros e avisos
- ✅ **Fluent Interface**: Métodos encadeáveis para melhor DX
- ✅ **Dependency Injection**: Integração completa com DI container

### API Surface

#### IEmail Interface Methods
```csharp
Task<object> EnviarAsync(
    Dictionary<string, string> recipients,
    Dictionary<string, string> senders,
    string subject,
    string message,
    CancellationToken cancellationToken = default)
```

```csharp
string GetEmailTemplate(
    Dictionary<string, string> variables,
    string templateFullName)
```

```csharp
IEmail WithAttachment(
    string pathFileFullName,
    EmailMidiaType midiaType,
    EmailMidiaSubType midiaSubType)
```

```csharp
IEmail WithAttachment(
    Stream fileStream,
    EmailMidiaType midiaType,
    EmailMidiaSubType midiaSubType,
    string fullFileName)
```

```csharp
int CountAttachments()
bool IsValid()
IEmail RemoveAllAttachments()
IEmail RemoveAllLogMessage()
IEmail RemoveAllNotifications()
void ResetMailInstance()
```

#### Properties
```csharp
List<NotificationR> Notifications { get; set; }
List<NotificationR> LogMessage { get; set; }
EmailServerConfiguration EmailServerConfiguration { get; set; }
```

### Use Cases

#### 1. Transactional Emails
- ✅ Confirmação de cadastro
- ✅ Reset de senha
- ✅ Confirmação de pedidos
- ✅ Notificações de status

#### 2. Marketing Emails
- ✅ Newsletters
- ✅ Campanhas promocionais
- ✅ Comunicados importantes
- ✅ E-mails segmentados

#### 3. System Notifications
- ✅ Alertas de sistema
- ✅ Relatórios agendados
- ✅ Notificações de erro
- ✅ Logs de auditoria

#### 4. Bulk Emails
- ✅ Envio em lote com `ResetMailInstance()`
- ✅ Personalização individual
- ✅ Retry logic
- ✅ Error tracking

---

## Roadmap

### Version 1.1.0 - Planned

#### Enhanced Template Engine
- [ ] **Razor Template Support**: Integração com Razor Engine para templates dinâmicos
- [ ] **Template Caching**: Cache de templates compilados para melhor performance
- [ ] **Template Inheritance**: Suporte a layouts base e herança
- [ ] **Conditional Rendering**: Suporte a if/else dentro de templates

#### Advanced Features
- [ ] **Email Queue**: Sistema de fila para processamento assíncrono
- [ ] **Priority Levels**: Níveis de prioridade para envio (High, Normal, Low)
- [ ] **Scheduled Sending**: Agendamento de envios para data/hora específica
- [ ] **Bulk Send Optimization**: Otimizações para envio massivo

#### Monitoring & Telemetry
- [ ] **Send Metrics**: Métricas de envio (success rate, avg time, etc.)
- [ ] **OpenTelemetry**: Integração com OpenTelemetry para tracing
- [ ] **Health Checks**: Health check endpoint para validar SMTP
- [ ] **Event Callbacks**: Callbacks para eventos (enviado, erro, etc.)

### Version 1.2.0 - Planned

#### Provider Abstraction
- [ ] **SendGrid SDK**: Suporte nativo ao SDK SendGrid
- [ ] **AWS SES SDK**: Suporte nativo ao SDK Amazon SES
- [ ] **Azure Communication Services**: Integração com Azure
- [ ] **Provider Fallback**: Fallback automático entre provedores

#### Advanced Attachments
- [ ] **Inline Images**: Suporte a imagens embutidas no HTML
- [ ] **Cloud Attachments**: Anexar arquivos de Azure Blob, AWS S3
- [ ] **Attachment Compression**: Compressão automática de anexos grandes
- [ ] **Attachment Encryption**: Criptografia de anexos sensíveis

#### Validation & Security
- [ ] **Email Validation**: Validação avançada de e-mails (syntax, MX records)
- [ ] **Domain Verification**: Verificação de domínio (SPF, DKIM, DMARC)
- [ ] **Rate Limiting**: Limitação de taxa de envio
- [ ] **Spam Score Check**: Verificação de score de spam antes do envio

### Version 2.0.0 - Future

#### Breaking Changes
- [ ] **Async-only API**: Remover métodos síncronos (breaking change)
- [ ] **Fluent Configuration**: API fluente para configuração
- [ ] **Strong-typed Templates**: Templates tipados com compile-time checking

#### Enterprise Features
- [ ] **Multi-tenant Support**: Suporte a múltiplos tenants
- [ ] **Email Analytics**: Dashboard de analytics de e-mails
- [ ] **A/B Testing**: Testes A/B de templates e conteúdo
- [ ] **Webhook Integration**: Webhooks para eventos (aberto, clicado, bounce)

#### Performance
- [ ] **Connection Pooling**: Pool de conexões SMTP
- [ ] **Batch Processing**: Processamento em lote otimizado
- [ ] **Memory Optimization**: Otimizações de uso de memória
- [ ] **Async Streaming**: Streaming assíncrono de anexos grandes

---

## Migration Guides

### Migrating from System.Net.Mail

Se você está migrando de `System.Net.Mail` (obsoleto), aqui está como adaptar:

#### Before (System.Net.Mail)
```csharp
var mail = new MailMessage();
mail.From = new MailAddress("from@email.com");
mail.To.Add("to@email.com");
mail.Subject = "Subject";
mail.Body = "Body";
mail.IsBodyHtml = true;

var smtp = new SmtpClient("smtp.server.com", 587);
smtp.Credentials = new NetworkCredential("user", "pass");
smtp.EnableSsl = true;
smtp.Send(mail);
```

#### After (Nuuvify.CommonPack.Email)
```csharp
var recipients = new Dictionary<string, string>
{
    { "to@email.com", "Recipient Name" }
};

var senders = new Dictionary<string, string>
{
    { "from@email.com", "Sender Name" }
};

await _emailService.EnviarAsync(
    recipients: recipients,
    senders: senders,
    subject: "Subject",
    message: "Body" // HTML automático
);
```

---

## Known Issues

### Current Limitations

1. **Single SMTP Server**: Apenas um servidor SMTP por instância
   - **Workaround**: Use múltiplas instâncias com diferentes configurações

2. **Template Engine**: Sistema simples de substituição de strings
   - **Workaround**: Use Razor externamente ou aguarde v1.1.0

3. **No Built-in Retry**: Sem retry automático em caso de falha
   - **Workaround**: Implemente retry logic manualmente (exemplo no README)

4. **Inline Images**: Sem suporte nativo a imagens inline
   - **Workaround**: Use imagens hospedadas externamente ou aguarde v1.2.0

5. **No Email Tracking**: Sem tracking de abertura/clique
   - **Workaround**: Use webhook provider (SendGrid, etc.) ou aguarde v2.0.0

---

## Breaking Changes

### None (Version 1.0.0)

This is the initial stable release. Future versions will document breaking changes here.

---

## Contributing

Para contribuir com o projeto:

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'feat: add some amazing feature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

### Contribution Guidelines

- Siga os padrões de código do projeto (.editorconfig)
- Escreva testes para novas funcionalidades
- Atualize a documentação (README, CHANGELOG)
- Use Conventional Commits para mensagens de commit
- Certifique-se que todos os testes passam

---

## License

Este projeto está licenciado sob a [MIT License](../../LICENSE).

---

## Support & Community

- 🐛 **Issues**: [GitHub Issues](https://github.com/nuuvify/CommonPack/issues)
- 📧 **Email**: [suporte@zocate.li](mailto:suporte@zocate.li)
- 📖 **Documentation**: [Wiki do Projeto](https://github.com/nuuvify/CommonPack/wiki)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/nuuvify/CommonPack/discussions)

---

**Nuuvify CommonPack.Email** - Simplificando o envio de e-mails em .NET 📧
