# Changelog - Nuuvify.CommonPack.Domain

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado
- Suporte a CNPJ alfanumérico no value object `Cnpj` (12 caracteres base `[A-Z0-9]` + 2 dígitos verificadores numéricos).
- Novos cenários de máscara para CNPJ alfanumérico no formato `XX.XXX.XXX/XXXX-XX`.

### Alterado
- Validação de CNPJ atualizada para preservar letras após remoção de pontuação e aplicar cálculo de dígitos verificadores compatível com base alfanumérica.
- Constante `maxCNPJ` renomeada para `MaxCNPJ`.

### Corrigido
- Rejeição explícita de entradas com caracteres inválidos, tamanho incorreto ou repetição total de caracteres no CNPJ.

### Removido

### Segurança

## [Sem versão registrada] - 2026-05-29

### Adicionado
- Estrutura inicial do changelog padronizada para este pacote.
