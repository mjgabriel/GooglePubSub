using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.PubSub.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Google.PubSub
{
    public class MessageHandlerInvoker : IInvokeMessageHandler
    {
        private readonly IServiceProvider _serviceProvider;
        
        public MessageHandlerInvoker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task Invoke(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
        {
            var messageType = ResolveType(messageEnvelope.Header.MessageType);
            // we don't have anything defined for this type of message (event)
            // this is how we can ignore events on the subscription that we don't care about,
            // by not having a definition of the event model available in the solution
            if (messageType is null)
                return;

            var openGenericMessageHandlerType = typeof(IHandleMessages<>);
            // Consider caching, in a concurrent dictionary, the result of the next line to avoid reflection on every
            // single message processed, would also need to cache when an IHandleMessage<messageType>
            // does not have an implementation as well and short circuit out of here
            // this will creates a Type object of `IHandleMessage<messageType>`
            var messageHandlerType = openGenericMessageHandlerType.MakeGenericType(messageType);

            // IHandleMessages<> are registered with scoped lifetime
            using var scope = _serviceProvider.CreateScope();
            
            // try to resolve an implementation of messageHandlerType, if not found will be null
            // therefore, no handler defined for the message (event) being processed
            if (scope.ServiceProvider.GetService(messageHandlerType) is IHandleMessages handler)
            {
                await handler.HandleAsync(messageEnvelope, cancellationToken);
            }
        }
        private static System.Type ResolveType(string typeNameString)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Select(assembly => assembly.GetType(typeNameString))
                .SingleOrDefault(type => type != null);
        }
    }
}