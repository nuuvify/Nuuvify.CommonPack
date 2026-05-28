---
description: "Use when creating, altering, fixing, or extending C# source code in src/** for Nuuvify.CommonPack libraries. Covers package boundaries, public API stability, DI, async flows, exceptions, Clean Architecture, SOLID, and SemVer-safe changes."
name: "Nuuvify Library Source"
applyTo: "src/**/*.cs, src/**/*.csproj"
---

# Diretrizes para código de biblioteca

- Comece pelo pacote dono do comportamento; evite mudanças cruzadas sem necessidade objetiva.
- Preserve contratos públicos. Se precisar alterar assinatura, comportamento padrão, exceção observável ou nome público, trate como risco de breaking change.
- Prefira composição, interfaces pequenas e classes com uma responsabilidade clara.
- Em `*.Abstraction`, mantenha apenas contratos, modelos compartilhados mínimos e extensões realmente neutras.
- Em implementações, esconda detalhes de infraestrutura atrás de abstrações quando isso simplificar teste, evolução ou substituição.
- Prefira construtores explícitos e opções tipadas a acesso indireto ou estado implícito.
- Propague `CancellationToken` em operações assíncronas quando o fluxo já o possuir.
- Não introduza estado global, singletons acoplados ou helpers genéricos sem dono claro.
- Quando adicionar comportamento novo, tente manter o ponto de extensão local ao pacote, sem contaminar outros módulos.

## Qualidade de implementação

- Use nomes orientados ao domínio do pacote, não nomes genéricos.
- Trate nullability, guard clauses e mensagens de erro como parte do contrato do pacote.
- Prefira retorno previsível e erros específicos a fluxos implícitos.
- Adicione comentários apenas onde a intenção arquitetural não for evidente pelo código.
- Mantenha `*.cs` e `*.csproj` em `UTF-8` e preserve o fim de linha configurado no `.editorconfig`, evitando conversões arbitrárias entre `CRLF` e `LF`.

## Impacto esperado por mudança

- Código novo ou alterado.
- Testes do pacote tocado.
- Documentação do pacote quando uso externo ou comportamento observável mudar.
