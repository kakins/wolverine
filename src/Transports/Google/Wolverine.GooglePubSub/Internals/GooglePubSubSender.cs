using Google.Cloud.PubSub.V1;
using Wolverine.Transports.Sending;

namespace Wolverine.GooglePubSub.Internals
{
    internal class GooglePubSubSender : ISender
    {
        public bool SupportsNativeScheduledSend => false;

        public Uri Destination { get; }

        private readonly GooglePubSubTopic _topic;

        // TODO: How/when to call Shutdown?
        // Do we even need to do this?
        private readonly PublisherClient _publisher;

        public GooglePubSubSender(GooglePubSubTopic topic, PublisherClient publisher)
        {
            Destination = topic.Uri;

            _topic = topic;

            _publisher = publisher;
        }

        public async Task<bool> PingAsync()
        {
            try
            {
                await SendAsync(Envelope.ForPing(Destination));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ValueTask SendAsync(Envelope envelope)
        {
            var message = _topic.Mapper.CreateMessage(envelope);
            var publishTask = _publisher.PublishAsync(message);
            return new ValueTask(publishTask);
        }
    }
}
