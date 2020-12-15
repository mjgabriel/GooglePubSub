using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var messageType = ResolveType(messageEnvelope.MessageType);
            // we don't have anything defined for this type of message(event)
            if (messageType is null)
                return;

            var openGenericMessageHandlerType = typeof(IHandleMessages<>);
            var messageHandlerType = openGenericMessageHandlerType.MakeGenericType(messageType);

            using var scope = _serviceProvider.CreateScope();
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