// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S3928:Parameter names used into ArgumentException constructors should match an existing one ", Justification = "O uso de 'nameof(_baseConfiguration)' é correto para identificar o campo sendo validado. O SonarQube não reconhece adequadamente o padrão nameof() com campos privados em contextos de validação.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Services.ServiceBusConfigurationManager.ValidateBaseConfiguration")]

// Supressões para warnings CA1031 em métodos de exemplo
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Método de exemplo demonstra tratamento de exceções genéricas para fins educacionais e logging. Em exemplos é apropriado mostrar captura de Exception para demonstrar diferentes cenários de erro.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Examples.ServiceBusUsageExamples.ExemploComTratamentoErro")]

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Método de exemplo que demonstra cenários de performance com ReuseConnections. A captura de Exception genérica é intencional para mostrar tratamento robusto em loops de processamento e evitar falha completa do exemplo.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Examples.ServiceBusUsageExamples.ExemploReuseConnections(System.Threading.CancellationToken)")]

// Supressões para métodos Dispose da classe ServiceBusMessageSender
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No método DisposeAsync, a captura de Exception genérica é necessária para garantir que o dispose de clientes em cache não falhe silenciosamente. É crítico que o dispose sempre complete, mesmo se algum cliente específico falhar ao ser liberado. O log de warning garante visibilidade de problemas.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Services.ServiceBusMessageSender.DisposeAsync~System.Threading.Tasks.ValueTask")]

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No método DisposeAsync principal, a captura de Exception genérica é necessária para garantir que o objeto seja marcado como disposed mesmo se houver falha na liberação de recursos. Isso evita vazamentos de memória e estados inconsistentes. O log de erro garante que problemas sejam registrados para diagnóstico.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Services.ServiceBusMessageSender.DisposeAsync~System.Threading.Tasks.ValueTask")]

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No método Dispose protegido, a captura de Exception genérica é necessária para garantir que o padrão Dispose seja implementado corretamente. O método deve sempre marcar o objeto como disposed, mesmo se houver falha na liberação de recursos nativos. Isso segue as melhores práticas do padrão Dispose do .NET.", Scope = "member", Target = "~M:Nuuvify.CommonPack.AzureServiceBus.Services.ServiceBusMessageSender.Dispose(System.Boolean)")]
