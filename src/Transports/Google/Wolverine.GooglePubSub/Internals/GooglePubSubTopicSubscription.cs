using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Transports;
using Wolverine.Transports.Sending;

namespace Wolverine.GooglePubSub.Internals
{
    public class GooglePubSubTopicSubscription : GooglePubSubEndpoint
    {
        public IGooglePubSubEnvelopeMapper Mapper { get; set; }
        public SubscriberClientBuilder SubscriberConfiguration { get; } = new();
        public Subscription SubscriptionConfiguration { get; } = new();
        public SubscriptionName SubscriptionName => SubscriptionConfiguration.SubscriptionName;
        private readonly GooglePubSubTransport _parent;
        public GooglePubSubTopic Topic { get; internal set; }

        private bool _hasInitialized;

        public GooglePubSubTopicSubscription(GooglePubSubTransport parent, GooglePubSubTopic topic, string id, string projectId)
            : base(new Uri($"{parent.Protocol}://projects/{projectId}/subscriptions/{id}"), EndpointRole.Application)
        {
            IsListener = true;

            ArgumentNullException.ThrowIfNull(parent);
            _parent = parent;

            Mode = EndpointMode.Inline;

            Mapper = new GooglePubSubEnvelopeMapper(this);

            Topic = topic;
            SubscriptionConfiguration = new Subscription()
            {
                TopicAsTopicName = topic.TopicName,
                SubscriptionName = new SubscriptionName(projectId, id)
            };
        }

        public async override ValueTask<IListener> BuildListenerAsync(IWolverineRuntime runtime, IReceiver receiver)
        {
            SubscriberConfiguration.SubscriptionName ??= SubscriptionName;
            var subscriberClient = await SubscriberConfiguration.BuildAsync();
            var listener = new GooglePubSubListener(this, receiver, subscriberClient);
            return listener;
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
            var apiClient = _parent.SubscriberServiceApiClient;

            try
            {
                var existingSubscription = await apiClient.GetSubscriptionAsync(SubscriptionName, CancellationToken.None);
            }
            catch (Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode != Grpc.Core.StatusCode.NotFound)
                    throw;
            }

            try
            {
                await apiClient.CreateSubscriptionAsync(SubscriptionConfiguration);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error trying to initialize subscription {Id}", SubscriptionName.SubscriptionId);
                throw;
            }
        }


        protected override ISender CreateSender(IWolverineRuntime runtime)
        {
            throw new NotSupportedException();
        }

        public override ValueTask<bool> CheckAsync()
        {
            // todo: check if subscription exists
            throw new NotImplementedException();
        }

        public override ValueTask TeardownAsync(ILogger logger)
        {
            // todo: delete subscription
            throw new NotImplementedException();
        }
    }
}
