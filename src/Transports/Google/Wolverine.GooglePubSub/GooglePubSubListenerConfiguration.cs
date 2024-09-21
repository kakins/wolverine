using Wolverine.Configuration;
using Wolverine.GooglePubSub.Internals;

namespace Wolverine.GooglePubSub
{
    public class GooglePubSubListenerConfiguration : ListenerConfiguration<GooglePubSubListenerConfiguration, GooglePubSubTopicSubscription>
    {
        public GooglePubSubListenerConfiguration(GooglePubSubTopicSubscription endpoint) : base(endpoint)
        {
        }
    }
}
