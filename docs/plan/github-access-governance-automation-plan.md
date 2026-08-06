# Plano de automação da governança de acesso no GitHub

## Gestão de Status

| Campo | Valor |
|---|---|
| Status | Em andamento |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsáveis | lzocateli e eusener |
| Última revisão | 2026-08-06 |

### Histórico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | N/A -> Rascunho | Criação do plano para uso posterior |
| 2026-08-06 | Rascunho -> Rascunho | Inclusão de eusener como corresponsável pelo plano |
| 2026-08-06 | Rascunho -> Em andamento | Início da implementação com dois owners e administradores protegidos |

## Objetivo

Criar uma automação idempotente em PowerShell usando `gh api`, com modo de auditoria padrão e aplicação explícita, para:

- manter somente `lzocateli` e `eusener` como owners da organização `nuuvify`;
- manter somente `lzocateli` e `eusener` com administração total do repositório `nuuvify/Nuuvify.CommonPack`;
- centralizar colaboradores no time `collaborators`, com acesso `Write` somente ao repositório `Nuuvify.CommonPack`;
- remover acessos humanos diretos não declarados;
- restringir privilégios gerais dos membros da organização.

## Decisões

- Usar PowerShell 7 e GitHub CLI, sem dependências adicionais.
- Executar somente auditoria por padrão; alterações exigirão `-Apply`.
- Exigir confirmação separada e explícita para rebaixar qualquer owner fora da lista protegida.
- Sincronizar membros do time por arquivo JSON versionado.
- Definir a permissão-base da organização como `None`.
- Restringir aos owners a criação de repositórios e times.
- Preservar `@lzocateli` como único CODEOWNER; `lzocateli` e `eusener` serão os únicos maintainers do time.
- Auditar GitHub Apps e privilégios herdados do Enterprise, sem removê-los automaticamente.

## Etapas de implementação

1. Criar `tools/scripts/github_access_policy.json` como política declarativa, contendo:
   - organização `nuuvify`;
   - owners e administradores protegidos `lzocateli` e `eusener`;
   - time `collaborators`;
   - membros autorizados com papel `member`;
   - repositório `Nuuvify.CommonPack`;
   - permissão `push`, correspondente a `Write`;
   - controles organizacionais restritos;
   - lista explícita de exceções permitidas, quando necessária.

2. Criar `tools/scripts/set-github-access-policy.ps1` com os parâmetros:
   - `-PolicyPath` para indicar o arquivo de política;
   - `-Apply` para aplicar correções;
   - `-ConfirmOwnerDemotion` para permitir o rebaixamento controlado de owners extras.
   - `-ValidateOnly` para validar localmente a política sem exigir `gh`.

3. Antes de qualquer mutação, validar:
   - execução com PowerShell 7;
   - presença do comando `gh`;
   - autenticação válida do GitHub CLI;
   - identidade autenticada igual a `lzocateli` ou `eusener`;
   - papel de owner na organização;
   - acesso administrativo ao repositório;
   - permissões suficientes do token para organização e repositório.

4. Implementar uma camada pequena de chamadas REST sobre `gh api`, com:
   - paginação;
   - corpos JSON estruturados;
   - tratamento consistente de respostas `403`, `404` e `422`;
   - mensagens que não exponham tokens;
   - operações idempotentes;
   - estados de relatório `OK`, `DRIFT`, `CHANGED` e `BLOCKED`.

5. Auditar e reconciliar privilégios da organização por `PATCH /orgs/nuuvify`:
   - `default_repository_permission=none`;
   - impedir criação de repositórios públicos por membros;
   - impedir criação de repositórios privados por membros;
   - impedir criação de repositórios internos por membros, quando aplicável;
   - impedir criação de times por membros.

6. Auditar owners por `GET /orgs/nuuvify/members?role=admin` e proteger estas invariantes:
   - `lzocateli` e `eusener` devem estar ativos como owners;
   - nenhum dos dois pode ser removido ou rebaixado;
   - qualquer outro owner deve gerar `DRIFT`;
   - rebaixamento para `member` deve exigir `-Apply`, `-ConfirmOwnerDemotion` e confirmação nominal;
   - execução não interativa deve exigir um valor explícito de confirmação.

7. Auditar funções organizacionais delegadas:
   - remover atribuições de organization roles de terceiros quando aplicável;
   - preservar somente atribuições administrativas de `lzocateli` e `eusener`;
   - detectar permissões herdadas do Enterprise e retornar `BLOCKED` com sua origem;
   - auditar billing managers, security managers, GitHub App managers e instalações de GitHub Apps;
   - não desinstalar Apps automaticamente.

