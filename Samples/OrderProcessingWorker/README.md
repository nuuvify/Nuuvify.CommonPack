# Order Processing Worker - Exemplo de Background Service

Este é um exemplo de como implementar um Worker Service usando a biblioteca `Nuuvify.CommonPack.BackgroundService`.

## Descrição

O `OrderProcessingBackgroundService` demonstra como criar um serviço de background que processa mensagens do Azure Service Bus para pedidos de e-commerce.

## Funcionalidades

- Processamento de mensagens do Azure Service Bus
- Telemetria com Activity Source
- Tratamento de erros robusto
- Logging estruturado
- Processamento de pedidos com validação, reserva de estoque, pagamento e criação

## Configuração

### appsettings.json

Configure as seguintes seções no `appsettings.json`:

```json
{
  "ServiceBus": {
    "CnnName": "Endpoint=sb://your-servicebus-namespace.servicebus.windows.net/;SharedAccessKeyName=your-key-name;SharedAccessKey=your-key",
    "Topic": {
      "Name": "orders",
      "Subscription": "order-processing"
    }
  }
}
```

### Variáveis de Ambiente (Alternativa)

Você também pode configurar usando variáveis de ambiente:

- `ServiceBus__CnnName`: String de conexão do Service Bus
- `ServiceBus__Topic__Name`: Nome do tópico
- `ServiceBus__Topic__Subscription`: Nome da subscription

## Como Executar

1. Configure a string de conexão do Azure Service Bus no `appsettings.json`
2. Execute o comando:

```bash
dotnet run
```

## Estrutura do Projeto

- `OrderProcessingBackgroundService.cs`: Implementação do serviço de background
- `Program.cs`: Configuração e inicialização do Worker
- `appsettings.json`: Configurações da aplicação

## Modelo de Dados

### OrderMessage

Representa uma mensagem de pedido recebida do Service Bus:

- `OrderId`: Identificador único do pedido
- `CustomerId`: Identificador do cliente
- `TotalAmount`: Valor total do pedido
- `OrderDate`: Data do pedido
- `Items`: Lista de itens do pedido

### OrderItem

Representa um item do pedido:

- `ProductId`: Identificador do produto
- `ProductName`: Nome do produto
- `Quantity`: Quantidade
- `UnitPrice`: Preço unitário

## Telemetria

O serviço utiliza `ActivitySource` para telemetria distribuída, adicionando tags relevantes:

- `order.id`: ID do pedido
- `order.customer_id`: ID do cliente
- `order.total_amount`: Valor total do pedido

## Tratamento de Erros

O serviço está configurado para:

- Abandonar mensagens em caso de falha (`AbandonMessageIfFailed = true`)
- Fazer log de erros detalhados
- Tratar diferentes tipos de exceção de forma apropriada
