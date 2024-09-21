using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Wolverine.Configuration;

namespace Wolverine.GooglePubSub
{
    public static class GooglePubSubTransportExtensions
    {
        internal static GooglePubSubTransport GooglePubSubTransport(this WolverineOptions endpoints)
        {
            var transports = endpoints.As<WolverineOptions>().Transports;

            return transports.GetOrCreate<GooglePubSubTransport>();
        }

        public static GooglePubSubTransportExpression UseGooglePubSub(this WolverineOptions options, string projectId)
        {
            var transport = options.GooglePubSubTransport();
            transport.ProjectId = projectId;
            return new GooglePubSubTransportExpression(transport, options);
        }

        public static GooglePubSubTopicSubscriberConfiguration ToGooglePubSubTopic(
            this IPublishToExpression publishing, 
            string topicName,
            Action<Topic> configureTopic = null)
        {
            var transports = publishing.As<PublishingExpression>().Parent.Transports;
            var transport = transports.GetOrCreate<GooglePubSubTransport>();

            var topic = transport.Topics[topicName];
            configureTopic?.Invoke(topic.TopicConfiguration);
            
            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(topic.Uri);

            return new GooglePubSubTopicSubscriberConfiguration(topic);
        }

        public static SubscriptionExpression ListenToGooglePubSubTopicSubscription(
            this WolverineOptions endpoints,
            string projectId,
            string subscriptionId,
            Action<SubscriberClientBuilder> configureSubscriber = null)
        {
            if (subscriptionId == null)
            {
                throw new ArgumentNullException(nameof(subscriptionId));
            }

            var transport = endpoints.GooglePubSubTransport();

            return new SubscriptionExpression(
                projectId,
                transport.MaybeCorrectName(subscriptionId),
                configureSubscriber,
                transport);
        }

        public class SubscriptionExpression
        {
            private readonly string _projectId;
            private readonly string _subscriptionId;
            private readonly Action<SubscriberClientBuilder> _configureSubscriber;
            private readonly GooglePubSubTransport _transport;

            public SubscriptionExpression(
                string projectId,
                string subscriptionId,
                Action<SubscriberClientBuilder> configureSubscription,
                GooglePubSubTransport transport)
            {
                _projectId = projectId;
                _subscriptionId = subscriptionId;
                _configureSubscriber = configureSubscription;
                _transport = transport;
            }

            public GooglePubSubListenerConfiguration FromTopic(string topicName, Action<PublisherClientBuilder> configureClient = null)
            {
                if (topicName == null)
                {
                    throw new ArgumentNullException(nameof(topicName));
                }

                // Gather any naming prefix
                topicName = _transport.MaybeCorrectName(topicName);

                var topic = _transport.Topics[topicName];
                configureClient?.Invoke(topic.ClientConfiguration);

                var subscription = topic.FindOrCreateSubscription(_subscriptionId, _projectId);
                subscription.IsListener = true;
                _configureSubscriber?.Invoke(subscription.Configuration);

                return new GooglePubSubListenerConfiguration(subscription);
            }
        }

        internal static Envelope CreateEnvelope(this IGooglePubSubEnvelopeMapper mapper, PubsubMessage message)
        {
            var envelope = new Envelope { Data = [.. message.Data] };
            mapper.MapIncomingToEnvelope(envelope, message);
            return envelope;
        }

        internal static PubsubMessage CreateMessage(this IGooglePubSubEnvelopeMapper mapper, Envelope envelope)
        {
            var message = new PubsubMessage
            {
                MessageId = envelope.Id.ToString(),
                Data = ByteString.CopyFrom(envelope.Data),
            };

            mapper.MapEnvelopeToOutgoing(envelope, message);

            return message;
        }
    }
}
