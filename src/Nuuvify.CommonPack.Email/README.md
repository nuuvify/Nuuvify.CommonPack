# Nuuvify.CommonPack.Email

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)

Implementação robusta e completa de serviço de envio de e-mails usando **MailKit**. Fornece uma abstração de alto nível para envio de e-mails com suporte a templates HTML, anexos, múltiplos destinatários e configuração flexível via appsettings.json.

## 📋 Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Dependências](#dependências)
- [Configuração](#configuração)
- [Uso](#uso)
  - [Envio Básico](#envio-básico)
  - [Templates HTML](#templates-html)
  - [Anexos](#anexos)
  - [Múltiplos Destinatários](#múltiplos-destinatários)
  - [Notificações e Logging](#notificações-e-logging)
- [Exemplos Práticos](#exemplos-práticos)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

## Funcionalidades

### 🎯 Core Features
- ✅ **Envio de E-mails** via SMTP usando MailKit
- ✅ **Templates HTML** com substituição de variáveis
- ✅ **Anexos** (documentos, imagens, PDFs)
- ✅ **Múltiplos Destinatários** (To, Cc, Bcc)
- ✅ **Configuração via appsettings.json** para diferentes ambientes
- ✅ **Notification Pattern** para rastreamento de erros
- ✅ **Suporte a SSL/TLS** para comunicação segura
- ✅ **Async/Await** para operações não bloqueantes

### 🔒 Security Features
- ✅ **SSL/TLS Encryption** (None, SSL, TLS)
- ✅ **Autenticação SMTP** com usuário e senha
- ✅ **Validação de configurações** antes do envio
- ✅ **Proteção de credenciais** via configuration

### 📧 Email Features
- ✅ **HTML e Text** support
- ✅ **Inline Images** para templates
- ✅ **Attachments** com múltiplos tipos MIME
- ✅ **Encoding UTF-8** automático
- ✅ **Template Engine** simples e eficaz
- ✅ **Fluent API** para construção de e-mails

### 🛠️ Advanced Features
- ✅ **Stream Support** para anexos dinâmicos
- ✅ **Reset Instance** para envio em lote
- ✅ **Log Messages** separado de notificações
- ✅ **Validation** automática de configurações
- ✅ **Cancellation Token** support
- ✅ **Compatibilidade .NET Standard 2.1**

## Instalação

### Via Package Manager Console
```powershell
Install-Package Nuuvify.CommonPack.Email
Install-Package Nuuvify.CommonPack.Email.Abstraction
```

### Via .NET CLI
```bash
dotnet add package Nuuvify.CommonPack.Email
dotnet add package Nuuvify.CommonPack.Email.Abstraction
```

### Via PackageReference
```xml
<PackageReference Include="Nuuvify.CommonPack.Email" Version="X.X.X" />
<PackageReference Include="Nuuvify.CommonPack.Email.Abstraction" Version="X.X.X" />
```

## Dependências

### NuGet Packages

| Package                                                  | Version | Descrição                                      |
| -------------------------------------------------------- | ------- | ---------------------------------------------- |
| **MailKit**                                              | Latest  | Biblioteca completa para envio de e-mails SMTP |
| **Microsoft.Extensions.Options.ConfigurationExtensions** | Latest  | Suporte ao padrão Options para configuração    |
| **Nuuvify.CommonPack.Email.Abstraction**                 | -       | Interfaces e abstrações do serviço de e-mail   |

### Framework

- **.NET Standard 2.1**: Compatível com .NET Core 3.0+, .NET 5+, .NET 6+, .NET 8+ e .NET Framework 4.7.2+

## Configuração

### 1. Configurar appsettings.json

```json
{
  "EmailConfig": {
    "EmailServerConfiguration": {
      "ServerHost": "smtp.gmail.com",
      "Port": 587,
      "Security": "StartTls",
      "AccountUserName": "seu-email@gmail.com",
      "AccountPassword": "sua-senha-ou-app-password"
    },
    "RemetenteEmail": {
      "email1": "noreply@empresa.com",
      "nome1": "Empresa XYZ",
      "email2": "suporte@empresa.com",
      "nome2": "Suporte Técnico"
    },
    "DestinatariosEmail": {
      "admin": "admin@empresa.com",
      "suporte": "suporte@empresa.com"
    }
  }
}
```

### 2. Registrar Serviços (Dependency Injection)

```csharp
// Program.cs (.NET 6+)
using Nuuvify.CommonPack.Email;
using Nuuvify.CommonPack.Email.Abstraction;

var builder = WebApplication.CreateBuilder(args);

// Registrar serviço de e-mail com configuração
builder.Services.AddScoped<IEmail, Email>();
builder.Services.Configure<EmailServerConfiguration>(
    builder.Configuration.GetSection("EmailConfig:EmailServerConfiguration"));

var app = builder.Build();
app.Run();
```

```csharp
// Startup.cs (.NET Core 3.1 / .NET 5)
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Registrar serviço de e-mail
        services.AddScoped<IEmail, Email>();

        // Configurar servidor de e-mail via Options Pattern
        services.Configure<EmailServerConfiguration>(
            Configuration.GetSection("EmailConfig:EmailServerConfiguration"));
    }
}
```

### 3. Configurações de Segurança SSL/TLS

```csharp
using System.Security.Authentication;

// Opções de segurança disponíveis:
public enum SslProtocols
{
    None = 0,           // Sem criptografia (não recomendado)
    Ssl2 = 12,          // SSL 2.0 (obsoleto)
    Ssl3 = 48,          // SSL 3.0 (obsoleto)
    Tls = 192,          // TLS 1.0
    Tls11 = 768,        // TLS 1.1
    Tls12 = 3072,       // TLS 1.2 (recomendado)
    Tls13 = 12288       // TLS 1.3 (mais recente)
}
```

### Configurações para Provedores Comuns

#### Gmail
```json
{
  "ServerHost": "smtp.gmail.com",
  "Port": 587,
  "Security": "StartTls",
  "AccountUserName": "seu-email@gmail.com",
  "AccountPassword": "app-password-gerado-no-google"
}
```
⚠️ **Importante**: Para Gmail, use [App Passwords](https://support.google.com/accounts/answer/185833) ao invés da senha normal.

#### Outlook/Office 365
```json
{
  "ServerHost": "smtp-mail.outlook.com",
  "Port": 587,
  "Security": "StartTls",
  "AccountUserName": "seu-email@outlook.com",
  "AccountPassword": "sua-senha"
}
```

#### SendGrid
```json
{
  "ServerHost": "smtp.sendgrid.net",
  "Port": 587,
  "Security": "StartTls",
  "AccountUserName": "apikey",
  "AccountPassword": "SG.sua-api-key-aqui"
}
```

#### Amazon SES
```json
{
  "ServerHost": "email-smtp.us-east-1.amazonaws.com",
  "Port": 587,
  "Security": "StartTls",
  "AccountUserName": "suas-credenciais-smtp",
  "AccountPassword": "sua-senha-smtp"
}
```

## Uso

### Envio Básico

```csharp
using Nuuvify.CommonPack.Email.Abstraction;

public class NotificationService
{
    private readonly IEmail _emailService;
    private readonly IConfiguration _configuration;

    public NotificationService(IEmail emailService, IConfiguration configuration)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task EnviarEmailSimples()
    {
        // Preparar destinatários
        var recipients = new Dictionary<string, string>
        {
            { "destinatario@exemplo.com", "João Silva" }
        };

        // Preparar remetentes
        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        // Assunto e mensagem
        var subject = "Bem-vindo à Empresa XYZ";
        var message = "<h1>Olá, João!</h1><p>Seja bem-vindo à nossa plataforma.</p>";

        // Enviar e-mail
        var result = await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: subject,
            message: message
        );

        // Verificar se houve erros
        if (!_emailService.IsValid())
        {
            foreach (var notification in _emailService.Notifications)
            {
                Console.WriteLine($"Erro: {notification.Property} - {notification.Message}");
            }
        }
    }
}
```

### Templates HTML

#### Criar Template HTML

```html
<!-- templates/boas-vindas.html -->
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <style>
        body { font-family: Arial, sans-serif; }
        .header { background-color: #007bff; color: white; padding: 20px; }
        .content { padding: 20px; }
        .footer { background-color: #f8f9fa; padding: 10px; text-align: center; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Bem-vindo, {{NOME_CLIENTE}}!</h1>
    </div>
    <div class="content">
        <p>Olá {{NOME_CLIENTE}},</p>
        <p>Seu cadastro foi realizado com sucesso em {{DATA_CADASTRO}}.</p>
        <p>Sua conta: <strong>{{NUMERO_CONTA}}</strong></p>
        <p>Para acessar o sistema, use o seguinte link:</p>
        <p><a href="{{LINK_ACESSO}}">Acessar Sistema</a></p>
    </div>
    <div class="footer">
        <p>&copy; {{ANO}} Empresa XYZ. Todos os direitos reservados.</p>
    </div>
</body>
</html>
```

#### Usar Template

```csharp
public class TemplateEmailService
{
    private readonly IEmail _emailService;
    private readonly IWebHostEnvironment _environment;

    public TemplateEmailService(IEmail emailService, IWebHostEnvironment environment)
    {
        _emailService = emailService;
        _environment = environment;
    }

    public async Task EnviarEmailComTemplate(string nomeCliente, string emailCliente)
    {
        // Caminho do template
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Templates",
            "boas-vindas.html"
        );

        // Variáveis a serem substituídas
        var variables = new Dictionary<string, string>
        {
            { "{{NOME_CLIENTE}}", nomeCliente },
            { "{{DATA_CADASTRO}}", DateTime.Now.ToString("dd/MM/yyyy") },
            { "{{NUMERO_CONTA}}", "123456789" },
            { "{{LINK_ACESSO}}", "https://www.empresa.com/login" },
            { "{{ANO}}", DateTime.Now.Year.ToString() }
        };

        // Processar template
        var htmlMessage = _emailService.GetEmailTemplate(
            variables: variables,
            templateFullName: templatePath
        );

        // Preparar destinatários
        var recipients = new Dictionary<string, string>
        {
            { emailCliente, nomeCliente }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        // Enviar e-mail com template
        await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: "Bem-vindo à Empresa XYZ",
            message: htmlMessage
        );
    }
}
```

### Anexos

```csharp
public class EmailComAnexosService
{
    private readonly IEmail _emailService;

    public EmailComAnexosService(IEmail emailService)
    {
        _emailService = emailService;
    }

    public async Task EnviarEmailComAnexos()
    {
        var recipients = new Dictionary<string, string>
        {
            { "cliente@exemplo.com", "Cliente" }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        // Anexar PDF
        _emailService.WithAttachment(
            pathFileFullName: @"C:\Documentos\relatorio.pdf",
            midiaType: EmailMidiaType.Application,
            midiaSubType: EmailMidiaSubType.Pdf
        );

        // Anexar imagem
        _emailService.WithAttachment(
            pathFileFullName: @"C:\Imagens\logo.png",
            midiaType: EmailMidiaType.Image,
            midiaSubType: EmailMidiaSubType.Png
        );

        // Anexar arquivo de texto
        _emailService.WithAttachment(
            pathFileFullName: @"C:\Documentos\informacoes.txt",
            midiaType: EmailMidiaType.Text,
            midiaSubType: EmailMidiaSubType.Text
        );

        // Verificar quantidade de anexos
        var totalAnexos = _emailService.CountAttachments();
        Console.WriteLine($"Total de anexos: {totalAnexos}");

        // Enviar e-mail
        await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: "Documentos Anexados",
            message: "<p>Segue em anexo os documentos solicitados.</p>"
        );

        // Limpar anexos para próximo envio
        _emailService.RemoveAllAttachments();
    }

    public async Task EnviarEmailComAnexoStream()
    {
        // Criar arquivo em memória
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("Conteúdo do relatório dinâmico");
        writer.Flush();
        stream.Position = 0;

        // Anexar stream
        _emailService.WithAttachment(
            fileStream: stream,
            midiaType: EmailMidiaType.Text,
            midiaSubType: EmailMidiaSubType.Text,
            fullFileName: "relatorio-dinamico.txt"
        );

        var recipients = new Dictionary<string, string>
        {
            { "cliente@exemplo.com", "Cliente" }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: "Relatório Dinâmico",
            message: "<p>Segue relatório gerado automaticamente.</p>"
        );

        // Limpar recursos
        stream.Dispose();
        _emailService.RemoveAllAttachments();
    }
}
```

### Múltiplos Destinatários

```csharp
public class EmailMultiplosDestinatariosService
{
    private readonly IEmail _emailService;
    private readonly IConfiguration _configuration;

    public EmailMultiplosDestinatariosService(
        IEmail emailService,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task EnviarParaMultiplosDestinatarios()
    {
        // Ler destinatários do appsettings.json
        var destinatarios = _configuration
            .GetSection("EmailConfig:DestinatariosEmail")
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);

        // Ou criar manualmente
        var recipients = new Dictionary<string, string>
        {
            { "destinatario1@exemplo.com", "João Silva" },
            { "destinatario2@exemplo.com", "Maria Santos" },
            { "destinatario3@exemplo.com", "Pedro Oliveira" }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        await _emailService.EnviarAsync(
            recipients: recipients,
            senders: senders,
            subject: "Comunicado Importante",
            message: "<h1>Comunicado</h1><p>Informamos que...</p>"
        );
    }

    public async Task EnviarEmailsIndividuais()
    {
        var listaClientes = new List<(string Email, string Nome)>
        {
            ("cliente1@exemplo.com", "Cliente 1"),
            ("cliente2@exemplo.com", "Cliente 2"),
            ("cliente3@exemplo.com", "Cliente 3")
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        foreach (var cliente in listaClientes)
        {
            // Resetar instância para novo envio
            _emailService.ResetMailInstance();

            var recipients = new Dictionary<string, string>
            {
                { cliente.Email, cliente.Nome }
            };

            var message = $"<p>Olá {cliente.Nome},</p><p>Mensagem personalizada...</p>";

            await _emailService.EnviarAsync(
                recipients: recipients,
                senders: senders,
                subject: $"Mensagem para {cliente.Nome}",
                message: message
            );

            // Verificar erros
            if (!_emailService.IsValid())
            {
                Console.WriteLine($"Erro ao enviar para {cliente.Email}");
                foreach (var notification in _emailService.Notifications)
                {
                    Console.WriteLine($"  - {notification.Message}");
                }
                _emailService.RemoveAllNotifications();
            }
        }
    }
}
```

### Notificações e Logging

```csharp
public class EmailComLoggingService
{
    private readonly IEmail _emailService;
    private readonly ILogger<EmailComLoggingService> _logger;

    public EmailComLoggingService(
        IEmail emailService,
        ILogger<EmailComLoggingService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> EnviarComValidacao()
    {
        var recipients = new Dictionary<string, string>
        {
            { "cliente@exemplo.com", "Cliente" }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        try
        {
            await _emailService.EnviarAsync(
                recipients: recipients,
                senders: senders,
                subject: "Teste de E-mail",
                message: "<p>Conteúdo do e-mail</p>"
            );

            // Verificar validação
            if (!_emailService.IsValid())
            {
                // Logar erros (Notifications)
                foreach (var notification in _emailService.Notifications)
                {
                    _logger.LogError(
                        "Erro ao enviar e-mail: {Property} - {Message}",
                        notification.Property,
                        notification.Message
                    );
                }
                return false;
            }

            // Logar mensagens informativas (LogMessage)
            foreach (var logMsg in _emailService.LogMessage)
            {
                _logger.LogInformation(
                    "Log de e-mail: {Property} - {Message}",
                    logMsg.Property,
                    logMsg.Message
                );
            }

            _logger.LogInformation("E-mail enviado com sucesso para {Count} destinatários",
                recipients.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao enviar e-mail");
            return false;
        }
        finally
        {
            // Limpar notificações e logs
            _emailService.RemoveAllNotifications();
            _emailService.RemoveAllLogMessage();
        }
    }

    public async Task EnviarComRetry(int maxRetries = 3)
    {
        var recipients = new Dictionary<string, string>
        {
            { "cliente@exemplo.com", "Cliente" }
        };

        var senders = new Dictionary<string, string>
        {
            { "noreply@empresa.com", "Empresa XYZ" }
        };

        int tentativa = 0;
        bool sucesso = false;

        while (tentativa < maxRetries && !sucesso)
        {
            tentativa++;
            _logger.LogInformation("Tentativa {Tentativa} de {MaxTentativas}",
                tentativa, maxRetries);

            try
            {
                await _emailService.EnviarAsync(
                    recipients: recipients,
                    senders: senders,
                    subject: "E-mail com Retry",
                    message: "<p>Conteúdo</p>"
                );

                if (_emailService.IsValid())
                {
                    sucesso = true;
                    _logger.LogInformation("E-mail enviado com sucesso na tentativa {Tentativa}",
                        tentativa);
                }
                else
                {
                    _logger.LogWarning("Falha na tentativa {Tentativa}. Erros: {Erros}",
                        tentativa,
                        string.Join(", ", _emailService.Notifications.Select(n => n.Message))
                    );
                    _emailService.RemoveAllNotifications();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na tentativa {Tentativa}", tentativa);
            }

            if (!sucesso && tentativa < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(5 * tentativa)); // Backoff exponencial
            }
        }

        if (!sucesso)
        {
            _logger.LogError("Falha ao enviar e-mail após {MaxTentativas} tentativas",
                maxRetries);
        }
    }
}
```

## Exemplos Práticos

### Exemplo 1: Sistema de Boas-Vindas

```csharp
public class WelcomeEmailService
{
    private readonly IEmail _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WelcomeEmailService> _logger;

    public WelcomeEmailService(
        IEmail emailService,
        IWebHostEnvironment environment,
        ILogger<WelcomeEmailService> logger)
    {
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<bool> EnviarBoasVindasAsync(
        string nomeUsuario,
        string emailUsuario,
        string tokenAtivacao)
    {
        try
        {
            // Caminho do template
            var templatePath = Path.Combine(
                _environment.ContentRootPath,
                "Templates",
                "Email",
                "boas-vindas.html"
            );

            // Variáveis do template
            var variables = new Dictionary<string, string>
            {
                { "{{NOME_USUARIO}}", nomeUsuario },
                { "{{EMAIL_USUARIO}}", emailUsuario },
                { "{{DATA_CADASTRO}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm") },
                { "{{LINK_ATIVACAO}}", $"https://www.empresa.com/ativar?token={tokenAtivacao}" },
                { "{{ANO}}", DateTime.Now.Year.ToString() }
            };

            // Processar template
            var htmlMessage = _emailService.GetEmailTemplate(variables, templatePath);

            // Anexar manual do usuário
            var manualPath = Path.Combine(
                _environment.ContentRootPath,
                "Documents",
                "manual-usuario.pdf"
            );

            if (File.Exists(manualPath))
            {
                _emailService.WithAttachment(
                    pathFileFullName: manualPath,
                    midiaType: EmailMidiaType.Application,
                    midiaSubType: EmailMidiaSubType.Pdf
                );
            }

            // Preparar destinatários e remetentes
            var recipients = new Dictionary<string, string>
            {
                { emailUsuario, nomeUsuario }
            };

            var senders = new Dictionary<string, string>
            {
                { "noreply@empresa.com", "Equipe Empresa XYZ" }
            };

            // Enviar e-mail
            await _emailService.EnviarAsync(
                recipients: recipients,
                senders: senders,
                subject: $"Bem-vindo(a) à Empresa XYZ, {nomeUsuario}!",
                message: htmlMessage
            );

            // Validar envio
            if (!_emailService.IsValid())
            {
                _logger.LogError(
                    "Erro ao enviar e-mail de boas-vindas para {Email}. Erros: {Erros}",
                    emailUsuario,
                    string.Join(", ", _emailService.Notifications.Select(n => n.Message))
                );
                return false;
            }

            _logger.LogInformation(
                "E-mail de boas-vindas enviado com sucesso para {Nome} ({Email})",
                nomeUsuario,
                emailUsuario
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao enviar e-mail de boas-vindas para {Email}",
                emailUsuario);
            return false;
        }
        finally
        {
            _emailService.RemoveAllAttachments();
            _emailService.RemoveAllNotifications();
            _emailService.RemoveAllLogMessage();
        }
    }
}
```

### Exemplo 2: Notificações de Pedidos

```csharp
public class OrderNotificationService
{
    private readonly IEmail _emailService;
    private readonly IWebHostEnvironment _environment;

    public OrderNotificationService(
        IEmail emailService,
        IWebHostEnvironment environment)
    {
        _emailService = emailService;
        _environment = environment;
    }

    public async Task NotificarNovoPedidoAsync(Order order)
    {
        // Template para cliente
        var clienteTemplatePath = Path.Combine(
            _environment.ContentRootPath,
            "Templates",
            "Email",
            "confirmacao-pedido.html"
        );

        var clienteVariables = new Dictionary<string, string>
        {
            { "{{NOME_CLIENTE}}", order.CustomerName },
            { "{{NUMERO_PEDIDO}}", order.OrderNumber },
            { "{{DATA_PEDIDO}}", order.OrderDate.ToString("dd/MM/yyyy HH:mm") },
            { "{{VALOR_TOTAL}}", order.TotalAmount.ToString("C2") },
            { "{{ITENS_PEDIDO}}", GerarHtmlItensPedido(order.Items) },
            { "{{LINK_ACOMPANHAMENTO}}", $"https://www.empresa.com/pedidos/{order.Id}" }
        };

        var clienteHtml = _emailService.GetEmailTemplate(
            clienteVariables,
            clienteTemplatePath
        );

        // Gerar PDF do pedido
        var pdfStream = GerarPdfPedido(order);

        _emailService.WithAttachment(
            fileStream: pdfStream,
            midiaType: EmailMidiaType.Application,
            midiaSubType: EmailMidiaSubType.Pdf,
            fullFileName: $"pedido-{order.OrderNumber}.pdf"
        );

        // Enviar para cliente
        var clienteRecipients = new Dictionary<string, string>
        {
            { order.CustomerEmail, order.CustomerName }
        };

        var senders = new Dictionary<string, string>
        {
            { "vendas@empresa.com", "Equipe de Vendas - Empresa XYZ" }
        };

        await _emailService.EnviarAsync(
            recipients: clienteRecipients,
            senders: senders,
            subject: $"Pedido #{order.OrderNumber} - Confirmação",
            message: clienteHtml
        );

        // Limpar para próximo envio
        _emailService.ResetMailInstance();

        // Notificar equipe de vendas
        var vendedoresRecipients = new Dictionary<string, string>
        {
            { "vendas@empresa.com", "Equipe de Vendas" },
            { "gerencia@empresa.com", "Gerência" }
        };

        var vendedoresHtml = $@"
            <h2>Novo Pedido Recebido</h2>
            <p><strong>Pedido:</strong> {order.OrderNumber}</p>
            <p><strong>Cliente:</strong> {order.CustomerName}</p>
            <p><strong>Valor:</strong> {order.TotalAmount:C2}</p>
            <p><a href='https://admin.empresa.com/pedidos/{order.Id}'>Ver Detalhes</a></p>
        ";

        await _emailService.EnviarAsync(
            recipients: vendedoresRecipients,
            senders: senders,
            subject: $"[Novo Pedido] #{order.OrderNumber} - {order.CustomerName}",
            message: vendedoresHtml
        );

        // Cleanup
        pdfStream.Dispose();
        _emailService.RemoveAllAttachments();
        _emailService.RemoveAllNotifications();
    }

    private string GerarHtmlItensPedido(List<OrderItem> items)
    {
        var html = new StringBuilder();
        html.Append("<table style='width:100%; border-collapse: collapse;'>");
        html.Append("<thead><tr>");
        html.Append("<th style='border:1px solid #ddd; padding:8px;'>Produto</th>");
        html.Append("<th style='border:1px solid #ddd; padding:8px;'>Qtd</th>");
        html.Append("<th style='border:1px solid #ddd; padding:8px;'>Valor Unit.</th>");
        html.Append("<th style='border:1px solid #ddd; padding:8px;'>Total</th>");
        html.Append("</tr></thead><tbody>");

        foreach (var item in items)
        {
            html.Append("<tr>");
            html.Append($"<td style='border:1px solid #ddd; padding:8px;'>{item.ProductName}</td>");
            html.Append($"<td style='border:1px solid #ddd; padding:8px; text-align:center;'>{item.Quantity}</td>");
            html.Append($"<td style='border:1px solid #ddd; padding:8px; text-align:right;'>{item.UnitPrice:C2}</td>");
            html.Append($"<td style='border:1px solid #ddd; padding:8px; text-align:right;'>{item.TotalPrice:C2}</td>");
            html.Append("</tr>");
        }

        html.Append("</tbody></table>");
        return html.ToString();
    }

    private MemoryStream GerarPdfPedido(Order order)
    {
        // Implementação simplificada - use uma biblioteca de PDF real
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.WriteLine($"Pedido: {order.OrderNumber}");
        writer.WriteLine($"Cliente: {order.CustomerName}");
        writer.WriteLine($"Total: {order.TotalAmount:C2}");
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
```

### Exemplo 3: Newsletter em Lote

```csharp
public class NewsletterService
{
    private readonly IEmail _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        IEmail emailService,
        IWebHostEnvironment environment,
        ILogger<NewsletterService> logger)
    {
        _emailService = emailService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<NewsletterResult> EnviarNewsletterAsync(
        string tituloNewsletter,
        string conteudoHtml,
        List<Subscriber> subscribers)
    {
        var result = new NewsletterResult
        {
            TotalSubscribers = subscribers.Count,
            StartTime = DateTime.Now
        };

        var senders = new Dictionary<string, string>
        {
            { "newsletter@empresa.com", "Newsletter Empresa XYZ" }
        };

        foreach (var subscriber in subscribers)
        {
            try
            {
                // Resetar instância para cada envio
                _emailService.ResetMailInstance();

                // Personalizar conteúdo para cada assinante
                var conteudoPersonalizado = conteudoHtml
                    .Replace("{{NOME}}", subscriber.Name)
                    .Replace("{{EMAIL}}", subscriber.Email)
                    .Replace("{{LINK_CANCELAR}}",
                        $"https://www.empresa.com/newsletter/cancelar?token={subscriber.UnsubscribeToken}");

                var recipients = new Dictionary<string, string>
                {
                    { subscriber.Email, subscriber.Name }
                };

                await _emailService.EnviarAsync(
                    recipients: recipients,
                    senders: senders,
                    subject: tituloNewsletter,
                    message: conteudoPersonalizado
                );

                if (_emailService.IsValid())
                {
                    result.SuccessCount++;
                    _logger.LogDebug("Newsletter enviada para {Email}", subscriber.Email);
                }
                else
                {
                    result.FailureCount++;
                    result.FailedEmails.Add(subscriber.Email);

                    _logger.LogWarning(
                        "Falha ao enviar newsletter para {Email}. Erros: {Erros}",
                        subscriber.Email,
                        string.Join(", ", _emailService.Notifications.Select(n => n.Message))
                    );
                }

                // Delay para evitar sobrecarga do servidor SMTP
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.FailedEmails.Add(subscriber.Email);
                _logger.LogError(ex, "Exceção ao enviar newsletter para {Email}",
                    subscriber.Email);
            }
            finally
            {
                _emailService.RemoveAllNotifications();
                _emailService.RemoveAllLogMessage();
            }
        }

        result.EndTime = DateTime.Now;
        result.Duration = result.EndTime - result.StartTime;

        _logger.LogInformation(
            "Newsletter finalizada. Enviados: {Success}, Falhas: {Failures}, Duração: {Duration}",
            result.SuccessCount,
            result.FailureCount,
            result.Duration
        );

        return result;
    }
}

public class NewsletterResult
{
    public int TotalSubscribers { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedEmails { get; set; } = new List<string>();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
}

public class Subscriber
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string UnsubscribeToken { get; set; }
}
```

## API Reference

### IEmail Interface

```csharp
public interface IEmail
{
    // Propriedades
    List<NotificationR> Notifications { get; set; }
    List<NotificationR> LogMessage { get; set; }
    EmailServerConfiguration EmailServerConfiguration { get; set; }

    // Métodos de validação
    bool IsValid();

    // Envio de e-mail
    Task<object> EnviarAsync(
        Dictionary<string, string> recipients,
        Dictionary<string, string> senders,
        string subject,
        string message,
        CancellationToken cancellationToken = default);

    // Templates
    string GetEmailTemplate(
        Dictionary<string, string> variables,
        string templateFullName);

    // Anexos
    IEmail WithAttachment(
        string pathFileFullName,
        EmailMidiaType midiaType,
        EmailMidiaSubType midiaSubType);

    IEmail WithAttachment(
        Stream fileStream,
        EmailMidiaType midiaType,
        EmailMidiaSubType midiaSubType,
        string fullFileName);

    int CountAttachments();
    IEmail RemoveAllAttachments();

    // Gerenciamento
    IEmail RemoveAllLogMessage();
    IEmail RemoveAllNotifications();
    void ResetMailInstance();
}
```

### EmailServerConfiguration

```csharp
public class EmailServerConfiguration
{
    public string ServerHost { get; set; }
    public int Port { get; set; }
    public SslProtocols Security { get; set; }
    public string AccountUserName { get; set; }
    public string AccountPassword { get; set; }
}
```

### EmailMidiaType & EmailMidiaSubType

```csharp
public enum EmailMidiaType
{
    Text,        // Arquivos de texto
    Image,       // Imagens
    Application  // Documentos (PDF, etc.)
}

public enum EmailMidiaSubType
{
    Text,  // .txt
    Gif,   // .gif
    Jpg,   // .jpg, .jpeg
    Png,   // .png
    Pdf    // .pdf
}
```

### PessoasEmail

```csharp
public enum PessoasEmail
{
    From = 0,  // Remetente
    To = 1,    // Destinatário
    Cc = 2,    // Com Cópia
    Bcc = 3    // Com Cópia Oculta
}
```

## Troubleshooting

### Problemas Comuns

#### 1. Falha na autenticação SMTP

**Problema**: Erro "Authentication failed" ao enviar e-mail

**Causas Comuns**:
- Credenciais incorretas
- Gmail bloqueando "apps menos seguros"
- 2FA habilitado sem App Password

**Solução**:
```json
// Para Gmail, use App Passwords
{
  "EmailServerConfiguration": {
    "ServerHost": "smtp.gmail.com",
    "Port": 587,
    "Security": "StartTls",
    "AccountUserName": "seu-email@gmail.com",
    "AccountPassword": "xxxx xxxx xxxx xxxx"  // App Password de 16 dígitos
  }
}
```

#### 2. E-mails não são enviados

**Problema**: Método `EnviarAsync()` executa mas e-mails não chegam

**Solução**: Verificar notificações
```csharp
await _emailService.EnviarAsync(recipients, senders, subject, message);

if (!_emailService.IsValid())
{
    // Verificar erros
    foreach (var notification in _emailService.Notifications)
    {
        Console.WriteLine($"Erro: {notification.Message}");
    }
}
```

#### 3. Anexos não aparecem

**Problema**: E-mail enviado mas anexos não estão presentes

**Causa**: Caminho do arquivo incorreto ou tipo MIME errado

**Solução**:
```csharp
// Verificar se arquivo existe
var filePath = @"C:\Docs\file.pdf";
if (!File.Exists(filePath))
{
    throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
}

// Usar tipo MIME correto
_emailService.WithAttachment(
    pathFileFullName: filePath,
    midiaType: EmailMidiaType.Application,  // Para PDFs
    midiaSubType: EmailMidiaSubType.Pdf
);

// Verificar se foi anexado
var count = _emailService.CountAttachments();
Console.WriteLine($"Total de anexos: {count}");
```

#### 4. Template não substitui variáveis

**Problema**: Variáveis como `{{NOME}}` aparecem literalmente no e-mail

**Causa**: Variáveis não correspondem às do template

**Solução**:
```csharp
// ✅ Correto - Variáveis exatamente como no template
var variables = new Dictionary<string, string>
{
    { "{{NOME}}", "João" },        // Incluir {{ }}
    { "{{EMAIL}}", "joao@email.com" }
};

// ❌ Incorreto
var variables = new Dictionary<string, string>
{
    { "NOME", "João" },            // Faltam {{ }}
    { "EMAIL", "joao@email.com" }
};
```

#### 5. Erro de conexão SSL/TLS

**Problema**: "The remote certificate is invalid"

**Causa**: Problemas com certificado SSL

**Solução**:
```csharp
// Ajustar protocolo de segurança
{
  "Security": "StartTls"  // Ou "Tls12" dependendo do servidor
}

// Para desenvolvimento (NÃO usar em produção)
// Adicionar callback para ignorar erros de certificado
```

#### 6. Timeout ao enviar e-mail

**Problema**: Operação demora muito ou timeout

**Causa**: Servidor SMTP lento ou problema de rede

**Solução**:
```csharp
// Usar CancellationToken com timeout
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await _emailService.EnviarAsync(
        recipients,
        senders,
        subject,
        message,
        cts.Token
    );
}
catch (OperationCanceledException)
{
    _logger.LogError("Timeout ao enviar e-mail");
}
```

### Logs e Debugging

Para debugging detalhado:

```csharp
public class EmailDebugService
{
    private readonly IEmail _emailService;
    private readonly ILogger<EmailDebugService> _logger;

    public async Task EnviarComDebugAsync()
    {
        _logger.LogInformation("Iniciando envio de e-mail");

        // Log configuração (sem senha!)
        var config = _emailService.EmailServerConfiguration;
        _logger.LogInformation(
            "Configuração: {Host}:{Port}, Security: {Security}",
            config.ServerHost,
            config.Port,
            config.Security
        );

        await _emailService.EnviarAsync(recipients, senders, subject, message);

        // Log resultado
        _logger.LogInformation("Notificações: {Count}", _emailService.Notifications.Count);
        _logger.LogInformation("Log Messages: {Count}", _emailService.LogMessage.Count);
        _logger.LogInformation("Anexos: {Count}", _emailService.CountAttachments());

        // Log detalhado de erros
        foreach (var notification in _emailService.Notifications)
        {
            _logger.LogError(
                "Notificação - Property: {Property}, Message: {Message}, Time: {Time}",
                notification.Property,
                notification.Message,
                notification.DateOccurrence
            );
        }

        // Log de mensagens informativas
        foreach (var logMsg in _emailService.LogMessage)
        {
            _logger.LogInformation(
                "LogMessage - Property: {Property}, Message: {Message}",
                logMsg.Property,
                logMsg.Message
            );
        }
    }
}
```

## Changelog

Ver arquivo [CHANGELOG.md](CHANGELOG.md) para histórico detalhado de alterações.

---

## Funcionalidades planejadas

### v1.1.0
- Suporte a templates Razor para e-mails dinâmicos com cache de templates compilados
- Sistema de fila para processamento assíncrono com prioridades e agendamento
- Métricas e telemetria de envio via OpenTelemetry
- Health check endpoint para validar SMTP

### v1.2.0
- Suporte a provedores alternativos (SendGrid SDK, AWS SES SDK) com fallback automático
- Imagens inline (embedded images) no HTML
- Validação avançada de e-mails (syntax, MX records)
- Limitação de taxa de envio

### v2.0.0
- API async-only (breaking change — remoção de métodos síncronos)
- Suporte a multi-tenant
- Streaming assíncrono de anexos grandes

---

## Suporte

Para dúvidas, issues ou contribuições:
- **Issues**: [GitHub Issues](https://github.com/nuuvify/CommonPack/issues)
- **Documentação**: [Wiki do Projeto](https://github.com/nuuvify/CommonPack/wiki)

---
**Nuuvify CommonPack** - Construindo soluções robustas para .NET
