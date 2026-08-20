# Nuuvify.CommonPack.Middleware

Middlewares, filtros e extensões de configuração para aplicações ASP.NET Core.

## Índice

- [Instalação](#instalação)
- [Configuração](#configuração)
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

O padrão é fail-closed (`optional: false`) e sem recarga automática (`reloadOnChange: false`). Para um diretório opcional:

```csharp
builder.Configuration.AddContainerSecrets("/run/secrets", optional: true);
```

Um arquivo chamado `Database__Password` fica disponível como `Database:Password`. O provider `KeyPerFile` não altera, copia ou remove os arquivos montados. Permissões, montagem e rotação pertencem ao runtime ou ao orquestrador.

O método legado `AddEnvironmentVariablesToKeyPerFile` continua disponível para compatibilidade. Ele agora captura os valores em memória e pode remover as variáveis somente do processo atual quando `removeVariavel` for verdadeiro. Para novos mounts de secrets, prefira `AddContainerSecrets`.

Os providers são aplicados na ordem em que são registrados; fontes posteriores podem substituir chaves anteriores.

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
