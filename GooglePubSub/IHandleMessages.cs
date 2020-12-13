using System.Threading;
using System.Threading.Tasks;

namespace GooglePubSub
{
    public interface IHandleMessages
    {
        Task HandleAsync(MessageEnvelope message, CancellationToken cancellationToken = default);
    }

    public interface IHandleMessages<in TMessage> : IHandleMessages where TMessage : class
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}