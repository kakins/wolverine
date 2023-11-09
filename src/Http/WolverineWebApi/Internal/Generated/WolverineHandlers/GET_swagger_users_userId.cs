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
    // START: GET_swagger_users_userId
    public class GET_swagger_users_userId : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;
        private readonly Wolverine.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;
        private readonly Wolverine.Runtime.IWolverineRuntime _wolverineRuntime;

        public GET_swagger_users_userId(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions, Wolverine.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory, Wolverine.Runtime.IWolverineRuntime wolverineRuntime) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
            _outboxedSessionFactory = outboxedSessionFactory;
            _wolverineRuntime = wolverineRuntime;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var messageContext = new Wolverine.Runtime.MessageContext(_wolverineRuntime);
            // Building the Marten session
            await using var querySession = _outboxedSessionFactory.QuerySession(messageContext);
            var userId = (string)httpContext.GetRouteValue("userId");
            
            // The actual HTTP request handler execution
            var userProfile_response = await WolverineWebApi.SwaggerEndpoints.GetUserProfile(userId, querySession).ConfigureAwait(false);

            // Writing the response body to JSON because this was the first 'return variable' in the method signature
            await WriteJsonAsync(httpContext, userProfile_response);
        }

    }

    // END: GET_swagger_users_userId
    
    
}

