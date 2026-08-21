# Nuuvify.CommonPack.Middleware

Middlewares, filtros e extensões de configuração para aplicações ASP.NET Core.

## Índice

- [Instalação](#instalação)
- [Configuração](#configuração)
- [Dotenv canônico](#dotenv-canônico)
- [Tratamento de exceções](#tratamento-de-exceções)
- [Contexto de operação](#contexto-de-operação)
- [Validação](#validação)
- [Segurança](#segurança)
- [Compatibilidade](#compatibilidade)
- [Troubleshooting](#troubleshooting)

## Quando usar

Use este pacote quando a aplicação precisar dos middlewares do pacote ou de carregar secrets montados em configuração hierárquica.

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.Middleware" Version="2.8.0" />
```

## Configuração

Para secrets montados por Docker, Podman ou Kubernetes, registre o diretório durante o startup:

```csharp
builder.Configuration.AddContainerSecrets("/run/secrets");
```

Quando o caminho precisa seguir o padrão do sistema operacional, use o
resolvedor sem estado:

```csharp
var secretsPath = builder.GetContainerSecretsPath();
builder.Configuration.AddContainerSecrets(secretsPath, optional: true);
```

O padrão é fail-closed (`optional: false`) e sem recarga automática (`reloadOnChange: false`). Para um diretório opcional:

```csharp
builder.Configuration.AddContainerSecrets("/run/secrets", optional: true);
```

Um arquivo chamado `Database__Password` fica disponível como `Database:Password`. O provider `KeyPerFile` não altera, copia ou remove os arquivos montados. Permissões, montagem e rotação pertencem ao runtime ou ao orquestrador.

O método legado `AddEnvironmentVariablesToKeyPerFile` continua disponível para compatibilidade, mas está obsoleto. Ele agora captura os valores em memória e pode remover as variáveis somente do processo atual quando `removeVariavel` for verdadeiro. Para novos mounts de secrets, prefira `AddContainerSecrets`; para arquivos `.env`, use `AddDotEnvConfiguration`.

`PathSecrets`, `SetPathSecretsToOSPlatform` e `GetPathSecretsToOSPlatform`
também estão obsoletos. Eles permanecem para compatibilidade, mas mantêm estado
estático e devem ser substituídos por `GetContainerSecretsPath`.

Os providers são aplicados na ordem em que são registrados; fontes posteriores podem substituir chaves anteriores.

## Dotenv canônico

Para carregar um arquivo `.env` sem materializar seus valores no ambiente do
processo, use:

```csharp
builder.AddDotEnvConfiguration();
```

O parser separa chave e valor no primeiro `=`, preserva valores vazios,
converte `__` em `:` e insere a fonte abaixo das fontes já registradas. Assim,
variáveis de ambiente reais, secrets montados e argumentos de linha de comando
podem manter precedência conforme a composição do host.

## Tratamento de exceções

Para aplicações novas, registre o handler baseado em `IExceptionHandler` e
`ProblemDetailsService` no composition root:

```csharp
builder.Services.AddProblemDetailsExceptionHandler();
```

O registro é opt-in e deve ser combinado com `app.UseExceptionHandler()`. O
handler retorna `application/problem+json` com mensagem genérica e registra a
exceção somente no logger. O middleware legado
`UseGlobalExceptionHandlerMiddleware` continua disponível para consumidores
existentes e mantém o envelope anterior.

## Contexto de operação

Para integrar requisições HTTP ao contexto neutro de observabilidade, registre
o accessor e adicione o adapter ao pipeline:

```csharp
builder.Services.AddOperationContextHeaders();
app.UseOperationContextHeaders();
```

O adapter preserva o `CorrelationId` recebido, cria um identificador quando o
header não existe e restaura o contexto anterior ao finalizar a requisição. O
middleware legado `UseHandlingHeadersMiddleware` continua disponível para
consumidores existentes.

## Validação

Para APIs novas, registre a resposta canônica de validação do ASP.NET Core:

```csharp
builder.Services.AddCanonicalValidation();
```

Quando o MVC já foi registrado, aplique a extensão diretamente ao builder
existente para evitar um segundo registro de controllers:

```csharp
builder.Services.AddControllers().AddCanonicalValidation();
```

O setup responde com HTTP 400 e `ValidationProblemDetails`. O filtro legado
`ValidateModelStateCustomAttribute` permanece disponível para endpoints que
dependem do contrato anterior com HTTP 417.

## Segurança

`IConfiguration` não é um cofre de secrets. Evite expor valores em logs, diagnósticos, dumps ou endpoints e controle as permissões do diretório montado. O pacote não implementa criptografia, rotação ou eliminação de cópias fora do processo atual.

## Compatibilidade

- .NET 8.
- Depende de `Microsoft.Extensions.Configuration.KeyPerFile`.
- Compatível com diretórios de secrets montados em modo somente leitura.

## Troubleshooting

### O diretório obrigatório não existe

Verifique o mount e as permissões do runtime. Use `optional: true` somente quando a ausência do diretório for realmente aceitável.

### A chave não aparece na configuração

Confirme o nome do arquivo, o diretório registrado e a ordem dos providers. Use `Database__Password` para obter `Database:Password` e não registre o valor do secret durante o diagnóstico.
