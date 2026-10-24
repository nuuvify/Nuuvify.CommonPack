#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente ci). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Valida o título do PR como Conventional Commit e resume o impacto na próxima release.

.DESCRIPTION
O título do PR vira a mensagem do commit squash na branch padrão. O script exige
`<tipo>[(escopo)][!]: <descrição>` com tipos aprovados, descrição iniciando com
minúscula e sem ponto final. Também informa se a mudança é releasable e qual
bump causará no release-please, para o revisor confirmar se o tipo está correto.
Escopos fora de `commitScopes` em .github/ci-cd.json geram aviso, não erro.
Não acessa rede nem altera arquivos, exceto o resumo opcional indicado.

.PARAMETER Title
Título do PR. Use variável de ambiente para não interpolar o valor no script.

.PARAMETER Body
Corpo do PR, usado para detectar `BREAKING CHANGE:` e `Release-As:`.

.PARAMETER Author
Login do autor do PR. Títulos de dependabot[bot] podem iniciar a descrição com
"Bump", que o Dependabot sempre gera com maiúscula; tipo, escopo e ponto final
continuam validados.

.PARAMETER SummaryPath
Arquivo Markdown em que o resumo é acrescentado (por exemplo $env:GITHUB_STEP_SUMMARY).

.PARAMETER ConfigPath
Configuração do pipeline. Padrão: .github/ci-cd.json.

.PARAMETER ManifestPath
Manifesto do release-please com a versão atual. Padrão: .release-please-manifest.json.

.EXAMPLE
./tools/scripts/ci/Test-PullRequestTitle.ps1 -Title 'feat(cli): adicione comando github'

.EXAMPLE
./tools/scripts/ci/Test-PullRequestTitle.ps1 --help

.NOTES
Requer PowerShell 7+. Código de saída 0 para título válido, 1 para inválido e 2 para argumento desconhecido.

.LINK
../../../.github/CI-CD.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $Title,
    [string] $Body = '',
    [string] $Author = '',
    [string] $SummaryPath = '',
    [string] $ConfigPath = '.github/ci-cd.json',
    [string] $ManifestPath = '.release-please-manifest.json',
    [Alias('h')]
    [switch] $Help,
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $RemainingArguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Help -or ($RemainingArguments | Where-Object { $_ -in @('--help', '-h', '/?', 'help') })) {
    Get-Help -Full $PSCommandPath
    exit 0
}

if ($RemainingArguments) {
    [Console]::Error.WriteLine("Argumento desconhecido: $($RemainingArguments -join ' '). Use --help.")
    exit 2
}

$types = @{
    feat     = 'minor'
    fix      = 'patch'
    deps     = 'patch'
    perf     = 'patch'
    revert   = 'patch'
    docs     = 'none'
    style    = 'none'
    refactor = 'none'
    test     = 'none'
    build    = 'none'
    ci       = 'none'
    chore    = 'none'
}
$automationScopes = @('deps', 'deps-dev', 'actions', 'main')

$pattern = '^(?<type>[a-z]+)(\((?<scope>[a-z0-9][a-z0-9._/-]*)\))?(?<breaking>!)?: (?<description>\S.*)$'
$match = [regex]::Match($Title ?? '', $pattern)
$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

if (-not $match.Success) {
    $errors.Add('Use <tipo>[(escopo)][!]: <descrição> (Conventional Commits 1.0.0).')
}
else {
    $type = $match.Groups['type'].Value
    $scope = $match.Groups['scope'].Value
    $description = $match.Groups['description'].Value
    if (-not $types.ContainsKey($type)) {
        $errors.Add("Tipo '$type' não aprovado. Use: $((@($types.Keys) | Sort-Object) -join ', ').")
    }

    $isDependabotBump = $Author -eq 'dependabot[bot]' -and $description -cmatch '^Bump '
    if ($description -cmatch '^\p{Lu}' -and -not $isDependabotBump) {
        $errors.Add('A descrição deve começar com letra minúscula.')
    }

    if ($description.TrimEnd().EndsWith('.')) {
        $errors.Add('A descrição não termina com ponto final.')
    }

    if ($scope -and (Test-Path -LiteralPath $ConfigPath -PathType Leaf)) {
        $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json -AsHashtable
        $approved = @($config['commitScopes'] ?? @())
        if ($approved.Count -gt 0 -and $scope -notin $approved -and $scope -notin $automationScopes) {
            $warnings.Add("Escopo '$scope' não está em commitScopes de $ConfigPath.")
        }
    }
}

if ($errors.Count -gt 0) {
    foreach ($message in $errors) {
        Write-Output "::error title=Título do PR::$message"
    }

    if ($SummaryPath) {
        @('## Título do PR', '', "Título inválido: ``$Title``", '') + ($errors | ForEach-Object { "- $_" }) |
            Add-Content -LiteralPath $SummaryPath -Encoding utf8
    }

    exit 1
}

$breaking = $match.Groups['breaking'].Success -or ($Body -match '(?m)^BREAKING[ -]CHANGE: ')
$releaseAs = [regex]::Match($Body, '(?mi)^Release-As:\s*(?<version>\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?)\s*$')
$preMajor = $false
if (Test-Path -LiteralPath $ManifestPath -PathType Leaf) {
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json -AsHashtable
    $current = [string]($manifest['.'] ?? '')
    $preMajor = $current -match '^0\.'
}

$bump = $types[$type]
if ($breaking) {
    $bump = $preMajor ? 'minor (breaking antes de 1.0)' : 'major'
}

if ($releaseAs.Success) {
    $bump = "versão explícita $($releaseAs.Groups['version'].Value) (Release-As)"
}

$releasable = $bump -ne 'none'
foreach ($message in $warnings) {
    Write-Output "::warning title=Título do PR::$message"
}

$impact = if ($releasable) {
    "Releasable: entra no Release PR com bump **$bump**. A release só ocorre após o merge humano do Release PR."
}
else {
    'Não releasable: apenas integra na branch padrão; o Release PR não é aberto nem alterado.'
}
Write-Output "Título válido ($type). $impact"
if ($SummaryPath) {
    @(
        '## Impacto na release'
        ''
        "| Título | Releasable | Bump |"
        '| --- | --- | --- |'
        "| ``$Title`` | $($releasable ? 'sim' : 'não') | $bump |"
        ''
        'Confirme se o tipo descreve a mudança: alteração de comportamento classificada como `refactor` deve ser `fix` ou `feat`.'
    ) + ($warnings | ForEach-Object { "- Aviso: $_" }) | Add-Content -LiteralPath $SummaryPath -Encoding utf8
}

exit 0
