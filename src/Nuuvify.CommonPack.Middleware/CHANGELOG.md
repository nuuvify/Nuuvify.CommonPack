# Changelog - Nuuvify.CommonPack.Middleware

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado
- Nova extensão `AddContainerSecrets` para carregar secrets montados usando o provider `KeyPerFile`.

### Alterado

### Corrigido
- Carregamento silenciosamente vazio de `AddEnvironmentVariablesToKeyPerFile` após a remoção do diretório temporário.

### Removido

### Segurança
- Eliminada a persistência temporária de secrets em arquivos locais pelo método legado.

## [Sem versão registrada] - 2026-05-29

### Adicionado
- Estrutura inicial do changelog padronizada para este pacote.
