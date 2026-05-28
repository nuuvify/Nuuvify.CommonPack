# Playbooks de manutenção

## Criar ou alterar código

1. Localize o pacote dono do comportamento em `src/`.
2. Leia o ponto de entrada local, um teste vizinho e o `README.md` do pacote se houver contrato público.
3. Implemente a menor mudança funcional.
4. Atualize ou crie o teste mais próximo.
5. Atualize `README.md` ou `CHANGELOG.md` do pacote apenas se o consumidor externo perceber a mudança.

## Refatorar

1. Garanta evidência do comportamento atual por teste ou cenário reproduzível.
2. Faça extrações e renomeações pequenas.
3. Revalide a cada fatia.
4. Só promova utilitários compartilhados quando houver semântica estável e dono claro.

## Testar

1. Classifique como `Unit` ou `Integration`.
2. Cubra cenário feliz, borda relevante e regressão.
3. Use Moq e Bogus apenas para reduzir ruído, não para esconder comportamento.
4. Prefira validar contrato observável.

## Revisar

1. Foque em bugs, regressões e quebras de API pública.
2. Verifique se a mudança respeita fronteiras entre pacotes.
3. Confirme se os testes realmente cobrem a mudança.
4. Confirme atualização documental quando o comportamento público mudou.

## Documentar

1. Explique o que mudou do ponto de vista do consumidor.
2. Dê um exemplo curto quando isso reduzir ambiguidade.
3. Registre migração quando houver quebra.
4. Remova texto promocional ou redundante.
