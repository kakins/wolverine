using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using JasperFx.Core;
using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;
using Wolverine.Runtime;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub;

public class GooglePubSubTransport : BrokerTransport<GooglePubSubEndpoint>
{
    // TODO: Is there a better way to pass in the project ID?
    public string ProjectId { get; set; }
    private readonly Lazy<PublisherServiceApiClient> _publisherServiceApiClient;
    private readonly Lazy<SubscriberServiceApiClient> _subscriberServiceApiClient;

    public SubscriberServiceApiClient SubscriberServiceApiClient => _subscriberServiceApiClient.Value;
    public PublisherServiceApiClient PublisherServiceApiClient => _publisherServiceApiClient.Value;

    public LightweightCache<string, GooglePubSubTopic> Topics { get; }
    public readonly List<GooglePubSubTopicSubscription> Subscriptions = new();

    public bool SystemTopicsEnabled { get; set; } = true;

    // TODO: This should be what?
    public const string ProtocolName = "pubsub";

    public GooglePubSubTransport() : base(ProtocolName, "Google PubSub")
    {
        Topics = new(name => new GooglePubSubTopic(this, ProjectId, name));
        
        _publisherServiceApiClient = new Lazy<PublisherServiceApiClient>(() =>
        {
            return new PublisherServiceApiClientBuilder()
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            }
            .Build();
        });

        _subscriberServiceApiClient = new Lazy<SubscriberServiceApiClient>(() =>
        {
            return new SubscriberServiceApiClientBuilder()
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
            }
            .Build();
        });

        IdentifierDelimiter = ".";
    }

    public override ValueTask ConnectAsync(IWolverineRuntime runtime)
    {
        // Different clients are used per endpoint
        return ValueTask.CompletedTask;
    }

    public override IEnumerable<PropertyColumn> DiagnosticColumns()
    {
        // TODO: Implement
        return Enumerable.Empty<PropertyColumn>();
    }

    public ValueTask DisposeAsync()
    {
        // TODO: Anything to dispose here?
        return ValueTask.CompletedTask;
    }

    protected override IEnumerable<Endpoint> explicitEndpoints()
    {
        foreach (var topic in Topics) yield return topic;

        foreach (var subscription in Subscriptions) yield return subscription;
    }

    protected override IEnumerable<GooglePubSubEndpoint> endpoints()
    {
        foreach (var topic in Topics) yield return topic;

        foreach (var subscription in Subscriptions) yield return subscription;

        // TODO: consider DLQ topics and associated subscription
    }

    protected override GooglePubSubEndpoint findEndpointByUri(Uri uri)
    {
        var topicName = uri.Segments[3].TrimEnd('/');
        if (uri.Segments.Length == 3)
        {
            var subscription = Subscriptions.FirstOrDefault(x => x.Uri == uri);
            if (subscription != null)
            {
                return subscription;
            }

            // TODO: Get subscriptionId and project id from url segment
            var subscriptionId = uri.Segments.Last().TrimEnd('/');
            var projectId = uri.Segments.Skip(1).First().TrimEnd('/');
            var topic = Topics[topicName];

            subscription = new GooglePubSubTopicSubscription(this, topic, subscriptionId, projectId);
            Subscriptions.Add(subscription);

            return subscription;
        }

        return Topics[topicName];
    }

    protected override void tryBuildSystemEndpoints(IWolverineRuntime runtime)
    {
        if (!SystemTopicsEnabled)
            return;

        // TODO: make topic/subscription config available

        CreateTopicAndSubscription(
            $"wolverine.response.{runtime.DurabilitySettings.AssignedNodeNumber}",
            "GooglePubSubResponses");

        CreateTopicAndSubscription(
            SanitizeIdentifier($"wolverine.retries.{runtime.Options.ServiceName}".ToLower()),
            "GooglePubSubRetries");

        void CreateTopicAndSubscription(string subscriptionId, string endpointName)
        {
            var topic = Topics[subscriptionId];
            topic.Mode = EndpointMode.BufferedInMemory;
            topic.EndpointName = endpointName;
            topic.Role = EndpointRole.System;

            var subscription = Subscriptions.FirstOrDefault(s => s.SubscriptionName.SubscriptionId == subscriptionId);
            if (subscription == null)
            {
                subscription = new GooglePubSubTopicSubscription(this, topic, subscriptionId, ProjectId)
                {
                    Mode = EndpointMode.BufferedInMemory,
                    Role = EndpointRole.System
                };

                // TODO: move into passed in config
                // e.g., configure(subscription.Configuration)
                subscription.SubscriberConfiguration.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;

                Subscriptions.Add(subscription);
            }
        }
    }

    public override Endpoint? ReplyEndpoint()
    {
        // todo: return reply subscription
        return base.ReplyEndpoint();
    }
}
