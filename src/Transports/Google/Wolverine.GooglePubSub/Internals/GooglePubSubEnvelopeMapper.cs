using Google.Cloud.PubSub.V1;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub.Internals
{
    internal class GooglePubSubEnvelopeMapper : EnvelopeMapper<PubsubMessage, PubsubMessage>, IGooglePubSubEnvelopeMapper
    {
        public GooglePubSubEnvelopeMapper(Endpoint endpoint) : base(endpoint)
        {
        }

        protected override void writeOutgoingHeader(PubsubMessage outgoing, string key, string value)
        {
            outgoing.Attributes[key] = value;
        }

        protected override void writeIncomingHeaders(PubsubMessage incoming, Envelope envelope)
        {
            if (incoming.Attributes == null) return;

            foreach (var pair in incoming.Attributes)
                envelope.Headers[pair.Key] = pair.Value?.ToString();
        }

        protected override bool tryReadIncomingHeader(PubsubMessage incoming, string key, out string? value)
        {
            if (incoming.Attributes.TryGetValue(key, out var header))
            {
                value = header.ToString();
                return true;
            }

            value = default;
            return false;
        }
    }
}
