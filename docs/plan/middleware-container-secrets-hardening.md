# Plano de robustez para configuração e container secrets

## Gestão de Status

| Campo           | Valor                              |
| --------------- | ---------------------------------- |
| Status          | Concluído                          |
| Criado em       | 2026-08-20                         |
| Atualizado em   | 2026-08-28                         |
| Responsável     | Lincoln Zocateli                   |
| Última revisão  | 2026-08-28                         |

### Histórico de Status

| Data | De -> Para | Motivo |
| --- | --- | --- |
| 2026-08-20 | N/A -> Rascunho | Criação do plano para revisão e aprovação humana |
| 2026-08-20 | Rascunho -> Em andamento | Implementação iniciada e validada parcialmente |
| 2026-08-28 | Em andamento -> Concluído | Etapa 8 fechada: testes focados (11/11) e do pacote (42/42) aprovados, build e pack Release sem erros, inspeção do `.nupkg` confirmando README, CHANGELOG e XML docs, e smoke test em container Docker validando `AddContainerSecrets` (resolução de `Database__Password`, arquivos montados preservados, `optional=true`/`false` e fail-closed) |

> Plano concluído após validação completa da etapa 8, incluindo o smoke test
> em container. O plano de modernização relacionado
> (`middleware-modernization-compatibility.md`) já está concluído e publicado.

## Status de Execução

| Etapa | Status | Evidência |
| --- | --- | --- |
| 1. Testes de regressão | Concluída | Suíte focada adicionada |
| 2. Correção do método legado | Concluída | Captura em memória e remoção após captura |
| 3. API para container secrets | Concluída | `AddContainerSecrets` implementada |
| 4. Documentação XML | Concluída | Membros públicos tocados documentados |
| 5. README e migração | Concluída | README do pacote atualizado |
| 6. Changelogs | Concluída | Changelogs do pacote e raiz atualizados |
| 7. Auditoria de consumidores | Concluída | Consumidores revisados sem edição |
| 8. Validação final | Concluída | 11/11 testes focados, 42/42 testes do pacote, build e pack Release sem erros; `.nupkg` inspecionado com README, CHANGELOG e XML docs presentes; smoke test em container Docker aprovado (resolução `Database__Password` -> `Database:Password`, arquivos montados preservados, `optional=true` sem exceção e `optional=false` fail-closed com `DirectoryNotFoundException`) |

## Relação com a modernização posterior

Este plano limita-se ao hardening compatível já implementado. A evolução do
parser `.env`, a remoção da mutação global de `Environment`, a migração dos
consumidores e a ativação de `[Obsolete]` estão em
[`middleware-modernization-compatibility.md`](middleware-modernization-compatibility.md).

## Objetivo

Corrigir o defeito funcional e reduzir os riscos de segurança em
`ConfigurationBuilderExtensions` sem quebrar a API pública do pacote
`Nuuvify.CommonPack.Middleware`.

O método legado deixará de materializar variáveis de ambiente em arquivos
temporários e passará a registrar uma fonte em memória. Uma nova extensão
explícita carregará secrets já montados por Docker, Podman ou Kubernetes por
meio do provider oficial `KeyPerFile`.

## Diagnóstico

1. `AddEnvironmentVariablesToKeyPerFile` registra o provider e apaga o
   diretório temporário antes de `IConfigurationBuilder.Build()`.
2. Como a fonte é registrada com `optional: true`, o resultado pode ser uma
   configuração sem os secrets e sem erro, depois que as variáveis já foram
   removidas do processo.
3. O round-trip por `Path.GetTempPath()` grava secrets em texto puro sem
   necessidade e pode deixar resíduos em falhas ou encerramento abrupto.
4. `Environment.SetEnvironmentVariable(..., null)` remove a variável apenas do
   processo atual. Isso não elimina cópias no processo pai, metadados do
   container ou outras superfícies do runtime.
5. A mutação de variáveis de ambiente é global ao processo e deve ocorrer
   somente durante o startup, antes de fluxos concorrentes.
6. O uso de `__` em nomes de arquivos não é um defeito: o provider
   `KeyPerFile` converte esse delimitador em `:` para formar chaves
   hierárquicas.
7. O pacote tem projeto de testes, mas não há cobertura para essas extensões.
8. O `Readme.md` do pacote é um placeholder e sua capitalização diverge de
   `PackageReadmeFile=README.md`, o que pode afetar o pack em Linux.

## Escopo

- Pacote `src/Nuuvify.CommonPack.Middleware`.
- Testes em `test/Nuuvify.CommonPack.Middleware.xTest`.
- README e changelog do pacote.
- Changelog raiz do repositório.
- Auditoria somente leitura dos consumidores no repositório `CommonPack`.

