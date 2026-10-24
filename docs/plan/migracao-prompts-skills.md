# Nuuvify.CommonPack — plano de migração de prompts para skills

## Gestao de Status

| Campo | Valor |
| --- | --- |
| Status | Rascunho |
| Criado em | 2026-10-22 |
| Atualizado em | 2026-10-22 |
| Responsavel | a designar pelo solicitante |
| Ultima revisao | N/A |
| Origem | IA assistida |
| Revisor humano | pendente de designação e revisão |
| Autorização | somente planejamento; migração não autorizada |
| Projeto | `/home/u_zocatel/projetos/Nuuvify.CommonPack` |

### Historico de Status

| Data | De -> Para | Motivo |
| --- | --- | --- |
| 2026-10-22 | N/A -> Rascunho | pedido de planos para revisão |

Plano salvo em `docs/plan/migracao-prompts-skills.md`, conforme a política
local, para revisão e uso posterior. Sua persistência não autoriza execução.

## Compatibilidade e fontes

Conforme o [guia oficial de migração](https://code.visualstudio.com/docs/agent-customization/migrate-customizations#_convert-prompt-files-to-skills),
consultado em 2026-10-22, o harness Copilot não carrega `*.prompt.md`.
Pontes mínimas para skills são possíveis como compatibilidade Local, sem
procedimento duplicado, mas exigem teste real e não atendem ao Copilot.
A recomendação é migrar completamente, mantendo pontes somente se houver
necessidade Local explicitamente aprovada.

A conversão não preserva seleção de `agent`, `model` ou `tools`. Seguir o
[formato suportado de skills](https://code.visualstudio.com/docs/agent-customization/agent-skills)
e revisar os contratos de execução, sem presumir suporte aos campos antigos.

## Estratégia proposta

Há **26 prompts**: seis operações genéricas, seis para BackgroundService,
seis para Security, seis para UnitOfWork, um para README NuGet e um para
comentários de PR.

Criar **oito skills por operação**, não 26 cópias e não um único procedimento
gigante. Cada skill recebe o pacote/área como entrada e carrega as instruções
do proprietário apenas quando necessário.

Manter `nuuvify-maintenance` como ponto de descoberta/roteamento, com links às
oito skills. Remover dela procedimentos concorrentes e referências à
preferência por prompts especializados. A skill agregadora não deve ser
um passo obrigatório: a invocação direta de cada operação precisa funcionar.

Essa divisão preserva fronteiras relevantes: planejamento, revisão sem edição,
refatoração restrita e correção por comentários de PR têm controles diferentes.

## Mapeamento completo: 26 origens, oito destinos

Caminhos de origem relativos a `.github/prompts/`.
Destinos relativos a `.github/skills/`; são propostos, ainda não criados.

| # | Prompt atual | Destino |
| --- | --- | --- |
| 1 | `plan-nuuvify-change.prompt.md` | `nuuvify-plan-change/SKILL.md` |
| 2 | `bgs-plan-change.prompt.md` | mesmo destino, área BackgroundService |
| 3 | `sec-plan-change.prompt.md` | mesmo destino, área Security |
| 4 | `uow-plan-change.prompt.md` | mesmo destino, área UnitOfWork |
| 5 | `implement-nuuvify-change.prompt.md` | `nuuvify-implement-change/SKILL.md` |
| 6 | `bgs-implement-change.prompt.md` | mesmo destino, área BackgroundService |
| 7 | `sec-implement-change.prompt.md` | mesmo destino, área Security |
| 8 | `uow-implement-change.prompt.md` | mesmo destino, área UnitOfWork |
| 9 | `refactor-nuuvify-code.prompt.md` | `nuuvify-refactor-code/SKILL.md` |
| 10 | `bgs-refactor-code.prompt.md` | mesmo destino, área BackgroundService |
| 11 | `sec-refactor-code.prompt.md` | mesmo destino, área Security |
| 12 | `uow-refactor-code.prompt.md` | mesmo destino, área UnitOfWork |
| 13 | `generate-nuuvify-tests.prompt.md` | `nuuvify-generate-tests/SKILL.md` |
| 14 | `bgs-generate-tests.prompt.md` | mesmo destino, área BackgroundService |
| 15 | `sec-generate-tests.prompt.md` | mesmo destino, área Security |
| 16 | `uow-generate-tests.prompt.md` | mesmo destino, área UnitOfWork |
| 17 | `review-nuuvify-change.prompt.md` | `nuuvify-review-change/SKILL.md` |
| 18 | `bgs-review-change.prompt.md` | mesmo destino, área BackgroundService |
| 19 | `sec-review-change.prompt.md` | mesmo destino, área Security |
| 20 | `uow-review-change.prompt.md` | mesmo destino, área UnitOfWork |
| 21 | `update-package-docs.prompt.md` | `nuuvify-update-package-docs/SKILL.md` |
| 22 | `bgs-update-docs.prompt.md` | mesmo destino, área BackgroundService |
| 23 | `sec-update-docs.prompt.md` | mesmo destino, área Security |
| 24 | `uow-update-docs.prompt.md` | mesmo destino, área UnitOfWork |
| 25 | `create-package-readme-nuget.prompt.md` | `nuuvify-create-package-readme-nuget/SKILL.md` |
| 26 | `fix-github-pr-comments.prompt.md` | `nuuvify-fix-github-pr-comments/SKILL.md` |

## N1 — Contratos de entradas e especialização

Em cada destino, preservar as entradas exclusivas das origens:

| Operação | Entradas |
| --- | --- |
| Planejar | objetivo, comportamento esperado, pacote/símbolo/arquivo, provider e riscos pertinentes |
| Implementar | objetivo, comportamento esperado, restrições, arquivo/símbolo e validação desejada |
| Refatorar | pacote, símbolo/arquivo, cheiro de código, restrições e teste alvo |
| Gerar testes | arquivo/símbolo, cenário, categoria Unit/Integration, provider ou cenário de mensagem |
| Revisar | PR/diff/arquivo/símbolo, escopo e contexto de compatibilidade |
| Atualizar documentação | pacote, mudança, público-alvo e nível de detalhe |
| README NuGet | pacote, finalidade, API/setup público, exemplos e restrições de apresentação |
| Comentários de PR | URL completa, ou somente os dados faltantes de owner/repo/prNumber |

Resolver a área pelos caminhos/pacotes informados; se houver ambiguidade,
pedir esclarecimento. Não tratar pacotes Security.JwtCredentials e
Security.JwtStore.Ef como fora da especialização Security, nem
UnitOfWork.Abstraction como fora de UnitOfWork.

Referências canônicas, sem copiar suas regras:

- `.github/instructions/backgroundservice-source.instructions.md`;
- `.github/instructions/security-source.instructions.md`;
- `.github/instructions/unitofwork-source.instructions.md`;
- `.github/instructions/nuuvify-source.instructions.md`;
- `.github/instructions/nuuvify-tests.instructions.md`;
- `.github/instructions/package-docs.instructions.md`;
- `.github/instructions/package-readme-nuget.instructions.md`;
- `.github/instructions/targets-source.instructions.md`, quando aplicável.

Comparar as variantes especializadas linha a linha com as instruções antes de
excluir os prompts: qualquer regra exclusiva precisa ganhar proprietário
canônico, e não desaparecer sob a hipótese de que a instrução já a contém.
Criar referência adicional somente para conteúdo exclusivo que não pertença
às instruções existentes.

## N2 — Agentes, modelos e permissões

Os prompts atuais possuem seleção de modelo, não apenas uma recomendação
informal: usam GPT-5, e revisão/documentação usam fallback Claude Sonnet 4.5 /
GPT-5. A conversão para skill não preserva essa seleção.

| Fluxo | Contrato atual a revisar |
| --- | --- |
| Planejamento | `agent: plan`; impedir implementação ao planejar |
| Implementação, testes e documentação | `agent: agent`; preservar limites do pedido |
| Refatoração | `nuuvify-refactorer`; edição/validação local, sem web/subagentes |
| Revisão | `nuuvify-reviewer`; revisão realmente sem edição |
| Comentários de PR | `nuuvify-GitHub PR Fixer`; leitura/API, edição e validação conforme pedido |

O prompt de PR também declara `tools: [read, edit, search, execute, todo,
vscode_askQuestions]`. Não copiar essa lista para uma skill como se ela
restringisse ferramentas no Copilot. Confirmar equivalentes suportados pelo
harness e manter restrições nos agentes; revisar a disponibilidade de perguntas.

Decisão humana obrigatória: abandonar a seleção automática de modelos antigos
em favor da seleção atual do cliente ou definir mecanismo compatível. Não
fixar modelo de subagente nem criar obrigação textual impossível de cumprir.

As oito novas skills devem ser inicialmente explícitas:
`user-invocable: true` e `disable-model-invocation: true`.
Preservar a descoberta atual de `nuuvify-maintenance`, mas seu roteamento não
pode iniciar automaticamente os novos fluxos mutantes. O usuário deve solicitar
a operação; descoberta não é autorização.

## N3 — Remover redundância real

- `.github/skills/nuuvify-maintenance/SKILL.md`: eliminar preferência por prompts
  especializados e concentrar-se em descoberta e links aos donos por operação.
- `.github/skills/nuuvify-maintenance/references/playbooks.md`: reconciliar com
  as novas skills. Mover o procedimento para seu dono e deixar apenas índice,
  ou retirar o arquivo depois de atualizar todos os consumidores.
- Templates de CHANGELOG já estão em `package-docs.instructions.md`: a skill
  de documentação os referencia, sem copiar os templates de
  `update-package-docs.prompt.md`.
- A skill de documentação referencia a skill de README NuGet quando essa
  operação específica for solicitada, sem reproduzi-la.
- Não exigir carregamento dos 26 textos ou de todas as áreas a cada invocação.

## N4 — Fluxo de comentários de PR

- Preservar extração automática de owner/repo/número quando a URL completa vier
  no pedido; perguntar apenas o que faltar.
- Referenciar `.github/instructions/github-pr.instructions.md`.
- Preservar precedência de autenticação `GITHUB_LZOCATELI_TOKEN`, `GH_TOKEN`,
  `GITHUB_TOKEN`; nunca ler ou mostrar valores.
- Conferir os entry points existentes `uv run --project tools/scripts
  get-github-pr-comments` e `uv run --project tools/scripts
  update-github-pr-comments` antes de transportar a orientação.
- Não mudar projeto Python, caminhos de scripts ou scripts neste escopo; se
  a verificação indicar incompatibilidade, registrar bloqueio e decisão própria.
- Preservar agrupamento por arquivo/regra, prioridade funcional/contratual,
  validação focada e relatório de comentários tratados/pendentes.
- Distinguir correção local de publicação de comentário, resolução de thread
  ou push. A migração não autoriza efeitos remotos.

## Arquivos afetados e consumidores

| Arquivo ou conjunto | Ação futura |
| --- | --- |
| 26 prompts da tabela | preservar durante a transição; remover após aceite ou reduzir a pontes Local aprovadas |
| oito destinos da tabela | criar, com entradas e referências canônicas |
| `nuuvify-maintenance/SKILL.md` e `references/playbooks.md` | reconciliar roteamento e fonte única |
| `.github/agents/nuuvify-reviewer.agent.md` | revisar compatibilidade e restrição read-only, editar só se necessário |
| `.github/agents/nuuvify-refactorer.agent.md` | revisar bloqueio de web/subagentes, editar só se necessário |
| `.github/agents/nuuvify-github-pr-fixer.agent.md` | confirmar contrato do PR Fixer e compatibilidade de ferramentas antes de qualquer edição |
| `.github/prompts/update-package-docs.prompt.md` | atenção ao link interno para o prompt de README e aos templates duplicados |
| `docs/ai/README.md` | revisar orientação que ainda menciona prompts |
| `docs/maintainers/changelog-standardization-plan.md` | revisar vínculo/estado histórico; não reescrever histórico nem mover plano legado neste escopo |
| `.github/copilot-instructions.md`, `.github/README.md` se existente | conferir consumidores antes de editar; não criar arquivo apenas para a migração |

Uma referência histórica pode continuar identificando um arquivo antigo como
evidência; identificá-la como histórica, sem deixá-la orientar uma operação
atual. A política exige que planos físicos novos incorporados ao repositório
fiquem em `docs/plan/`; ajustes em planos legados fora dela exigem decisão e
movimentação no mesmo PR, portanto não devem ser feitos incidentalmente.

## Sequência e dependências

1. Aprovar oito destinos, política de modelos e fronteiras de agentes.
2. Registrar referência anterior dos prompts, skill existente e consumidores.
3. Migrar planejamento e revisão primeiro, com cenários sem escrita.
4. Migrar implementação, refatoração e testes após resolver agentes/permissões.
5. Migrar documentação e README juntos, eliminando templates duplicados.
6. Migrar PR separadamente, após confirmar entry points e efeitos remotos.
7. Reconciliar agregadora/playbooks e atualizar consumidores após os destinos.
8. Validar a matriz completa; só então remover as origens por diff revisado.

## Critérios de aceite

- 26 origens mapeadas exatamente uma vez; oito skills operacionais novas.
- Matriz de **24 casos básicos**: seis operações para geral, BackgroundService,
  Security e UnitOfWork, mais README e PR, totalizando **26 cenários de origem**.
- Casos adicionais: área ambígua, pacote de abstração, provider ausente,
  cancelamento/concorrência/dead-letter, claims/defaults seguros,
  paginação/ordenação/tradução LINQ conforme a área.
- Revisão não escreve; planejamento não implementa; refatoração mantém
  contrato e restrições de ferramentas.
- Nenhum fluxo escolhe modelo por frontmatter não suportado de skill.
- PR com URL completa não pede dados já disponíveis; simular autenticação
  ausente sem ler secrets e sem chamada remota mutante.
- Links e entry points resolvem; nenhum template/procedimento tem duas fontes.
- Invocação explícita funciona no Copilot com raiz Nuuvify correta.
- Se Local for mantido, cada ponte apenas indica operação/área e skill;
  não repete regras e passa teste real.

Não executar correção real de pacote ou de PR apenas para testar a conversão.
Usar fixtures/cópia descartável para cenários com escrita e indicar
`NOT RUN` onde não houver ambiente ou autorização.

## Recuperação e decisões pendentes

Se uma variante não tiver equivalência, não apagar sua fonte. Corrigir a skill
e revalidar a operação/área antes de avançar. Recuperar apenas o diff da
migração, sem reset e sem tocar alterações do usuário.

Para revisão: oito skills versus outro agrupamento; descarte dos prompts
após aceite; necessidade de pontes Local; seleção de agentes/modelos;
revisor humano e escopo de validação.

Nesta entrega, somente este plano foi adicionado ao repositório. Nenhuma
migração, instalação, teste .NET, coleta de comentários ou escrita remota
foi executada.
