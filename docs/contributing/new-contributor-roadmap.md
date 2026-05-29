# Roteiro para novos contribuidores

Este roteiro foi pensado para quem vai contribuir pela primeira vez com o Nuuvify.CommonPack.

## 1. Entenda o contexto do projeto

Antes de alterar código, leia:

- [Guia de contribuição](../../.github/CONTRIBUTING.md)
- [Readme](../../Readme.md)
- [EditorConfig](../../.editorconfig)
- [Changelog](../../CHANGELOG.md)

## 2. Prepare o ambiente

Pré-requisitos:

- .NET SDK `8.0.303` (conforme `global.json`)
- Git atualizado
- Visual Studio 2022 ou VS Code
- PowerShell 7 (recomendado)

Validação rápida:

```powershell
dotnet --version
dotnet --info
```

## 3. Faça fork e branch de trabalho

Fluxo inicial:

```powershell
git clone https://github.com/<seu-usuario>/Nuuvify.CommonPack.git
cd Nuuvify.CommonPack-policy
git checkout -b feat/minha-alteracao
```

Boas práticas de branch:

- Use prefixos como `feat/`, `fix/`, `docs/`, `refactor/`, `test/`
- Mantenha a branch curta e focada em um único objetivo

## 4. Restaure e valide antes de codar

```powershell
dotnet restore
dotnet build Nuuvify.CommonPack.sln
dotnet test --filter "Category=Unit"
```

## 5. Faça mudanças pequenas e objetivas

- Altere somente o pacote relacionado ao problema
- Evite misturar refatoração não relacionada na mesma PR
- Preserve compatibilidade pública sempre que possível

## 6. Garanta testes da mudança

Comandos mais usados:

```powershell
dotnet test
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

Cobertura com script do repositório:

```powershell
cd test
pwsh ./run-tests.ps1
```

Importante: execute o script a partir de `test/`.

## 7. Faça commits no padrão esperado

O repositório usa Conventional Commits + SemVer.

Exemplos:

```text
feat(unitofwork): adiciona filtro para propriedade aninhada
fix(email): corrige tratamento de anexos nulos
docs(readme): atualiza instrucoes de contribuicao
```

## 8. Abra o pull request com checklist completo

Antes de abrir PR:

- Garanta build e testes relevantes passando
- Atualize documentação e changelog quando necessário
- Verifique se não há arquivos não relacionados na alteração

No PR:

- Descreva problema e solução com objetividade
- Informe impactos (compatibilidade, segurança, performance), quando houver
- Relacione issue (por exemplo: `Closes #123`)
- Inclua evidências (logs, prints, cobertura), quando aplicável

Consulte também o template: [pull_request_template.md](../../pull_request_template.md)

## 9. Escolha a branch de destino correta

- `main`: release estável
- `qas`: preview
- `nugettest/qas`: build `dev` para homologação

## 10. Comece por contribuições de baixo risco

Sugestões para primeira contribuição:

- Ajuste de documentação
- Melhoria de testes
- Correção pequena e isolada

Evite no início:

- Mudanças grandes sem alinhamento prévio
- Alterações de API pública sem justificativa
- PR com múltiplos objetivos sem relação
