// <auto-generated/>
#pragma warning disable

namespace Internal.Generated.WolverineHandlers
{
    // START: DataHandler620835457
    public class DataHandler620835457 : Wolverine.Runtime.Handlers.MessageHandler
    {


        public override System.Threading.Tasks.Task HandleAsync(Wolverine.Runtime.MessageContext context, System.Threading.CancellationToken cancellation)
        {
            // The actual message body
            var data = (WolverineWebApi.Data)context.Envelope.Message;

            var dataHandler = new WolverineWebApi.DataHandler();
            
            // The actual message execution
            dataHandler.Handle(data);

            return System.Threading.Tasks.Task.CompletedTask;
        }

    }

    // END: DataHandler620835457
    
    
}

