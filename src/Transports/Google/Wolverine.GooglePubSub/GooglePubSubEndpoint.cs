using Microsoft.Extensions.Logging;
using Wolverine.Configuration;
using Wolverine.Transports;

namespace Wolverine.GooglePubSub
{
    public abstract class GooglePubSubEndpoint : Endpoint, IBrokerEndpoint
    {
        public GooglePubSubEndpoint(Uri uri, EndpointRole role) : base(uri, role)
        {
        }

        public abstract ValueTask<bool> CheckAsync();
        public abstract ValueTask TeardownAsync(ILogger logger);
        public abstract ValueTask SetupAsync(ILogger logger);
    }
}
