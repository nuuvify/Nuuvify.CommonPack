# Guia de IA do repositório

Este diretório concentra a estratégia de uso de IA para o Nuuvify.CommonPack com foco em baixo consumo de contexto, previsibilidade técnica e manutenção de bibliotecas .NET.

## Objetivos

- reduzir tokens sempre ativos
- carregar contexto apenas quando a tarefa pedir
- manter mudanças locais ao pacote dono
- preservar compatibilidade pública, testes e documentação

## Estrutura criada

- `.github/copilot-instructions.md`: regras globais e curtas, válidas para qualquer tarefa
- `.github/instructions/`: instruções específicas por tipo de trabalho
- `.github/agents/`: agentes especializados com ferramentas restritas por tipo de tarefa
- `.github/prompts/`: prompts reutilizáveis para tarefas recorrentes
- `.github/skills/library-maintenance/`: skill para fluxo completo de manutenção de bibliotecas

## Descoberta do Copilot

Para customizações compartilhadas do workspace, considere estas superfícies como oficiais:

- prompts descobríveis em `.github/prompts/`
- instructions descobríveis em `.github/instructions/`
- agents descobríveis em `.github/agents/`

Não trate arquivos dentro de `src/<Pacote>/` como superfície padrão de descoberta de prompts do Copilot. Conteúdo local do pacote continua útil como documentação e contexto humano, mas a descoberta automática de prompts e instructions do workspace deve permanecer centralizada em `.github/`.

## Quando usar cada recurso

### Instruções globais

Use sempre que a tarefa envolver o repositório como um todo. Elas devem permanecer curtas para não consumir contexto fixo além do necessário.

### Instruções sob demanda

Use para carregar regras específicas somente quando a tarefa realmente exigir:

Importante: `instructions` normalmente não são chamadas com `/`. Elas entram por relevância da tarefa e, principalmente, por `applyTo` quando o arquivo aberto ou alterado cai no escopo da instrução.

- `library-source.instructions.md`: criação e alteração de código em `src/`
- `library-tests.instructions.md`: testes em `test/`
- `package-docs.instructions.md`: `README`, `CHANGELOG` e docs
- `library-refactoring.instructions.md`: refatorações
- `library-review.instructions.md`: revisão técnica

Para pacotes críticos, use instructions específicas por caminho quando o contexto do arquivo justificar:

- `unitofwork-source.instructions.md`: regras adicionais para `UnitOfWork` e `UnitOfWork.Abstraction`
- `backgroundservice-source.instructions.md`: regras adicionais para `BackgroundService`
- `security-source.instructions.md`: regras adicionais para `Security`, `JwtCredentials` e `JwtStore.Ef`

#### O que significa `applyTo`

`applyTo` é o campo que define em quais arquivos uma `instruction` deve ser aplicada automaticamente.

Pense nele como um filtro por caminho:

- se o arquivo da tarefa combinar com o padrão, a instruction é relevante
- se o arquivo não combinar, a instruction não deve entrar só porque existe no repositório

Exemplos deste repositório:

```md
applyTo: "src/**/*.cs, src/**/*.csproj"
```

Leitura prática:

- vale para código C# dentro de `src/`
- vale para arquivos de projeto dentro de `src/`
- é o caso de `library-source.instructions.md`

```md
applyTo: "Readme.md, docs/**/*.md, src/**/README.md, src/**/CHANGELOG.md"
```

Leitura prática:

- vale para a documentação principal do repositório
- vale para arquivos Markdown em `docs/`
- vale para `README.md` e `CHANGELOG.md` dos pacotes
- é o caso de `package-docs.instructions.md`

```md
applyTo: "src/Nuuvify.CommonPack.UnitOfWork/**/*.cs, src/Nuuvify.CommonPack.UnitOfWork.Abstraction/**/*.cs, src/Nuuvify.CommonPack.UnitOfWork/**/*.csproj, src/Nuuvify.CommonPack.UnitOfWork.Abstraction/**/*.csproj"
```

Leitura prática:

- vale só para arquivos do pacote `UnitOfWork`
- adiciona regras mais específicas que a instruction genérica de `src/**`
- é o caso de `unitofwork-source.instructions.md`

Resumo rápido:

- `applyTo` não é comando
- `applyTo` não usa slash
- `applyTo` define escopo automático da instruction
- prompt é chamado explicitamente; instruction entra por contexto

### Prompts

Use para padronizar pedidos recorrentes sem reescrever contexto em toda conversa:

Ao contrário de `instructions`, prompts podem ser chamados de forma explícita pelo usuário. No VS Code, isso normalmente acontece por slash command, como `/uow-plan-change`, quando o prompt está disponível no chat.

Prompts genéricos:

- `plan-library-change.prompt.md`
- `implement-library-change.prompt.md`
- `refactor-library-code.prompt.md`
- `generate-library-tests.prompt.md`
- `review-library-change.prompt.md`
- `update-package-docs.prompt.md`

Prompts especializados por pacote crítico:

