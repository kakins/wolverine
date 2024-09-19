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
            //_publisher = PublisherClient.Create(
            //    new TopicName(topic.ProjectId, topic.TopicName),
            //    // TODO: add via configuration delegates?
            //    // TODO: replace with PublisherClientBuilder
            //    new PublisherClient.ClientCreationSettings { },
            //    new PublisherClient.Settings { });
        }

        public async Task<bool> PingAsync()
        {
            try
            {
                //await SendAsync(Envelope.ForPing(Destination));
                //await SendAsync(Envelope.ForPing(Destination));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async ValueTask SendAsync(Envelope envelope)
        {
            var message = _topic.Mapper.CreateMessage(envelope);
            await _publisher.PublishAsync(message);
        }
    }
}
