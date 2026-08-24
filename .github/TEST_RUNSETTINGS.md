# Configuração de cobertura com test.runsettings.xml

O arquivo `test.runsettings.xml`, na raiz do repositório, configura a coleta de cobertura usada por `tools/scripts/Test-UnitExecute.ps1`.

## Quando criar

Crie o arquivo somente quando ele não existir. Não substitua nem descarte configurações existentes sem analisar os consumidores e obter confirmação para mudanças incompatíveis.

## Conteúdo padrão

Crie `test.runsettings.xml` na raiz com este conteúdo:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <DataCollectionRunSettings>
        <DataCollectors>
            <DataCollector friendlyName="XPlat code coverage">
                <Configuration>
                    <Format>cobertura</Format>
                    <Exclude>[coverlet.*.tests?]*,[*]Coverlet.Core*,[*.xTest]*</Exclude>
                    <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
                    <SingleHit>false</SingleHit>
                    <UseSourceLink>true</UseSourceLink>
                    <IncludeTestAssembly>true</IncludeTestAssembly>
                    <SkipAutoProps>true</SkipAutoProps>
                </Configuration>
            </DataCollector>
        </DataCollectors>
    </DataCollectionRunSettings>
</RunSettings>
```

## Validação

1. Confirme que o XML é válido:

```powershell
[xml](Get-Content ./test.runsettings.xml -Raw) | Out-Null
```

1. Execute um recorte de testes com cobertura:

```powershell
./tools/scripts/Test-UnitExecute.ps1 -TestCategory Unit -MinimumCoverage 0 -OpenReport:$false
```

1. Confirme a criação de `test/TestResults/Coverage/Report/Summary.txt`.

## Restrições

- Não adicione exclusões para elevar artificialmente a cobertura.
- Não exclua código de produção sem justificativa técnica e revisão.
- Mantenha o formato `cobertura`, consumido pelo ReportGenerator e pelos workflows do repositório.
- Atualize este guia e o script no mesmo ciclo quando o contrato do arquivo mudar.
