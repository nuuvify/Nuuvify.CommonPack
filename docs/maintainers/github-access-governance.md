# Governança de acesso no GitHub

Este procedimento mantém `lzocateli` e `eusener` como os únicos owners da organização `nuuvify` e os únicos administradores do repositório `Nuuvify.CommonPack`. Colaboradores recebem `Write` exclusivamente pelo time `collaborators`.

## Pré-requisitos

- PowerShell 7 ou superior.
- GitHub CLI disponível no `PATH`.
- Autenticação no `gh` como `lzocateli` ou `eusener`.
- Token com acesso administrativo à organização, aos membros, aos times e ao repositório.
- 2FA e códigos de recuperação protegidos para os dois owners.

Valide a autenticação antes da auditoria:

```powershell
gh auth status --hostname github.com
```

## Política declarativa

Edite `tools/scripts/github_access_policy.json` para declarar os membros do time. Não remova `lzocateli` ou `eusener` de `protectedAdministrators` nem de `maintainers`.

Valide o arquivo localmente, sem acessar o GitHub:

```powershell
./tools/scripts/set-github-access-policy.ps1 -ValidateOnly
```

## Auditoria

O modo padrão é somente leitura:

```powershell
./tools/scripts/set-github-access-policy.ps1
```

Resultados possíveis:

- `OK`: estado atual aderente.
- `DRIFT`: divergência detectada e não alterada.
- `CHANGED`: divergência corrigida com `-Apply`.
- `BLOCKED`: acesso herdado, integração administrativa ou correção que exige intervenção.

Os códigos de saída são `0` para conformidade, `1` para drift em auditoria e `2` para bloqueio.

## Aplicação

Revise todo o relatório de auditoria antes de executar:

```powershell
./tools/scripts/set-github-access-policy.ps1 -Apply
```

Owners fora da política não são rebaixados pelo comando anterior. Para autorizá-los explicitamente:

```powershell
./tools/scripts/set-github-access-policy.ps1 -Apply -ConfirmOwnerDemotion
```

O script solicitará os logins exibidos, em ordem alfabética e separados por vírgula. Em execução não interativa, informe o mesmo valor em `-OwnerDemotionConfirmation`.

Após aplicar, execute novamente sem `-Apply`. A operação está concluída somente quando não houver `DRIFT` nem `BLOCKED`.

## Limites e rollback

O script não altera políticas do Enterprise, IdP/SCIM, billing managers, security managers nem desinstala GitHub Apps. Esses casos exigem revisão manual.

Para reverter uma alteração, restaure o JSON versionado e execute novamente com `-Apply`. Se um owner tiver sido rebaixado indevidamente, o outro owner protegido deve promovê-lo imediatamente pela interface do GitHub antes de qualquer nova execução.
