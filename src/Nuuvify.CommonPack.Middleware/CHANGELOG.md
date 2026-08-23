# Changelog - Nuuvify.CommonPack.Middleware

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

- Nova extensão `AddContainerSecrets` para carregar secrets montados usando o provider `KeyPerFile`.
- Handler opt-in baseado em `IExceptionHandler` e `ProblemDetailsService`.
- Adapter HTTP opt-in para preencher `OperationContext` por requisição.
- Loader canônico `AddDotEnvConfiguration` sem mutação do ambiente do processo.
- Setup opt-in `AddCanonicalValidation` com HTTP 400 e `ValidationProblemDetails`.
- Integração de `AddCanonicalValidation` com `IMvcBuilder` existente.

### Compatibilidade

- `PathSecrets`, `SetPathSecretsToOSPlatform` e `GetPathSecretsToOSPlatform` agora emitem aviso de obsolescência não bloqueante; use `GetContainerSecretsPath`.
- `AddEnvironmentVariablesToKeyPerFile` agora emite aviso de obsolescência não bloqueante; use `AddContainerSecrets` ou `AddDotEnvConfiguration`.
- `AddEnvironmentVariablesToMemoryCollection` agora emite aviso de obsolescência não bloqueante; use o provider nativo de variáveis ou `AddDotEnvConfiguration`.
- `ValidateModelStateCustomAttribute`, `FileStreamResultCustom` e `GetFilesBase64` agora emitem avisos de obsolescência não bloqueantes; use os APIs nativos do ASP.NET Core.

### Alterado

### Corrigido

- Carregamento silenciosamente vazio de `AddEnvironmentVariablesToKeyPerFile` após a remoção do diretório temporário.

### Removido

### Segurança

- Eliminada a persistência temporária de secrets em arquivos locais pelo método legado.
- Respostas modernas de exceção não expõem mensagens internas.

## [Sem versão registrada] - 2026-05-29

### Histórico

- Estrutura inicial do changelog padronizada para este pacote.