## Fora de Escopo

- Adicionar dependências externas.
- Alterar manualmente a versão do pacote.
- Modificar consumidores em outros repositórios sem necessidade comprovada.
- Implementar um cofre de secrets ou criptografia em memória.
- Garantir rotação dinâmica para todos os orquestradores.
- Remover ou renomear APIs públicas neste ciclo.

## Etapas de Implementação

### Etapa 1 - Fixar o contrato com testes de regressão

Criar
`test/Nuuvify.CommonPack.Middleware.xTest/ConfigurationBuilderExtensionsTests.cs`.

Cobrir:

1. Prefixo nulo ou vazio retorna o mesmo builder sem adicionar fonte.
2. Filtro de prefixo usa comparação ordinal.
3. `removePrefix` nos estados `true` e `false`.
4. Transformação de `__` para `:`.
5. Valores vazios e logger opcional.
6. Configuração disponível depois de `Build()`.
7. `removeVariavel` nos estados `true` e `false`.
8. Ausência de fonte temporária `KeyPerFileConfigurationSource` no método
   legado corrigido.
9. Isolamento dos testes com prefixos únicos e restauração das variáveis em
   bloco `finally`.

Esta etapa bloqueia as etapas 2 e 3.

### Etapa 2 - Corrigir o método legado preservando compatibilidade

Refatorar `AddEnvironmentVariablesToKeyPerFile` sem alterar nome, parâmetros,
valores padrão ou retorno público:

1. Capturar variáveis correspondentes em uma única passagem ordinal.
2. Converter `__` para `:` antes de registrar a configuração.
3. Registrar os valores com `AddInMemoryCollection`.
4. Remover as variáveis do processo somente após a captura, quando
   `removeVariavel` for `true`.
5. Não escrever secrets em disco.
6. Não registrar nomes ou valores de secrets em logs.
7. Manter somente logs de contagem e contexto não sensível.
8. Compartilhar uma rotina privada com
   `AddEnvironmentVariablesToMemoryCollection` apenas se isso reduzir
   duplicação sem alterar o contrato observável.

### Etapa 3 - Adicionar API explícita para container secrets

Adicionar no mesmo arquivo:

```csharp
AddContainerSecrets(
    this IConfigurationBuilder builder,
    string directoryPath,
    bool optional = false,
    bool reloadOnChange = false)
```

Requisitos:

1. Delegar ao provider oficial `AddKeyPerFile`.
2. Validar `builder` e `directoryPath`.
3. Ser fail-closed por padrão com `optional=false`.
4. Usar `reloadOnChange=false` por padrão.
5. Não copiar, alterar ou apagar arquivos montados.
6. Documentar que arquivos com `__` formam chaves hierárquicas.
7. Documentar que permissões, montagem e rotação pertencem ao runtime ou
   orquestrador.

### Etapa 4 - Completar documentação XML

Documentar a classe pública e todos os membros públicos tocados em português
do Brasil, incluindo:

1. Responsabilidade e momento correto de uso durante startup.
2. Parâmetros, retorno e exceções diretas.
3. Efeitos de `removeVariavel` apenas no processo atual.
4. Limites de segurança de `IConfiguration`.
5. Comportamento fail-closed e reload da nova API.

Não adicionar `[Obsolete]` neste ciclo, pois consumidores podem tratar warnings
como erros. A depreciação deve ser avaliada separadamente.

### Etapa 5 - Documentar consumo e migração

Renomear `src/Nuuvify.CommonPack.Middleware/Readme.md` para `README.md` com
`git mv` em dois passos no Windows e substituir o placeholder por um README
adequado para NuGet.

Incluir:

1. Instalação e compatibilidade.
2. Uso de `AddContainerSecrets("/run/secrets")`.
3. Exemplos para Docker, Podman e Kubernetes.
4. Hierarquia de configuração por `__`.
5. Precedência dos providers.
6. Fail-closed e ausência de reload por padrão.
7. Limites de `IConfiguration` como armazenamento de secrets.
8. Migração do método legado.
9. Troubleshooting sem endpoints ou credenciais reais.

### Etapa 6 - Registrar impacto público

Atualizar as seções `[Não Lançado]` de:

- `src/Nuuvify.CommonPack.Middleware/CHANGELOG.md`.
- `CHANGELOG.md` na raiz.

Registrar:

- `Adicionado`: nova extensão `AddContainerSecrets`.
- `Corrigido`: carregamento silenciosamente vazio do método legado.
- `Segurança`: eliminação da persistência temporária de secrets.

Não criar seção de versão manualmente.

### Etapa 7 - Auditar consumidores

Revisar, sem ampliar automaticamente a mudança:

