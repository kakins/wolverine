using System.Runtime.CompilerServices;
using Oakton;
using Wolverine.Attributes;

[assembly: JasperFx.Core.TypeScanning.IgnoreAssembly]
[assembly: OaktonCommandAssembly]
[assembly: WolverineFeature]

[assembly: InternalsVisibleTo("CoreTests")]
[assembly: InternalsVisibleTo("DiagnosticsTests")]
[assembly: InternalsVisibleTo("PolicyTests")]
[assembly: InternalsVisibleTo("CircuitBreakingTests")]
[assembly: InternalsVisibleTo("Wolverine.ComplianceTests")]
[assembly: InternalsVisibleTo("Wolverine.RabbitMq")]
[assembly: InternalsVisibleTo("Wolverine.RabbitMq.Tests")]
[assembly: InternalsVisibleTo("Wolverine.AzureServiceBus")]
[assembly: InternalsVisibleTo("Wolverine.ConfluentKafka")]
[assembly: InternalsVisibleTo("Wolverine.AzureServiceBus.Tests")]
[assembly: InternalsVisibleTo("Wolverine.GooglePubSub")]
[assembly: InternalsVisibleTo("PersistenceTests")]
[assembly: InternalsVisibleTo("EfCoreTests")]
[assembly: InternalsVisibleTo("MartenTests")]
[assembly: InternalsVisibleTo("PostgresqlTests")]
[assembly: InternalsVisibleTo("SqlServerTests")]
[assembly: InternalsVisibleTo("ScheduledJobTests")]
[assembly: InternalsVisibleTo("Wolverine.RDBMS")]
[assembly: InternalsVisibleTo("Wolverine.Marten")]
[assembly: InternalsVisibleTo("Wolverine.EntityFrameworkCore")]
[assembly: InternalsVisibleTo("Wolverine.Pulsar")]
[assembly: InternalsVisibleTo("Wolverine.Pulsar.Tests")]
[assembly: InternalsVisibleTo("MassTransitInteropTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Wolverine.Http")]
[assembly: InternalsVisibleTo("Wolverine.Http.Tests")]