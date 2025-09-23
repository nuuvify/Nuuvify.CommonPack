# Samples - Exemplos de Uso

Esta pasta contém exemplos práticos de como utilizar os pacotes da biblioteca `Nuuvify.CommonPack`.

## Estrutura

### OrderProcessingWorker

Este exemplo demonstra como implementar um Worker Service que utiliza o `Nuuvify.CommonPack.BackgroundService` para processar mensagens do Azure Service Bus.

**Características:**
- Worker Service completo funcional
- Processamento de mensagens do Azure Service Bus
- Telemetria integrada
- Logging estruturado
- Tratamento de erros robusto

**Como usar:**
1. Navegue até a pasta `OrderProcessingWorker`
2. Configure sua string de conexão do Service Bus no `appsettings.json`
3. Execute com `dotnet run`

Para mais detalhes, consulte o [README específico](./OrderProcessingWorker/README.md) do projeto.

## Contribuindo com Exemplos

Se você desenvolveu um exemplo interessante usando os pacotes da `Nuuvify.CommonPack`, fique à vontade para contribuir adicionando-o aqui seguindo a mesma estrutura:

1. Crie uma nova pasta com o nome do seu exemplo
2. Inclua um projeto funcional completo
3. Adicione documentação clara (README.md)
4. Configure o projeto na solução principal
