using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;
using Wolverine.Runtime;
using Wolverine.Transports;
using Wolverine.Transports.Sending;

namespace Wolverine.GooglePubSub
{
    public class GooglePubSubTopicSubscription : GooglePubSubEndpoint
    {
        public IGooglePubSubEnvelopeMapper Mapper { get; set; }
        public SubscriberClientBuilder Configuration { get; } = new();
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public GooglePubSubTransport Parent { get; }
        public GooglePubSubTopic Topic { get; internal set; }

        private bool _hasInitialized;

        public GooglePubSubTopicSubscription(GooglePubSubTransport parent, GooglePubSubTopic topic, string id, string projectId) 
            : base(new Uri($"{parent.Protocol}://projects/{projectId}/subscriptions/{id}"), EndpointRole.Application)
        {
            ArgumentNullException.ThrowIfNull(parent);

            Mode = EndpointMode.Inline;

            Mapper = new GooglePubSubEnvelopeMapper(this);
            Parent = parent;
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

            var client = Parent.SubscriberServiceApiClient;
            //await InitializeAsync(client, logger);

            _hasInitialized = true;
        }

        internal async ValueTask InitializeAsync(SubscriberServiceApiClient client, ILogger logger)
        {
            if (Parent.AutoProvision)
            {
                await SetupAsync(client, logger);
            }
        }

        internal async ValueTask SetupAsync(SubscriberServiceApiClient client, ILogger logger)
        {
            var subscription = await client.GetSubscriptionAsync(new SubscriptionName(ProjectId, Id));
            if (subscription == null) 
            { 
                // TODO: Create subscription
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

        public override ValueTask SetupAsync(ILogger logger)
        {
            // todo: check topic exists, create
            throw new NotImplementedException();
        }
    }
}
