// <auto-generated/>
#pragma warning disable
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using Wolverine.Http;

namespace Internal.Generated.WolverineHandlers
{
    // START: GET_fake_hello_async
    public class GET_fake_hello_async : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;

        public GET_fake_hello_async(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var fakeEndpoint = new WolverineWebApi.FakeEndpoint();
            
            // The actual HTTP request handler execution
            var result_of_SayHelloAsync = await fakeEndpoint.SayHelloAsync().ConfigureAwait(false);

            await WriteString(httpContext, result_of_SayHelloAsync);
        }

    }

    // END: GET_fake_hello_async
    
    
}