- `uow-plan-change.prompt.md`
- `uow-implement-change.prompt.md`
- `uow-refactor-code.prompt.md`
- `uow-generate-tests.prompt.md`
- `uow-review-change.prompt.md`
- `uow-update-docs.prompt.md`
- `bgs-plan-change.prompt.md`
- `bgs-implement-change.prompt.md`
- `bgs-refactor-code.prompt.md`
- `bgs-generate-tests.prompt.md`
- `bgs-review-change.prompt.md`
- `bgs-update-docs.prompt.md`
- `sec-plan-change.prompt.md`
- `sec-implement-change.prompt.md`
- `sec-refactor-code.prompt.md`
- `sec-generate-tests.prompt.md`
- `sec-review-change.prompt.md`
- `sec-update-docs.prompt.md`

Use os prompts genéricos quando a tarefa ainda não estiver claramente localizada em um pacote. Prefira os prompts especializados quando a mudança estiver concentrada em um destes pacotes sensíveis:

- `Nuuvify.CommonPack.UnitOfWork`
- `Nuuvify.CommonPack.BackgroundService`
- `Nuuvify.CommonPack.Security`

Os prompts especializados reduzem duplicação de contexto e deixam explícitos os riscos centrais de cada pacote, enquanto os genéricos continuam úteis para triagem inicial e fluxos transversais.

Na prática, a especialização por pacote agora cobre:

- planejamento
- implementação
- refatoração
- geração de testes
- revisão
- atualização de documentação

### Agentes

Use quando o tipo de tarefa pedir restrição explícita de ferramentas:

- `library-reviewer.agent.md`: revisão somente leitura com `read` e `search`
- `library-refactorer.agent.md`: refatoração com `read`, `search`, `edit` e `execute`

### Skill

Use `library-maintenance` quando a tarefa combinar várias etapas, como entender o pacote, implementar, testar, revisar e documentar.

## Seleção de modelo

- `GPT-5`: padrão para implementação, análise estrutural, correção, refatoração e geração de testes.
- `Claude Sonnet 4.5`: preferível para revisão ampla e síntese documental longa, quando disponível.
- Fallback prático: se o modelo preferido não estiver disponível, continue em `GPT-5` com escopo menor e contexto mais preciso.

## Estratégia de economia de tokens

- Comece por um arquivo, símbolo, teste ou pacote específico.
- Leia primeiro o código que decide o comportamento, não toda a solução.
- Use `README.md` e `CHANGELOG.md` do pacote antes de abrir documentação global.
- Separe planejamento, implementação, testes e revisão por prompt quando a tarefa for longa.
- Quando a tarefa estiver dentro de `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote antes do prompt genérico equivalente.
- Use instructions com `applyTo` para especialização automática por caminho, em vez de repetir o mesmo checklist em todos os prompts.
- Para revisão e refatoração recorrentes, prefira os agentes especializados em vez do agente genérico.
- Evite instruções globais extensas; mova detalhes para `instructions`, `prompts` ou `skills`.

## Fluxo recomendado

1. Identifique se a tarefa é transversal ou localizada em um pacote.
2. Se for transversal, comece pelos prompts genéricos.
3. Se for `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote.
4. Deixe as instructions por caminho completar automaticamente o contexto técnico do pacote.
5. Use agentes especializados quando a tarefa pedir revisão somente leitura ou refatoração controlada.

## Exemplo prático para um dev júnior

Hipótese de tarefa:

> Preciso implementar paginação em consultas geradas pelo EF Core específicas para banco IBM DB2, e limitar a paginação máxima para todos os bancos para `100` caso o usuário informe um valor maior que `100` para a quantidade por página.

Forma correta de pensar a tarefa neste repositório:

1. Primeiro classifique o problema no pacote dono do comportamento. Neste caso, o ponto de partida natural é `Nuuvify.CommonPack.UnitOfWork`, porque a mudança envolve `IQueryable`, paginação, tradução de consulta e possível compatibilidade entre providers.
2. Depois escolha o prompt especializado. O mais adequado seria `uow-plan-change.prompt.md` para desenhar a alteração com pouco contexto desperdiçado. Se a decisão já estiver clara, use `uow-implement-change.prompt.md`.
3. As instructions relevantes não precisam ser chamadas com `/`. Elas entram quase sozinhas pelo caminho dos arquivos: `library-source.instructions.md` continua valendo para `src/**` e `unitofwork-source.instructions.md` acrescenta os cuidados de tradução LINQ, paginação, filtros e compatibilidade com provider.
4. Se a tarefa envolver mais de uma etapa, como entender o comportamento atual, alterar código, criar testes e revisar impacto, use a skill `library-maintenance`. Ela é útil quando o fluxo não é só uma edição rápida.
5. Se depois da implementação você quiser uma revisão técnica sem editar nada, use o agent `library-reviewer`. Se precisar de uma refatoração controlada preservando comportamento, use `library-refactorer`.

Exemplo de uso esperado:

