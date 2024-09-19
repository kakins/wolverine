using Wolverine.GooglePubSub.Internals;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub
{
    // Note: the same as AzureServiceBusConfiguration or KafkaTransportExpression
    public class GooglePubSubTransportExpression : BrokerExpression<GooglePubSubTransport, GooglePubSubTopicSubscription, GooglePubSubTopic, GooglePubSubListenerConfiguration, GooglePubSubTopicSubscriberConfiguration, GooglePubSubTransportExpression>
    {
        public GooglePubSubTransportExpression(GooglePubSubTransport transport, WolverineOptions options) : base(transport, options)
        {
        }

        protected override GooglePubSubListenerConfiguration createListenerExpression(GooglePubSubTopicSubscription subscription)
        {
            return new GooglePubSubListenerConfiguration(subscription);
        }

        protected override GooglePubSubTopicSubscriberConfiguration createSubscriberExpression(GooglePubSubTopic topic)
        {
            return new GooglePubSubTopicSubscriberConfiguration(topic);
        }
    }
}
