using System.Threading;
using System.Threading.Tasks;

namespace GooglePubSub
{
    public interface IInvokeMessageHandler
    {
        Task Invoke(MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);
    }
}