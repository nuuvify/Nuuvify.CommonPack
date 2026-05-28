# GitHub Copilot Instructions

## Escopo do repositório

- Este repositório mantém bibliotecas .NET independentes, publicadas como pacotes reutilizáveis.
- O código-fonte fica em `src/`; os testes ficam em `test/`; documentação transversal fica em `docs/`.
- O target principal é `.NET 8` com C# moderno.

## Regras sempre válidas

- Faça mudanças pequenas e locais ao pacote dono do comportamento.
- Preserve compatibilidade de API pública e trate mudanças quebradoras como exceção explícita.
- Prefira Clean Architecture, SOLID, baixo acoplamento e alta testabilidade.
- Atualize testes para toda mudança de comportamento.
- Atualize `README.md` e `CHANGELOG.md` do pacote quando o contrato público, uso ou comportamento observável mudar.
- Siga `.editorconfig` e as convenções já existentes antes de introduzir novos padrões.
- Mantenha arquivos de programa e projeto, incluindo `*.cs` e `*.csproj`, em `UTF-8` e com fim de linha definido pelo `.editorconfig`, preservando compatibilidade entre Linux, Windows e macOS.

## Limites arquiteturais

- Pacotes `*.Abstraction` definem contratos; não coloque neles lógica de infraestrutura.
- Evite acoplamento entre pacotes independentes quando uma dependência local ou abstração resolver.
- Prefira extensões, serviços e opções pequenas a classes utilitárias monolíticas.
- Em bibliotecas, dê prioridade a nomes estáveis, erros previsíveis e composição via DI.

## Build e validação

- Build da solução: `dotnet build Nuuvify.CommonPack.sln`
- Testes: `dotnet test`
- Testes por categoria: `dotnet test --filter "Category=Unit"` e `dotnet test --filter "Category=Integration"`
- Cobertura: `pwsh ./test/run-tests.ps1`

## Uso eficiente de contexto

- Não replique documentação longa aqui; carregue instruções sob demanda em `.github/instructions/`.
- Use prompts de `.github/prompts/` para tarefas repetíveis.
- Use agentes em `.github/agents/` quando a tarefa pedir revisão somente leitura ou refatoração com ferramentas restritas.
- Use a skill `library-maintenance` para fluxos maiores de criar, alterar, refatorar, testar, revisar e documentar.

## Referências do projeto

- Arquitetura: `docs/maintainers/architecture.md`
- Testes: `docs/contributing/testing.md`
- Guia de IA do repositório: `docs/ai/README.md`
