# Guia de Contribuição

Obrigado por contribuir com o Nuuvify.CommonPack.

Este repositório mantém bibliotecas .NET reutilizáveis para aplicações corporativas. A contribuição ideal é pequena, objetiva, testada e alinhada com os padrões já adotados no projeto.

## Antes de começar

Leia estes arquivos antes de abrir sua primeira contribuição:

- [Readme.md](../Readme.md)
- [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md)
- [SECURITY.md](./SECURITY.md)
- [SUPPORT.md](./SUPPORT.md)
- [.editorconfig](../.editorconfig)
- [CHANGELOG.md](../CHANGELOG.md)

## Pré-requisitos

- .NET SDK `8.0.303` conforme [global.json](../global.json)
- Git atualizado
- Visual Studio 2022 ou VS Code
- PowerShell 7 recomendado para os scripts do diretório `test/`

Verificações rápidas:

```powershell
dotnet --version
dotnet --info
```

## Setup local

1. Faça um fork do repositório.
2. Clone o seu fork.
3. Crie uma branch a partir de `main`.
4. Restaure as dependências.
5. Execute build e testes antes de começar a alterar código.

Fluxo sugerido:

```powershell
git clone <seu-fork>
cd Nuuvify.CommonPack-policy
git checkout -b feat/minha-alteracao
dotnet restore
dotnet build Nuuvify.CommonPack.sln
dotnet test --filter "Category=Unit"
```

## Estrutura do repositório

- `src/`: pacotes principais da solução
- `test/`: projetos de teste `*.xTest`, scripts de apoio e cobertura
- `Samples/`: exemplos de uso
- `.github/`: políticas, templates e automações de comunidade
- `docs/`: guias para contribuidores e mantenedores

## Convenções técnicas

### Código

- Siga as regras de estilo definidas em [.editorconfig](../.editorconfig).
- Mantenha as alterações focadas no problema tratado.
- Evite refatorações não relacionadas na mesma contribuição.
- Preserve compatibilidade pública sempre que possível.
- Atualize documentação quando a mudança alterar comportamento, setup ou API.

### Commits

O repositório utiliza Conventional Commits e versionamento SemVer.

Exemplos:

```text
feat(unitofwork): adiciona filtro para propriedade aninhada
fix(email): corrige tratamento de anexos nulos
docs(readme): atualiza instrucoes de contribuicao
```

Se a alteração impactar release notes, atualize o [CHANGELOG.md](../CHANGELOG.md).

### Branches

- Use branches curtas e descritivas.
- Prefira prefixos como `feat/`, `fix/`, `docs/`, `refactor/` ou `test/`.
- Não envie trabalho diretamente para `main`.
- PRs para `main` exigem aprovação do code owner `@lzocateli`.
- O fluxo de release do projeto é baseado em branch:
	- `main`: release estável com tag e GitHub Release
	- `qas`: pacote preview no NuGet.org
	- `nugettest/qas`: pacote `dev` no feed interno de homologação

## Testes

Toda alteração relevante em código deve ser acompanhada por testes novos ou atualização dos testes existentes.

Comandos mais usados:

```powershell
dotnet build Nuuvify.CommonPack.sln
dotnet test
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

O script [test/run-tests.ps1](../test/run-tests.ps1) também pode ser usado para gerar cobertura, mas ele deve ser executado a partir do diretório `test/`.

```powershell
cd test
pwsh ./run-tests.ps1
```

Padrões esperados:

- xUnit para testes
- `Trait("Category", "Unit")` ou `Trait("Category", "Integration")`
- Moq para isolamento de dependências
- Bogus para dados de teste
- Nomes de teste claros no formato `Metodo_Cenario_ComportamentoEsperado`

## Pull requests

Antes de abrir um pull request, confirme:

- A solução compila localmente
- Os testes relevantes passam localmente
- A documentação foi atualizada quando necessário
- O changelog foi atualizado quando a mudança impacta release
- Não há mudanças acidentais em arquivos não relacionados

Ao abrir o PR:

1. Descreva o problema e a solução.
2. Informe impacto em compatibilidade, segurança e performance, se houver.
3. Referencie issues relacionadas.
4. Anexe evidências quando a mudança afetar comportamento observável.
5. Escolha corretamente a branch de destino conforme o objetivo de release da mudança.

## O que é uma boa contribuição inicial

- Correções de documentação
- Melhorias de testes
- Correções pequenas e isoladas
- Issues com label `good first issue` ou `help wanted`, quando disponíveis

## O que evitar

- Mudanças grandes sem contexto prévio
- Alterações de API pública sem justificar compatibilidade
- PRs com múltiplos objetivos sem relação entre si
- Reportar vulnerabilidades em issue pública

## Dúvidas e suporte

Use [SUPPORT.md](./SUPPORT.md) para escolher o canal correto.
