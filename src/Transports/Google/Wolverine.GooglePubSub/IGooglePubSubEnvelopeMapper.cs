using Google.Cloud.PubSub.V1;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub
{
    public interface IGooglePubSubEnvelopeMapper : IEnvelopeMapper<PubsubMessage, PubsubMessage>
    {

    }
}