- planejamento inicial: usar `uow-plan-change.prompt.md` para pedir a identificação do ponto de extensão atual da paginação, o risco específico para DB2 e onde aplicar o limite global de `100`
- implementação: usar `uow-implement-change.prompt.md` apontando o arquivo ou símbolo que decide a paginação
- testes: validar cenários com quantidade solicitada menor, igual e maior que `100`, além de garantir que o comportamento específico de DB2 não quebre outros providers
- revisão: executar `uow-review-change.prompt.md` ou o agent `library-reviewer` para verificar regressões de API pública, tradução de query e compatibilidade com SemVer

Como esse pedido pode ser digitado no VS Code:

1. Abrir um arquivo do pacote, por exemplo um extension method ou serviço de paginação em `Nuuvify.CommonPack.UnitOfWork`.
2. No chat do Copilot, chamar o prompt especializado e complementar com o pedido. Exemplo:

```text
/uow-plan-change

No pacote Nuuvify.CommonPack.UnitOfWork, preciso ajustar a paginação de consultas EF Core.
Para IBM DB2, identifique a estratégia necessária para paginação funcionar corretamente.
Para todos os bancos, se o consumidor informar pageSize maior que 100, o valor efetivo deve ser 100.
Preserve compatibilidade de API pública, valide impacto em tradução LINQ e proponha testes proporcionais.
```

3. Quando a análise estiver clara, pedir a implementação com algo como:

```text
/uow-implement-change

Implemente a alteração no pacote Nuuvify.CommonPack.UnitOfWork.
Use o arquivo aberto como ponto de partida, mantenha compatibilidade pública e adicione ou atualize testes.
```

Observação: o slash acima chama o prompt. As `instructions` de `src/**` e de `UnitOfWork` continuam sendo aplicadas por contexto e `applyTo`, não por `/library-source` ou `/unitofwork-source`.

Como esse pedido pode ser digitado no Copilot CLI:

Se o dev estiver no diretório raiz do repositório, ele pode fazer um pedido equivalente no terminal:

```powershell
copilot chat "No pacote Nuuvify.CommonPack.UnitOfWork, preciso ajustar a paginação de consultas EF Core. Para IBM DB2, implemente a estratégia necessária para paginação funcionar corretamente. Para todos os bancos, se o consumidor informar pageSize > 100, o valor efetivo deve ser 100. Preserve compatibilidade de API pública, valide impacto em tradução LINQ e adicione testes proporcionais. Considere as instruções especializadas de UnitOfWork e siga o padrão do repositório."
```

No CLI, trate `instructions` da mesma forma: elas não são chamadas com `/`. O caminho mais seguro é descrever a tarefa com clareza e deixar o contexto do repositório e das instruções orientar a execução.

Se quiser separar planejamento de implementação também no CLI, o fluxo fica melhor assim:

```powershell
copilot chat "Planeje a mudança no pacote Nuuvify.CommonPack.UnitOfWork para suportar paginação correta em IBM DB2 e limitar pageSize máximo em 100 para todos os bancos. Aponte arquivos, riscos de tradução LINQ e testes necessários."
```

```powershell
copilot chat "Implemente a mudança planejada no pacote Nuuvify.CommonPack.UnitOfWork para suportar paginação correta em IBM DB2 e limitar pageSize máximo em 100 para todos os bancos. Preserve API pública e atualize testes relevantes."
```

Exemplo de pedido bem formulado para a IA:

> No pacote `Nuuvify.CommonPack.UnitOfWork`, preciso ajustar a paginação de consultas EF Core. Para IBM DB2, implemente a estratégia necessária para paginação funcionar corretamente. Para todos os bancos, se o consumidor informar `pageSize > 100`, o valor efetivo deve ser `100`. Preserve compatibilidade de API pública, valide impacto em tradução LINQ e adicione testes proporcionais.

O que esse exemplo ensina:

- não começar pela solução inteira, e sim pelo pacote dono do comportamento
- usar prompt especializado, inclusive por slash no VS Code quando disponível, para reduzir contexto e aumentar precisão
- deixar a instruction do pacote trazer automaticamente os riscos técnicos relevantes, sem tentar chamá-la por slash
- usar skill quando a tarefa combinar implementação, teste e revisão
- usar agent apenas quando houver necessidade clara de revisão somente leitura ou refatoração controlada

## Evolução do padrão

Ao adicionar um novo pacote crítico no futuro:

1. defina uma sigla curta e estável para o pacote
2. crie prompts especializados em `.github/prompts/` para planejamento, implementação e refatoração
3. crie uma instruction específica em `.github/instructions/` com `applyTo` restrito ao pacote
4. atualize este guia apenas depois que o padrão do novo pacote estiver estável

## Critérios de qualidade para IA neste repositório

- mudanças pequenas, locais e reversíveis
- contratos públicos estáveis
- testes proporcionais à mudança
- documentação atualizada quando houver impacto externo
- refatoração guiada por validação, não por estética
- `*.cs` e `*.csproj` mantidos em `UTF-8` e com fim de linha compatível com Linux, Windows e macOS, sempre seguindo o `.editorconfig`

## Referências existentes do projeto

- `docs/maintainers/architecture.md`
- `docs/contributing/testing.md`
- `Readme.md`
