// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception is caught for logging and diagnostic purposes, then properly rethrown in HandleGenericExceptionAsync method. This ensures all unhandled exceptions are captured with proper context for troubleshooting while maintaining the original exception flow.", Scope = "member", Target = "~M:Nuuvify.CommonPack.BackgroundService.Services.ServiceBusBackgroundService`1.HandleMessageAsync(Azure.Messaging.ServiceBus.ProcessMessageEventArgs,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
