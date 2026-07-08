---
description: "Use when creating or updating .targets support for packages in src/** within Nuuvify.CommonPack. Ensures consistent content copy behavior and avoids duplicate MSBuild items/targets."
name: "Nuuvify Targets Source"
applyTo: "src/**/*.targets, src/**/*.csproj"
---

# Diretrizes para arquivos .targets em src/**

Use estas regras ao criar ou alterar arquivos `*.targets` para pacotes em `src/**`.

## Objetivo

- Garantir que novos pacotes em `src/**` tenham um `.targets` funcional para distribuição via NuGet.
- Padronizar o comportamento de copia de conteudo para `Docs`.
- Evitar duplicidade de `ItemGroup`, `Target`, `Include` e efeitos colaterais em build.

## Padrao obrigatorio para novos `.targets`

Quando um pacote novo em `src/**` precisar de `.targets`, use como base o padrao adotado no repositório (equivalente ao `Nuuvify.CommonPack.StandardHttpClient.targets`), com nomes unicos por pacote:

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <SourceCustomFiles<PackageAlias>Xml Include="$(MSBuildThisFileDirectory)..\content\*.xml" />
  </ItemGroup>
  <Target Name="<PackageAlias>CopyFilesToProject" BeforeTargets="Build">
    <Copy SourceFiles="@(SourceCustomFiles<PackageAlias>Xml)" DestinationFolder="$(ProjectDir)\Docs" />
  </Target>
</Project>
```

Substitua `<PackageAlias>` por um identificador curto e unico do pacote.

## Regras anti-duplicidade

- Sempre use nomes unicos para:
  - Item: `SourceCustomFiles<PackageAlias>Xml`
  - Target: `<PackageAlias>CopyFilesToProject`
- Nao reutilize nomes genericos em varios pacotes (ex.: `CopyFilesToProject`).
- Nao crie mais de um `Target` com a mesma finalidade dentro do mesmo pacote.
- Nao duplique regras ja existentes no `.targets` atual; altere apenas o necessario.

## Escopo

- Aplicar apenas a projetos em `src/**`.
- Nao criar `.targets` para `test/**` por padrao.
- Em `src/**`, se existir `.targets` vazio, preencher com o padrao acima.

## Compatibilidade com empacotamento

- O nome do arquivo deve seguir exatamente o nome do projeto:
  - `NomeProjeto.csproj` -> `NomeProjeto.targets`
- O `.targets` deve ficar na mesma pasta do `.csproj`.
- Preservar o comportamento de pack configurado no `src/Directory.Build.props`.

## Validacao minima recomendada

Depois de criar/alterar `.targets` de um pacote em `src/**`, validar:

```powershell
dotnet pack src/<Pacote>/<Pacote>.csproj -c Release --no-build -v minimal
```

Se o pacote exigir build previo:

```powershell
dotnet build src/<Pacote>/<Pacote>.csproj -c Release
dotnet pack src/<Pacote>/<Pacote>.csproj -c Release --no-build -v minimal
```
