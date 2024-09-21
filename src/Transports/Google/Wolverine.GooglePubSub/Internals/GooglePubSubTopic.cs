using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Transports;
using Wolverine.Transports.Sending;

namespace Wolverine.GooglePubSub.Internals;

public class GooglePubSubTopic : GooglePubSubEndpoint, IBrokerEndpoint
{
    private readonly GooglePubSubTransport _parent;
    private bool _hasInitialized;

    public IGooglePubSubEnvelopeMapper Mapper { get; set; }
    public PublisherClientBuilder PublisherConfiguration { get; } = new();
    public Topic TopicConfiguration { get; }
    public TopicName TopicName => TopicConfiguration.TopicName;


    public GooglePubSubTopic(GooglePubSubTransport parent, string projectId, string topicId)
        : base(new Uri($"{parent.Protocol}://projects/{projectId}/topics/{topicId}"), EndpointRole.Application)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(topicId);

        _parent = parent;
        Mapper = new GooglePubSubEnvelopeMapper(this);
        TopicConfiguration = new Topic
        {
            TopicName = new TopicName(projectId, topicId)
        };
    }

    public override ValueTask<IListener> BuildListenerAsync(IWolverineRuntime runtime, IReceiver receiver)
    {
        throw new NotSupportedException();
    }

    protected override ISender CreateSender(IWolverineRuntime runtime)
    {
        PublisherConfiguration.TopicName ??= TopicConfiguration.TopicName;
        var publisherClient = PublisherConfiguration.Build();
        return new GooglePubSubSender(this, publisherClient);
    }

    public override ValueTask<bool> CheckAsync()
    {
        // todo: check if topic exists
        throw new NotImplementedException();
    }

    public override ValueTask TeardownAsync(ILogger logger)
    {
        // todo: delete topic
        throw new NotImplementedException();
    }

    public override async ValueTask InitializeAsync(ILogger logger)
    {
        if (_hasInitialized)
        {
            return;
        }

        if (_parent.AutoProvision)
        {
            await SetupAsync(logger);
        }

        _hasInitialized = true;
    }

    public override async ValueTask SetupAsync(ILogger logger)
    {
        var apiClient = _parent.PublisherServiceApiClient;

        try
        {
            var existingTopic = await apiClient.GetTopicAsync(TopicConfiguration.TopicName, CancellationToken.None);
        }
        catch (Grpc.Core.RpcException ex)
        {
            if (ex.StatusCode != Grpc.Core.StatusCode.NotFound)
                throw;
        }

        try
        {
            await apiClient.CreateTopicAsync(TopicConfiguration, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error trying to initialize topic {TopicId}", TopicConfiguration.TopicName.TopicId);
            throw;
        }
    }

    internal GooglePubSubTopicSubscription FindOrCreateSubscription(string subscriptionId, string projectId)
    {
        var existing =
            _parent.Subscriptions.FirstOrDefault(x => x.SubscriptionName.SubscriptionId == subscriptionId && x.Topic == this);

        if (existing != null)
        {
            return existing;
        }

        var subscription = new GooglePubSubTopicSubscription(_parent, this, subscriptionId, projectId);
        _parent.Subscriptions.Add(subscription);

        return subscription;
    }
}
