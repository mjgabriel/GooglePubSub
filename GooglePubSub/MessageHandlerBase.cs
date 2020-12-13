using System.Threading;
using System.Threading.Tasks;

namespace GooglePubSub
{
    public abstract class MessageHandlerBase<TMessage> : IHandleMessages<TMessage> where TMessage : class
    {
        public Task HandleAsync(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default) =>
            HandleAsync(messageEnvelope.BodyAs<TMessage>(), cancellationToken);

        public abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}