8. Criar ou reconciliar o time `collaborators`:
   - privacidade `secret`;
   - notificações habilitadas;
   - `lzocateli` e `eusener` como únicos maintainers;
   - demais logins declarados no JSON como `member`;
   - convites pendentes exibidos no relatório;
   - membros não declarados removidos quando usado `-Apply`.

9. Conceder ao time somente `Write` no repositório por:

   `PUT /orgs/nuuvify/teams/collaborators/repos/nuuvify/Nuuvify.CommonPack`

10. Auditar todos os times com acesso ao repositório:
    - remover concessões não declaradas quando usado `-Apply`;
    - impedir permissões `admin` ou `maintain` para o time de colaboradores;
    - preservar somente integrações expressamente autorizadas na política.

11. Auditar colaboradores diretos e outside collaborators:
   - preservar grants administrativos somente de `lzocateli` e `eusener`;
   - remover grants administrativos diretos dos demais quando usado `-Apply`;
   - manter colaboradores declarados com `Write` por meio do time;
   - verificar a permissão efetiva de cada membro autorizado.

12. Executar verificação completa após qualquer aplicação. O comando somente deve terminar com sucesso quando:
   - existirem exatamente dois owners, `lzocateli` e `eusener`;
   - apenas `lzocateli` e `eusener` tiverem administração efetiva do repositório;
   - o time tiver `Write` no repositório;
   - os membros do time coincidirem com a política;
   - não houver grants humanos diretos de terceiros;
   - as políticas organizacionais estiverem restritas;
   - não houver bloqueios ou fontes de acesso administrativo desconhecidas.

13. Documentar a operação em `docs/maintainers/github-access-governance.md`, incluindo:
    - pré-requisitos e permissões do token;
    - modo de auditoria;
    - modo de aplicação;
    - confirmação de rebaixamento de owner;
    - inclusão e remoção de colaboradores pelo JSON;
    - interpretação de `BLOCKED`;
    - rollback;
   - continuidade operacional com exatamente dois owners.

14. Referenciar o guia operacional em `docs/maintainers/github-setup.md`, sem duplicar seu conteúdo.

## Arquivos previstos

- `tools/scripts/set-github-access-policy.ps1`: auditoria e reconciliação via `gh api`.
- `tools/scripts/github_access_policy.json`: estado desejado de owner, time, membros e permissões.
- `docs/maintainers/github-access-governance.md`: operação, segurança, validação e rollback.
- `docs/maintainers/github-setup.md`: referência curta para o procedimento.
- `.github/CODEOWNERS`: preservar `@lzocateli` como único owner de código.
- `.github/CONTRIBUTING.md`: revisar somente se for necessário documentar o fluxo interno por time.

## Verificação

1. Validar a sintaxe do script com o parser do PowerShell.
2. Validar o JSON com `ConvertFrom-Json`.
3. Executar sem `-Apply` e confirmar que nenhuma chamada mutável foi realizada.
4. Executar `-Apply` sem confirmação de owner e confirmar que owners extras não foram rebaixados.
5. Testar o rebaixamento somente em cenário controlado, com confirmação nominal.
6. Executar novamente em modo de auditoria e exigir apenas estados `OK`.
7. Conferir na interface do GitHub:
   - `Organization settings > People`: exatamente `lzocateli` e `eusener` como owners;
   - `Teams > collaborators`: ambos como únicos maintainers e membros declarados;
   - `Repository > Settings > Collaborators and teams`: time com `Write` e somente os dois protegidos com `Admin`;
   - `Organization settings > Member privileges`: políticas restritas.
8. Cobrir falhas de autenticação, token insuficiente, usuário inexistente, convite pendente, time sincronizado por IdP, papel herdado do Enterprise e App com privilégio administrativo.

## Critérios de conclusão

- O script é idempotente e sua segunda execução não produz mutações.
- O modo de auditoria nunca altera recursos.
- Alterações destrutivas exigem consentimento explícito.
- A política declarativa é a fonte de verdade dos colaboradores.
- Divergências não corrigíveis retornam código de saída não zero e estado `BLOCKED`.
- A documentação permite executar, validar e reverter a automação sem consultar seu código interno.

## Limites e riscos

- A política exige exatamente dois owners, `lzocateli` e `eusener`, para continuidade. Ambos devem manter 2FA, passkey ou chave de segurança, códigos de recuperação offline e procedimento de recuperação da conta.
- Nenhum script garante exclusividade contra um enterprise owner, IdP/SCIM, GitHub App ou política superior que reintroduza acesso. Esses casos devem ser detectados e reportados como `BLOCKED`.
- A automação não gerenciará branch protections, environments, secrets, billing nem desinstalação automática de GitHub Apps nesta entrega.
