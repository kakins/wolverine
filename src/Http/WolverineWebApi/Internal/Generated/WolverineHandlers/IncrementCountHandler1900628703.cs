// <auto-generated/>
#pragma warning disable
using WolverineWebApi.WebSockets;

namespace Internal.Generated.WolverineHandlers
{
    // START: IncrementCountHandler1900628703
    public class IncrementCountHandler1900628703 : Wolverine.Runtime.Handlers.MessageHandler
    {
        private readonly WolverineWebApi.WebSockets.Broadcaster _broadcaster;

        public IncrementCountHandler1900628703(WolverineWebApi.WebSockets.Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }



        public override async System.Threading.Tasks.Task HandleAsync(Wolverine.Runtime.MessageContext context, System.Threading.CancellationToken cancellation)
        {
            // The actual message body
            var incrementCount = (WolverineWebApi.WebSockets.IncrementCount)context.Envelope.Message;

            
            // The actual message execution
            var outgoing1 = WolverineWebApi.WebSockets.SomeUpdateHandler.Handle(incrementCount);

            await _broadcaster.Post(outgoing1).ConfigureAwait(false);
        }

    }

    // END: IncrementCountHandler1900628703
    
    
}

