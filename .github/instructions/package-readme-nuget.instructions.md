---
description: "Use when creating or updating package README files for Nuuvify.CommonPack so they are robust and ready for NuGet package pages."
name: "Nuuvify Package README NuGet"
applyTo: "src/**/README.md"
---

# Diretrizes para README de pacote (NuGet-ready)

Use estas regras sempre que criar ou atualizar `README.md` de pacote em `src/**`.

## Objetivo

- Garantir documentação clara para consumidores externos.
- Manter padrão consistente entre pacotes.
- Produzir README adequado para exibição no nuget.org.

## Estrutura mínima obrigatória

Todo README de pacote deve conter, no mínimo:

1. Título com nome exato do pacote.
2. Badges (CI e NuGet versão/downloads quando aplicável).
3. Descrição curta do propósito do pacote.
4. Índice navegável.
5. Seção "Quando usar".
6. Seção "Instalação" com `PackageReference`.
7. Seção "Configuração" (quando houver DI/opções).
8. Seção "Exemplo de uso" com código funcional e alinhado ao contrato público.
9. Seção "Boas práticas" e/ou "Segurança" (quando aplicável).
10. Seção "Troubleshooting" para erros comuns.
11. Seção "Compatibilidade" (framework alvo e dependências relevantes).

## Regras de conteúdo

- Escreva para consumidor do pacote, não para mantenedor interno.
- Priorize exemplos reais e curtos.
- Use placeholders seguros para segredos e ambientes.
- Não exponha valores sensíveis, endpoints privados ou credenciais reais.
- Evite texto de marketing e linguagem genérica.
- Evite exemplos que não compilarão com a API pública atual.

## Regras de qualidade técnica

- Todo exemplo deve refletir nomes reais de classes/interfaces/métodos públicos.
- Se houver mudanças de comportamento observável, atualize também o `CHANGELOG.md` do pacote.
- Se o pacote depende de outro pacote do repositório para funcionar, declarar na seção de dependências.

## Checklist rápido antes de concluir

1. O README permite onboarding sem abrir código-fonte?
2. Os exemplos batem com as APIs públicas atuais?
3. O conteúdo está focado em uso externo e cenários reais?
4. O texto está consistente com `Readme.md` raiz e `CHANGELOG.md` do pacote?
