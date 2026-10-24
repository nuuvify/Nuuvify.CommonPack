# Tipo de mudança

- [ ] Correção de bug
- [ ] Nova funcionalidade
- [ ] Refatoração
- [ ] Testes
- [ ] Documentação
- [ ] Breaking change

# Resumo

Descreva objetivamente o problema e a solução proposta.

# Impacto na release

Todo PR vai para `main`. O título em Conventional Commits define o impacto na próxima versão
(`feat` → minor, `fix`/`perf`/`revert`/`deps` → patch, `!` → major); veja
[docs/runbooks/release.md](../docs/runbooks/release.md).

- [ ] O título do PR segue Conventional Commits e reflete o impacto esperado na versão

# Checklist do autor

- [ ] Li o guia em `CONTRIBUTING.md`
- [ ] O código segue as convenções do projeto e o `.editorconfig`
- [ ] Executei build e os testes relevantes localmente
- [ ] Adicionei ou atualizei testes quando necessário
- [ ] Atualizei documentação e o `CHANGELOG.md` de cada pacote alterado (seção `## [Não Lançado]`)
- [ ] Avaliei impacto em compatibilidade, segurança e performance

# Evidências

Inclua logs, prints, cobertura, benchmark ou qualquer evidência útil para a revisão.

# Issues relacionadas

Use `Closes #123` ou `Related #123`, quando aplicável.

# Checklist do revisor

- [ ] A alteração está clara, focada e justificada
- [ ] O impacto técnico foi avaliado
- [ ] Os testes cobrem os cenários relevantes
- [ ] A documentação está coerente com a mudança
- [ ] O alvo do PR está alinhado ao canal de release esperado