- `CommonPack/src/CBL.CommonPack.Api/Extensions/AzureServiceBuilderExtensions.cs`.
- `CommonPack/src/CBL.CommonPack.Worker/Extensions/AzureServiceBuilderExtensions.cs`.

Confirmar que variáveis como `AzureKeyVault__...` continuam produzindo a mesma
chave hierárquica. Registrar eventual migração futura para
`AddContainerSecrets`, sem editar o outro repositório neste ciclo.

### Etapa 8 - Validar e fechar

1. Executar testes focados:

   ```powershell
   dotnet test test/Nuuvify.CommonPack.Middleware.xTest/Nuuvify.CommonPack.Middleware.xTest.csproj --filter "FullyQualifiedName~ConfigurationBuilderExtensionsTests"
   ```

2. Executar todos os testes do pacote:

   ```powershell
   dotnet test test/Nuuvify.CommonPack.Middleware.xTest/Nuuvify.CommonPack.Middleware.xTest.csproj
   ```

3. Compilar o pacote:

   ```powershell
   dotnet build src/Nuuvify.CommonPack.Middleware/Nuuvify.CommonPack.Middleware.csproj --no-restore
   ```

4. Empacotar em Release e inspecionar o `.nupkg`:

   ```powershell
   dotnet pack src/Nuuvify.CommonPack.Middleware/Nuuvify.CommonPack.Middleware.csproj -c Release --no-restore
   ```

5. Confirmar no pacote a presença de `README.md`, `CHANGELOG.md` e XML docs.
6. Executar o gate amplo proporcional ao risco:

   ```powershell
   dotnet test --filter "Category=Unit"
   dotnet build Nuuvify.CommonPack.sln
   ```

7. Testar em Docker ou Podman com arquivos read-only em `/run/secrets`.
8. Confirmar resolução de `Database__Password` como `Database:Password`.
9. Confirmar que a aplicação não altera nem remove os arquivos montados.
10. Confirmar que nenhum valor sensível aparece em logs.
11. Testar diretório obrigatório ausente e `optional=true`.

## Critérios de Aceite

1. O método legado carrega os valores após `Build()` e não grava arquivos
   temporários.
2. Nenhum secret é registrado em log ou persistido pelo pacote.
3. As assinaturas públicas existentes permanecem compatíveis.
4. `AddContainerSecrets` lê diretamente o diretório montado e falha fechado por
   padrão.
5. Testes focados e testes do pacote passam.
6. Build e pack do pacote passam.
7. README, XML docs e changelogs refletem o comportamento público.
8. Consumidores conhecidos foram auditados quanto à compatibilidade.

## Riscos e Mitigações

- **Risco:** consumidores dependem da remoção process-global das variáveis.
  **Mitigação:** preservar `removeVariavel` e documentar seu alcance real.
- **Risco:** mudança de precedência de providers.
  **Mitigação:** testes de contrato e documentação explícita da ordem de
  registro.
- **Risco:** consumidores tratam warnings como erro.
  **Mitigação:** não marcar a API legada como obsoleta neste ciclo.
- **Risco:** permissões inadequadas no diretório montado.
  **Mitigação:** manter ownership e permissões sob responsabilidade do runtime e
  documentar requisitos mínimos.
- **Risco:** expectativa incorreta de rotação dinâmica.
  **Mitigação:** manter `reloadOnChange=false` por padrão e não prometer suporte
  uniforme entre orquestradores.

## Decisões

1. Preservar as APIs públicas atuais para uma correção compatível com
   patch/minor.
2. Não renomear `removeVariavel` neste ciclo.
3. Capturar variáveis legadas em memória, nunca em arquivos temporários.
4. Ler secrets montados diretamente pelo provider oficial.
5. Não adicionar dependências.
6. Não alterar a versão manualmente.
7. Tratar `IConfiguration` como configuração, não como cofre: os valores ainda
   existirão na memória do processo consumidor.

## Arquivos Relevantes

- `src/Nuuvify.CommonPack.Middleware/Extensions/ConfigurationBuilderExtensions.cs`.
- `test/Nuuvify.CommonPack.Middleware.xTest/ConfigurationBuilderExtensionsTests.cs`.
- `src/Nuuvify.CommonPack.Middleware/README.md`.
- `src/Nuuvify.CommonPack.Middleware/CHANGELOG.md`.
- `CHANGELOG.md`.
- `src/Nuuvify.CommonPack.Middleware/Nuuvify.CommonPack.Middleware.csproj`.

## Considerações Futuras

As evoluções posteriores são controladas pelo plano
[`middleware-modernization-compatibility.md`](middleware-modernization-compatibility.md),
incluindo depreciação compatível na linha 2.x, remoção física apenas na próxima
major e avaliação de rotação dinâmica em orquestradores.
