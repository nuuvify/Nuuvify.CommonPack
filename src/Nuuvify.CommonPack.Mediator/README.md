<img src="https://github.com/nuuvify/Nuuvify.CommonPack/blob/ed5c88dd485e97115f73dcb636a616af8416e2a8/Images/logonuuvify.jpg" alt="Nuuvify" width="300px" />

# Mediator

Implementation of Mediator pattern for .net

| Package |  Version | Popularity |
| ------- | ----- | ----- |
| `Nuuvify.CommonPack.Mediator` | [![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Mediator.svg)](https://nuget.org/packages/Nuuvify.CommonPack.Mediator) | [![Nuget](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Mediator.svg)](https://nuget.org/packages/Nuuvify.CommonPack.Mediator) |


## Give a Star! ⭐

If this project is useful to you in any way, give me a star to help me maintain the project.

## Samples

You can find complete example projects demonstrating how to use the Mediator in the [`/samples`](./samples) folder.

These include:

- ✅ Basic usage with `Send` and `Publish`
- ✅ Modular application structure
- ✅ Manual and automatic registration of handlers

Feel free to explore and run them to see how the mediator works in different scenarios.

## Getting Started

### Installation

You can install the Mediator package via NuGet Package Manager or the .NET CLI:

```bash
dotnet add package Nuuvify.CommonPack.Mediator
```

#### 1. Define the Request and Notification

```csharp
public class CreateCustomerCommand : IRequest<string>
{
    public string Name { get; set; }
}

public class CustomerCreatedEvent : INotification
{
    public Guid CustomerId { get; }

    public CustomerCreatedEvent(Guid customerId)
    {
        CustomerId = customerId;
    }
}
```

---

#### 2. Implement the Handlers

```csharp
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, string>
{
    private readonly IMediator _mediator;

    public CreateCustomerHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        await _mediator.Publish(new CustomerCreatedEvent(id), cancellationToken);

        return $"Customer '{request.Name}' created with ID {id}";
    }
}

public class SendWelcomeEmailHandler : INotificationHandler<CustomerCreatedEvent>
{
    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending welcome email to customer {notification.CustomerId}");
        return Task.CompletedTask;
    }
}
```

---

#### 3. Register the Handlers (Dependency Injection)

You can register everything manually if you want full control:

```csharp
services.AddMediatoR();
```

---

#### 4. Execute the Flow

```csharp
public class CustomerAppService
{
    private readonly IMediatoR _mediator;

    public CustomerAppService(IMediatoR mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> CreateCustomer(string name)
    {
        return await _mediator.Send(new CreateCustomerCommand { Name = name });
    }
}
```

---

When the `CreateCustomer` method is called:

1. `CreateCustomerHandler` handles the request
2. It creates and persists the customer (simulated)
3. It publishes a `CustomerCreatedEvent`
4. `SendWelcomeEmailHandler` handles the event

This structure cleanly separates **commands** (which change state and return a result) from **notifications** (which communicate to the rest of the system that something happened).


## About

Nuuvify.CommonPack.Mediator was developed by [Lincoln Zocateli](https://zocate.li) under the MIT license.
