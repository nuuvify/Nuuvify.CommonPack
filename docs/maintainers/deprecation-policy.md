# Política de depreciação

Mudanças de API pública devem priorizar previsibilidade para consumidores dos pacotes.

## Regras gerais

- Evite remover ou alterar contratos públicos sem aviso prévio.
- Quando possível, marque APIs antigas como obsoletas antes da remoção.
- Documente alternativas de migração.
- Registre a mudança no changelog correspondente.

## Fluxo recomendado

1. Introduza a alternativa suportada.
2. Marque a API antiga como obsoleta com mensagem clara.
3. Atualize README, changelog e documentação impactada.
4. Aguarde pelo menos um ciclo de versão compatível antes de remover a API, salvo em correções críticas.

## Breaking changes

Toda breaking change deve:

- ser identificada explicitamente no changelog
- aparecer na descrição do pull request
- ser comunicada com racional técnico
- incluir orientação de migração quando aplicável

## Exceções

Correções de segurança, bugs graves ou inconsistências críticas podem exigir remoção ou ajuste mais rápido. Nesses casos, a documentação deve deixar o motivo explícito.
