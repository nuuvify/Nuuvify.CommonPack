---
description: "Planejar mudança em Nuuvify.CommonPack.BackgroundService com foco em processamento de mensagens, concorrência, cancelamento, lock renewal, diagnóstico e telemetria."
name: "BGS Plan Change"
argument-hint: "Objetivo, fluxo de mensagem, símbolo ou arquivo e risco conhecido"
agent: "plan"
model: "GPT-5 (copilot)"
---

Crie um plano enxuto para a mudança solicitada em `Nuuvify.CommonPack.BackgroundService`.

Foque em:
- fluxo de processamento da mensagem
- concorrência e cancelamento
- lock renewal
- escolha entre abandon e dead letter
- diagnósticos, logs e telemetria

Entregue:
1. ponto do fluxo onde o comportamento é controlado
2. hipótese local de implementação
3. menor conjunto de arquivos a alterar
4. validação mais estreita
5. riscos operacionais e de regressão

Referências:
- [BackgroundService README](../../src/Nuuvify.CommonPack.BackgroundService/README.md)
- [BackgroundService Source Instruction](../instructions/backgroundservice-source.instructions.md)