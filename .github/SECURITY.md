# Política de Segurança

## Versões com suporte

O projeto prioriza correções de segurança nas versões mais recentes publicadas no NuGet e no branch `main`.

| Linha de versão                         | Suporte de segurança |
| --------------------------------------- | -------------------- |
| Última major estável                    | Sim                  |
| Versão de desenvolvimento em `main`     | Sim                  |
| Versões anteriores sem manutenção ativa | Não garantido        |

## Como reportar uma vulnerabilidade

Se você identificar uma vulnerabilidade de segurança, não abra issue pública.

Use o recurso **Private Vulnerability Reporting** do GitHub neste repositório para enviar o relato de forma privada.

Inclua sempre que possível:

- Pacote(s) afetado(s)
- Versão(ões) impactadas
- Tipo de vulnerabilidade
- Cenário de exploração
- Passos para reprodução
- Impacto esperado
- Possível mitigação, se houver

## O que esperar após o reporte

- Confirmação inicial de recebimento assim que o relato for triado
- Avaliação técnica e de severidade pelo mantenedor
- Definição de plano de correção ou mitigação
- Divulgação coordenada após correção, quando aplicável

O prazo exato pode variar conforme complexidade e disponibilidade, mas relatos válidos serão tratados com prioridade.

## Boas práticas para pesquisadores

- Evite testes destrutivos, indisponibilidade deliberada ou acesso a dados de terceiros.
- Não exponha detalhes publicamente antes da correção.
- Compartilhe evidências suficientes para reprodução segura.

## Escopo

Esta política cobre os pacotes e artefatos publicados a partir deste repositório, incluindo bibliotecas distribuídas via NuGet e o código-fonte em manutenção ativa.
