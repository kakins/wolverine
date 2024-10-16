// <auto-generated/>
#pragma warning disable
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using Wolverine.Http;
using Wolverine.Marten.Publishing;
using Wolverine.Runtime;

namespace Internal.Generated.WolverineHandlers
{
    // START: POST_orders_id_confirm
    public class POST_orders_id_confirm : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;
        private readonly Wolverine.Runtime.IWolverineRuntime _wolverineRuntime;
        private readonly Wolverine.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;

        public POST_orders_id_confirm(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions, Wolverine.Runtime.IWolverineRuntime wolverineRuntime, Wolverine.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
            _wolverineRuntime = wolverineRuntime;
            _outboxedSessionFactory = outboxedSessionFactory;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var messageContext = new Wolverine.Runtime.MessageContext(_wolverineRuntime);
            // Reading the request body via JSON deserialization
            var (command, jsonContinue) = await ReadJsonAsync<WolverineWebApi.Marten.ConfirmOrder>(httpContext);
            if (jsonContinue == Wolverine.HandlerContinuation.Stop) return;
            await using var documentSession = _outboxedSessionFactory.OpenSession(messageContext);
            var eventStore = documentSession.Events;
            
            // Loading Marten aggregate
            var eventStream = await eventStore.FetchForWriting<WolverineWebApi.Marten.Order>(command.OrderId, httpContext.RequestAborted).ConfigureAwait(false);

            if (!System.Guid.TryParse((string)httpContext.GetRouteValue("id"), out var id))
            {
                httpContext.Response.StatusCode = 404;
                return;
            }


            
            // The actual HTTP request handler execution
            (var acceptResponse_response, var events) = WolverineWebApi.Marten.MarkItemEndpoint.Confirm(command, eventStream.Aggregate);

            if (events != null)
            {
                
                // Capturing any possible events returned from the command handlers
                eventStream.AppendMany(events);

            }

            // This response type customizes the HTTP response
            ApplyHttpAware(acceptResponse_response, httpContext);
            await documentSession.SaveChangesAsync(httpContext.RequestAborted).ConfigureAwait(false);
            // Writing the response body to JSON because this was the first 'return variable' in the method signature
            await WriteJsonAsync(httpContext, acceptResponse_response);
        }

    }

    // END: POST_orders_id_confirm
    
    
}

