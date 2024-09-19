using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;
using Wolverine.Runtime;
using Wolverine.Transports;
using Wolverine.Transports.Sending;

namespace Wolverine.GooglePubSub
{
    public class GooglePubSubTopic : GooglePubSubEndpoint, IBrokerEndpoint
    {
        private readonly GooglePubSubTransport _parent;

        public IGooglePubSubEnvelopeMapper Mapper {  get; set; }
        public PublisherClientBuilder Configuration { get; } = new();
        public string TopicName { get; }
        public string ProjectId { get; }

        public GooglePubSubTopic(GooglePubSubTransport parent, string projectId, string topicName) 
            : base(new Uri($"{parent.Protocol}://projects/{projectId}/topics/{topicName}"), EndpointRole.Application)
        {
            ArgumentNullException.ThrowIfNull(parent);

            _parent = parent;
            Mapper = new GooglePubSubEnvelopeMapper(this);
            ProjectId = projectId;
            TopicName = EndpointName = topicName ?? throw new ArgumentNullException(nameof(topicName));
        }

        public override ValueTask<IListener> BuildListenerAsync(IWolverineRuntime runtime, IReceiver receiver)
        {
            throw new NotSupportedException();
        }

        protected override ISender CreateSender(IWolverineRuntime runtime)
        {
            Configuration.TopicName ??= new TopicName(ProjectId, TopicName);
            var publisherClient = Configuration.Build();
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

        public override ValueTask SetupAsync(ILogger logger)
        {
            // todo: check topic exists, create
            throw new NotImplementedException();
        }

        internal GooglePubSubTopicSubscription FindOrCreateSubscription(string subscriptionId, string projectId)
        {
            var existing =
                _parent.Subscriptions.FirstOrDefault(x => x.Id == subscriptionId && x.Topic == this);

            if (existing != null)
            {
                return existing;
            }

            var subscription = new GooglePubSubTopicSubscription(_parent, this, subscriptionId, projectId);
            _parent.Subscriptions.Add(subscription);

            return subscription;
        }
    }
}
