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
        public SubscriberClientBuilder Configuration { get; } = new();
        public string Id { get; set; }
        public string ProjectId { get; set; }
        private readonly GooglePubSubTransport _parent;
        public GooglePubSubTopic Topic { get; internal set; }

        private bool _hasInitialized;

        public GooglePubSubTopicSubscription(GooglePubSubTransport parent, GooglePubSubTopic topic, string id, string projectId)
            : base(new Uri($"{parent.Protocol}://projects/{projectId}/subscriptions/{id}"), EndpointRole.Application)
        {
            IsListener = true;

            ArgumentNullException.ThrowIfNull(parent);

            Mode = EndpointMode.Inline;

            Mapper = new GooglePubSubEnvelopeMapper(this);
            _parent = parent;
            Topic = topic;
            Id = id;
            ProjectId = projectId;
        }

        public async override ValueTask<IListener> BuildListenerAsync(IWolverineRuntime runtime, IReceiver receiver)
        {
            Configuration.SubscriptionName ??= new SubscriptionName(ProjectId, Id);
            var subscriberClient = await Configuration.BuildAsync();
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

            var subscriptionName = new SubscriptionName(ProjectId, Id);

            try
            {
                var existingTopic = await apiClient.GetSubscriptionAsync(subscriptionName, CancellationToken.None);
            }
            catch (Grpc.Core.RpcException ex)
            {
                if (ex.StatusCode != Grpc.Core.StatusCode.NotFound)
                    throw;
            }

            try
            {
                await apiClient.CreateSubscriptionAsync(subscriptionName, new TopicName(ProjectId, Topic.TopicName), pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error trying to initialize subscription {Id}", Id);
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
