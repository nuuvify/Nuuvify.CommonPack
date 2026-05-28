---
description: "Use when creating, altering, fixing, or refactoring code in Nuuvify.CommonPack.UnitOfWork or Nuuvify.CommonPack.UnitOfWork.Abstraction. Covers IQueryable composition, LINQ/EF translation, pagination, dynamic filters, ordering, provider compatibility, and public extension safety."
name: "Nuuvify UnitOfWork Source"
applyTo: "src/Nuuvify.CommonPack.UnitOfWork/**/*.cs, src/Nuuvify.CommonPack.UnitOfWork.Abstraction/**/*.cs, src/Nuuvify.CommonPack.UnitOfWork/**/*.csproj, src/Nuuvify.CommonPack.UnitOfWork.Abstraction/**/*.csproj"
---

# Diretrizes para UnitOfWork

- Preserve composição encadeável sobre `IQueryable` sempre que isso fizer parte do contrato observável.
- Antes de simplificar expressões ou pipelines de consulta, valide impacto em tradução LINQ para EF Core.
- Trate paginação, filtros dinâmicos, ordenação e projeções como superfície pública do pacote.
- Evite mudanças que funcionem apenas em um provider se o contrato do pacote continuar declarando compatibilidade mais ampla.
- Em extensões públicas, preserve nomes estáveis, comportamento previsível e assinaturas coerentes com SemVer.

## Cuidados de implementação

- Prefira mudanças pequenas em expressões, builders, extensões e repositórios.
- Não troque legibilidade por abstrações que dificultem rastrear a query final.
- Se houver risco de regressão em performance ou tradução, acompanhe a mudança com teste ou validação estreita.

## Validação prioritária

- queries encadeadas
- filtros dinâmicos
- ordenação e paginação
- compatibilidade com APIs públicas existentes