# Runbook: release

Arquivo gerenciado pelo Altivy.Groundwork (componente `release`). Não edite: mudanças vão
para o catálogo. Procedimentos específicos do projeto ficam fora deste arquivo.

Repositório: `nuuvify/Nuuvify.CommonPack` · branch padrão: `main`.

## 1. Integração não é release

O título do PR (squash) decide se a mudança **conta** para a próxima release; o humano decide
**quando** a release acontece, fazendo o merge do Release PR.

| Tipo do título | Releasable | Bump |
| --- | --- | --- |
| `feat` | sim | minor |
| `fix`, `fix(deps)`, `deps` | sim | patch |
| `perf`, `revert` | sim | patch |
| qualquer tipo com `!` ou rodapé `BREAKING CHANGE:` | sim | major (antes de 1.0: minor) |
| `docs`, `chore`, `ci`, `build`, `test`, `refactor`, `style` | não | — |

- Commits releasable apenas criam ou atualizam o Release PR; nenhuma tag, release,
  publicação ou deploy acontece no merge de um PR comum.
- O job `Conventional title` do `gw-pr-hygiene.yml` mostra no resumo do PR se a mudança é
  releasable e qual bump causará. Confirme o tipo antes de aprovar: mudança de comportamento
  classificada como `refactor` deve ser `fix` ou `feat`.
- Snapshots `X.Y.Z-preview.N` da branch padrão (quando habilitados) não são releases: não
  criam tag nem GitHub Release e nunca são aceitos pelo deploy.

## 2. Lançar uma versão

1. Abra o Release PR `chore(main): release X.Y.Z` criado pelo GitHub App:

   ```bash
   gh pr list --repo nuuvify/Nuuvify.CommonPack --head release-please--branches--main
   ```

2. Revise o `CHANGELOG.md` e a versão; aguarde o check `Result`.
3. Faça o merge (squash). O workflow `gw-release.yml`:
   - cria a tag `vX.Y.Z` e a GitHub Release em rascunho;
   - chama `publish.yml` no Environment `release` (aprove quando solicitado);
   - gera, atesta e anexa o `release-manifest.json`;
   - publica a GitHub Release somente se tudo acima teve sucesso;
   - chama `post-release.yml` com a release já pública (distribuição do projeto, quando houver).
4. Confirme:

   ```bash
   gh release view vX.Y.Z --repo nuuvify/Nuuvify.CommonPack
   gh attestation verify release-manifest.json --repo nuuvify/Nuuvify.CommonPack
   ```

## 3. Lançar sem commit releasable

Para forçar uma versão (por exemplo, relançar por motivo operacional), faça merge de um PR
cujo commit tenha o rodapé abaixo. Isso abre um Release PR, que ainda exige merge humano.

```text
chore: solicite release operacional

Release-As: X.Y.Z
```

## 4. Republicar artefatos ausentes

Se o publish falhou parcialmente, a release continua em rascunho. Depois de corrigir a causa
(credencial, registro indisponível), reexecute na **tag**:

```bash
gh workflow run gw-release.yml --repo nuuvify/Nuuvify.CommonPack --ref vX.Y.Z -f mode=republish
```

O modo `republish` valida a tag, reexecuta `publish.yml` de forma idempotente (envia só o que
falta, nunca sobrescreve artefato publicado), finaliza a release e executa `post-release.yml` de novo
com `republish=true`. Se o `release-manifest.json`
já existir, ele não é alterado: qualquer digest divergente faz o workflow falhar.

## 5. Corrigir uma release defeituosa

Nunca apague ou mova tags, nunca exclua GitHub Releases e nunca recompile artefatos de uma
versão publicada.

1. Abra a correção (Issue ou fluxo de bug do projeto) e faça merge de um PR `fix:` ou
   `revert:` (ambos releasable, bump patch).
2. Faça merge do Release PR resultante para publicar a versão corrigida.
3. Sinalize a versão defeituosa quando aplicável:
   - GitHub Release: edite as notas com um aviso e o link da versão corrigida
     (`gh release edit vX.Y.Z --notes-file notas.md`);
   - NuGet.org: marque a versão como *deprecated* no portal, indicando a substituta;
   - imagens: mantenha a tag publicada e comunique a versão corrigida.

## 6. Falhas comuns

| Sintoma | Causa provável | Ação |
| --- | --- | --- |
| `Require release GitHub App` falha | `vars.RELEASE_APP_ID` ou `RELEASE_APP_PRIVATE_KEY` ausentes | Configure-os manualmente; nunca use PAT pessoal |
| Release PR sem checks | PR criado com `GITHUB_TOKEN` | Confirme que o App está instalado; o token do App dispara os checks |
| Tag recusada | ruleset `gw-release-tags` sem o App no bypass | Execute `groundwork github` e revise `release_app_id` |
| `Assert-ReleaseTag.ps1` falha | versão divergente entre tag e arquivos de versão | Corrija no Release PR; não crie tags manualmente |
| Release continua em rascunho | publish ou manifesto falhou | Corrija a causa e use o modo `republish` |
| `Failed to persist attestation: Feature not available` | Atestação indisponível no plano para repositório privado | Defina `"attestations": false` em `.github/ci-cd.json` (ver `.github/CI-CD.md`); releases assim não são implantáveis |
| Release PR bloqueado com um `Result` em falha e outro verde | Execução do CI cancelada no mesmo commit; o ruleset considera todas as execuções do check | Reexecute a execução cancelada (`gh run rerun <id>`); o `ci.yml` atual não cancela execuções de PR |
