using Google.Cloud.PubSub.V1;
using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;

namespace Wolverine.GooglePubSub
{
    public class GooglePubSubTopicSubscriberConfiguration : SubscriberConfiguration<
        GooglePubSubTopicSubscriberConfiguration, 
        GooglePubSubTopic>
    {
        public GooglePubSubTopicSubscriberConfiguration(GooglePubSubTopic endpoint) : base(endpoint)
        {
            
        }

        public GooglePubSubTopicSubscriberConfiguration ConfigureClient(Action<PublisherClientBuilder> configure)
        {
            add(e => configure(e.ClientConfiguration));
            return this;
        }
    }

    public class GooglePubSubTopicSubscriptionListenerConfiguration : ListenerConfiguration<GooglePubSubTopicSubscriptionListenerConfiguration, GooglePubSubTopicSubscription>
    {
        public GooglePubSubTopicSubscriptionListenerConfiguration(GooglePubSubTopicSubscription endpoint) : base(endpoint)
        {
        }

        //public GooglePubSubTopicSubscriptionListenerConfiguration RequireSessions(int? listenerCount = null)
        //{
        //    add(e =>
        //    {
        //        e.Options.RequiresSession = true;
        //        if (listenerCount.HasValue)
        //        {
        //            e.ListenerCount = listenerCount.Value;
        //        }
        //    });

        //    return this;
        //}
    }
}
